using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Types;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static Combat;

public class RuntimeCharacter : RuntimeStats
{
    public readonly Character BasedOfCharacter;
    public delegate void SimpleFunc();

    private float _actionValue;
    public int Id;
    public int Position;
    public Team Team;
    public CharacterEvents Events;
    public RuntimeShields Shields;
    public RuntimeEffects Effects { get => _effects; }
    public bool IsWeaknessBroken;
    public event SimpleFunc OnDeath;
    private RuntimeEffects _effects;
    private float _turnPercentage;

    public override float Energy { get => base.Energy; set { base.Energy = value; Invoke(Triggers.EnergyChange); } }
    public string Name { get => BasedOfCharacter.name; }

    public float ActionValue { get => _actionValue; set { _actionValue = MathF.Max(instance.CurrentActionValue + .01f, value); instance.SortActionOrder(); } }

    public float TurnCooldown { get => 10000 / Final.SPD; }

    public RuntimeCharacter(Character character, float actionValue, Team team, int position, int id) : base(character)
    {
        BasedOfCharacter = character;
        ActionValue = actionValue;
        DoTurn();
        Team = team;
        Position = position;
        Events = new CharacterEvents();
        _effects = new RuntimeEffects(this);
        Shields = new RuntimeShields();
        Id = id;
        Events[Triggers.AfterTakingDamage].Add(AfterTakingDamage);
        Events[Triggers.OnTurnStart].Add(StartOfTurn);
    }

    public void StartOfTurn(RuntimeCharacter receiver, RuntimeCharacter cause)
    {
        if (Adv.Toughness <= 0)
        {
            Adv.Toughness = AdvancedStats.Toughness;
            IsWeaknessBroken = false;
        }
    }

    public void DoTurn()
    {
        ActionValue += TurnCooldown;
        _turnPercentage = 0;
    }

    public void UpdateTurnPercentage(float diff, float current)
    {
        _turnPercentage += diff / (ActionValue - current);
    }

    public override void CalculateFinalSPD()
    {
        base.CalculateFinalSPD();
        float remainingActionValue = ActionValue - instance.CurrentActionValue;
        float diff = (TurnCooldown * (1 - _turnPercentage)) - remainingActionValue;
        ActionValue += diff;
    }

    public void ActionAdvanceForward(float per)
    {
        ActionValue -= TurnCooldown * per;
    }

    public void Invoke(Triggers trigger, RuntimeCharacter cause = null)
    {
        if (cause == null) cause = this;
        if (Events[trigger].Mechanics.Count > 0) AddTurnInfo($"\n{Name}({Enum.GetName(typeof(Triggers), trigger)})");
        Events.Invoke(trigger, this, cause);
        Team.Events.Invoke(trigger, this, cause);
    }

    public float DoDamageToShield(float amount)
    {
        float excessDMG = amount;
        RuntimeEffect[] effects = Shields.Keys.ToArray();
        for(int i = 0; i < effects.Length; i++)
        {
            RuntimeEffect key = effects[i];
            if (Shields[key] > amount) excessDMG = 0;
            Shields[key] -= amount;
            if (Shields[key] < 0)
            {
                excessDMG = MathF.Min(excessDMG, -Shields[key]);
                key.RemoveSelf();
            }
        }
        return excessDMG;
    }

    public void AfterTakingDamage(RuntimeCharacter receiver, RuntimeCharacter cause)
    {
        if (CurrentHP <= 0)
        {
            Events[Triggers.OnDeath].Invoke(receiver, cause);
            OnDeath.Invoke();
        }
    }

    //Not sure why I have to fill in the event into the function itself, but it doesn't work otherwise
    public void AddPassives()
    {
        if (BasedOfCharacter.Talent != null) Combat.AddPassives(BasedOfCharacter.Talent.Passives, this, OnDeath);
    }

    public string[] CharacterInfo()
    {
        string header = BasedOfCharacter.name;

        string body = "";
        body += "STATS:";
        foreach(Stats stat in Enum.GetValues(typeof(Stats)))
        {
            body += $"[{TypesUtility.GetName(stat)}:{GetStat(stat)}], ";
        }
        float highestShield = 0;
        foreach (float value in Shields.Values) highestShield = Mathf.Max(highestShield, value);
        body += $"[Shield:{highestShield}]";
        body += "\n EFFECTS:";
        foreach(RuntimeEffect effect in Effects.Values)
        {
            body += effect.Info();
        }
        return new string[] { header, body};
    }

    public class RuntimeEffects : Dictionary<KeyValuePair<Effect, int>, RuntimeEffect>
    {
        private readonly RuntimeCharacter _character;

        public RuntimeEffects(RuntimeCharacter character)
        {
            _character = character;
        }

        public void Add(Effect effect, RuntimeCharacter cause)
        {
            if (!effect.CanStackWithSameType) cause.Id = -1;
            KeyValuePair<Effect, int> key = new KeyValuePair<Effect, int>(effect, cause.Id);
            if (ContainsKey(key))
            {
                this[key].Apply();
            }
            else Add(key, new RuntimeEffect(effect, cause, _character, Remove));
        }

        public RuntimeEffect HasEffect(Effect effect)
        {
            foreach(KeyValuePair<KeyValuePair<Effect, int>, RuntimeEffect> runtime in this)
            {
                if (runtime.Key.Key == effect) return runtime.Value;
            }
            return null;
        }
    }

    public class RuntimeShields : Dictionary<RuntimeEffect, float>
    {
        public new void Add(RuntimeEffect effect, float amount)
        {
            if (ContainsKey(effect))
            {
                this[effect] = amount;
            }
            else base.Add(effect, amount);
        }
    }
}
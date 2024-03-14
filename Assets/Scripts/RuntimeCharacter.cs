using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Types;
using UnityEngine;
using static Combat;

public class RuntimeCharacter : RuntimeStats
{
    public readonly Character BasedOfCharacter;

    public float ActionValue;
    public int TeamId;
    public int Position;
    public Team Team;
    public CharacterEvents Events;
    public RuntimeShields Shields;
    public RuntimeEffects Effects { get => _effects; }
    private RuntimeEffects _effects;
    private float _turnPercentage;

    public override float Energy { get => base.Energy; set { base.Energy = value; Invoke(Triggers.EnergyChange); } }

    public RuntimeCharacter(Character character, float actionValue, int teamId, Team team, int position) : base(character)
    {
        BasedOfCharacter = character;
        ActionValue = actionValue;
        DoTurn();
        TeamId = teamId;
        Team = team;
        Position = position;
        Events = new CharacterEvents();
        _effects = new RuntimeEffects(this);
        Shields = new RuntimeShields();
    }

    public void DoTurn()
    {
        ActionValue += 10000 / Final.SPD;
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
        float diff = (10000 / Final.SPD * (1 - _turnPercentage)) - remainingActionValue;
        ActionValue += diff;
        ActionValue = MathF.Max(instance.CurrentActionValue, ActionValue);
    }

    public void Invoke(Triggers trigger, RuntimeCharacter cause = null)
    {
        if (cause == null) cause = this;
        //Debug.Log($"Invoking {Events[trigger].Mechanics.Count} mechanics. Trigger: {Enum.GetName(typeof(Triggers), trigger)} from:{BasedOfCharacter.name}");
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

    public string[] CharacterInfo()
    {
        string header = BasedOfCharacter.name;

        string body = "";
        body += "STATS:";
        foreach(Stats stat in Enum.GetValues(typeof(Stats)))
        {
            body += $"[{EnumName(stat)}:{GetStat(stat)}], ";
        }
        float highestShield = 0;
        foreach (float value in Shields.Values) highestShield = Mathf.Max(highestShield, value);
        body += $"[Shield:{highestShield}]";
        body += "\n EFFECTS:";
        foreach(RuntimeEffect effect in Effects.Values)
        {
            body += $"[{effect.Base.name},{EnumName(effect.Base.StatToBuff)}:[Flat:{effect.Base.FlatIncrease}],[Duration:{effect.Duration}]], ";
        }
        return new string[] { header, body};
    }

    public string EnumName(Stats stat)
    {
        return Enum.GetName(typeof(Stats), stat);
    }

    public class RuntimeEffects : Dictionary<Effect, RuntimeEffect>
    {
        private readonly RuntimeCharacter _character;

        public RuntimeEffects(RuntimeCharacter character)
        {
            _character = character;
        }

        public void Add(Effect effect)
        {
            if (ContainsKey(effect))
            {
                this[effect].Apply();
            }
            else Add(effect, new RuntimeEffect(effect, _character, Remove));
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
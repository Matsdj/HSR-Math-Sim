using System;
using System.Collections;
using System.Collections.Generic;
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

    public RuntimeEffects Effects { get => _effects; }
    private RuntimeEffects _effects;

    public RuntimeCharacter(Character character, float actionValue, int teamId, Team team, int position) : base(character)
    {
        BasedOfCharacter = character;
        ActionValue = actionValue;
        DoTurn();
        TeamId = teamId;
        Team = team;
        Position = position;
        Events = new CharacterEvents();
    }

    public float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.None:
                //Debug.LogError("None is not a valid Stat");
                return 0;
            case Stats.HP:
                return Final.HP;
            case Stats.ATK:
                return Final.ATK;
            case Stats.DEF:
                return Final.DEF;
            case Stats.SPD:
                return Final.SPD;
            case Stats.CRIT_Rate:
                return Adv.CRIT_Rate;
            case Stats.CRIT_DMG:
                return Adv.CRIT_DMG;
            case Stats.Break_Effect:
                return Adv.BreakEffect;
            case Stats.Outgoing_Healing_Boost:
                return Adv.OutgoingHealingBoost;
            case Stats.Max_Energy:
                return Adv.MaxEnergy;
            case Stats.Energy_Regeneration_Rate:
                return Adv.EnergyRegenerationRate;
            case Stats.Effect_Hit_Rate:
                return Adv.EffectHitRate;
            case Stats.Effect_RES:
                return Adv.EffectRES;
            case Stats.CurrentHP:
                return CurrentHP;
            case Stats.MissingHP:
                return Final.HP - CurrentHP;
        }
        Debug.LogError("Unimplemented Stat");
        return 0;
    }

    public void DoTurn()
    {
        ActionValue += 10000 / Final.SPD;
    }

    public void Invoke(Triggers trigger, RuntimeCharacter cause = null)
    {
        if (cause == null) cause = this;
        Events.Invoke(trigger, this, cause);
        Team.Events.Invoke(trigger, this, cause);
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
}

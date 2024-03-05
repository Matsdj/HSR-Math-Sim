using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class RuntimeCharacter : RuntimeStats
{
    public readonly Character BasedOfCharacter;
    public List<Effect> Buffs = new List<Effect>();

    public Dictionary<Triggers, List<Action<RuntimeCharacter>>> Events;
    public override float Energy { get => base.Energy; set { base.Energy = value; Invoke(Triggers.EnergyChange); } }

    public RuntimeCharacter(Character basedOfCharacter) : base(basedOfCharacter)
    {
        BasedOfCharacter = basedOfCharacter;

        Triggers[] triggers = (Triggers[])Enum.GetValues(typeof(Triggers));
        Events = new Dictionary<Triggers, List<Action<RuntimeCharacter>>>();
        foreach (Triggers trigger in triggers)
        {
            Events.Add(trigger, new List<Action<RuntimeCharacter>>());
        }
    }

    public float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.None:
                Debug.LogError("None is not a valid Stat");
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

    public void Invoke(Triggers trigger)
    {
        foreach(Action<RuntimeCharacter> action in Events[trigger])
        {
            action.Invoke(this);
        }
    }
}

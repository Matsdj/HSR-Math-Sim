using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class RuntimeCharacter
{
    public Character BasedOfCharacter;
    public RuntimeStats Stats;
    public List<Effect> Buffs = new List<Effect>();

    public Dictionary<Triggers, List<Action<RuntimeCharacter>>> Events;

    public RuntimeCharacter(Character basedOfCharacter)
    {
        BasedOfCharacter = basedOfCharacter;
        Stats = new RuntimeStats(this);

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
            case Types.Stats.None:
                Debug.LogError("None is not a valid Stat");
                return 0;
            case Types.Stats.HP:
                return Stats.Final.HP;
            case Types.Stats.ATK:
                return Stats.Final.ATK;
            case Types.Stats.DEF:
                return Stats.Final.DEF;
            case Types.Stats.SPD:
                return Stats.Final.SPD;
            case Types.Stats.CRIT_Rate:
                return Stats.Adv.CRIT_Rate;
            case Types.Stats.CRIT_DMG:
                return Stats.Adv.CRIT_DMG;
            case Types.Stats.Break_Effect:
                return Stats.Adv.BreakEffect;
            case Types.Stats.Outgoing_Healing_Boost:
                return Stats.Adv.OutgoingHealingBoost;
            case Types.Stats.Max_Energy:
                return Stats.Adv.MaxEnergy;
            case Types.Stats.Energy_Regeneration_Rate:
                return Stats.Adv.EnergyRegenerationRate;
            case Types.Stats.Effect_Hit_Rate:
                return Stats.Adv.EffectHitRate;
            case Types.Stats.Effect_RES:
                return Stats.Adv.EffectRES;
            case Types.Stats.CurrentHP:
                return Stats.CurrentHP;
            case Types.Stats.MissingHP:
                return Stats.Final.HP - Stats.CurrentHP;
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    public enum Stats
    {
        None,
        HP, //This is max
        ATK,
        DEF,
        SPD,
        CRIT_Rate,
        CRIT_DMG,
        Break_Effect,
        Outgoing_Healing_Boost,
        Max_Energy,
        Energy_Regeneration_Rate,
        Effect_Hit_Rate,
        Effect_RES,
        CurrentHP,
        MissingHP,
        Toughness,
        LVLMultiplier,
        MaxToughnessMultiplier,
        Weaken,
        DEF_PEN,
        Vulnerability,
        DMGReduction
    }

    public enum OtherEffects
    {
        None,
        Heal,
        Shield,
        Energy,
        ActionAdvance,
        DealDMG,
        SkipTurn,
    }

    public enum Element
    {
        None, //Not sure if this will be used but its anoying to add stuff to enums later
        All,
        Physical,
        Fire,
        Ice,
        Lightning,
        Wind,
        Quantum,
        Imaginary
    }

    public enum AbilityType
    {
        None,
        BasicAttack,
        Skill,
        Ultimate,
        Talent,
        Technique,
        FollowUp,
        Effect //This includes DOT
    }

    public enum Targets
    {
        EnemySingle,
        EnemyArea,
        EnemyAll,
        EnemyRandom,
        Self,
        AllySingle,
        AllyOthers, //Excluding cause
        AllyArea,
        AllyAll,
    }

    public enum Path
    {
        None,
        Destruction,
        TheHunt,
        Erudition,
        Harmony,
        Nihility,
        Preservation,
        Abundance
    }

    public enum Triggers
    {
        //All this triggers will have events that pass through 2 values: The Cause, The receiver (List)
        Immediate, //Always active even outside of combat
        StartOfCombat,
        BeforeAttack,
        AfterAttack,
        AfterKill,
        EnterField,
        WeaknessBreak,
        BeforeUlt,
        AfterUlt,
        BeforeSkill,
        AfterSkill,
        BeforeTakingDamage,
        AfterTakingDamage,
        EnergyChange,
        Heal,
        OnTurnStart,
        OnTurnEnd,
        OnDeath
    }

    public enum TriggerConditions
    {
        None,
        ImReceiver,
        /// <summary> Excluding yourself </summary>
        AllyIsTheReceiver,
        EnemyIsTheReceiver
    }

    public enum CauseConditions
    {
        None,
        ImTheCause,
        /// <summary> Excluding yourself </summary>
        AllyIsTheCause,
        EnemyIsTheCause,
    }

    public enum AboveOrBelow
    {
        Below,
        Above
    }

    public enum RelicPieces
    {
        Head,
        Hand,
        Body,
        Feet,
        Sphere,
        Rope
    }

    public static class TypesUtility
    {
        public static string GetName<T>(T t) where T : Enum
        {
            return Enum.GetName(typeof(T), t);
        }

        public static string GetName<T>(T[] t) where T : Enum
        {
            string value = "";
            foreach(T @enum in t)
            {
                value += GetName(@enum) + "&";
            }
            value = value.TrimEnd('&');
            return value;
        }

        public static string GetName(ScalesOf[] t)
        {
            string value = "";
            foreach (ScalesOf scale in t)
            {
                value += GetName(scale.stat) + "&";
            }
            value = value.TrimEnd('&');
            return value;
        }
    }
}

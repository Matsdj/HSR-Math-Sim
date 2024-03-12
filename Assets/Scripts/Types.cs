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
        MissingHP
    }

    public enum OtherEffects
    {
        None,
        Heal,
        Buff,
        Energy,
        ActionAdvance,
        DealDMG,
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
        DOT
    }

    public enum Targets
    {
        EnemySingle,
        EnemyArea,
        EnemyAll,
        Self,
        AllySingle,
        AllyOthers, //Excluding cause
        AllyArea,
        AllyAll,
    }

    public enum AbilityEffect
    {
        DMG,
        Heal,
        Shield,
        Buff
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
        Always, //Always active even outside of combat
        StartOfCombat,
        BeforeAttack,
        AfterAttack,
        AfterDefeatEnemy,
        EnemyEnterField,
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
    }

    public enum TriggerConditions
    {
        None,
        ImTheCause,
        AllyIsTheCause,
        EnemyIsTheCause,
        ImReceiver,
        AllyIsTheReceiver, //Excluding yourself
        EnemyIsTheReceiver
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
}

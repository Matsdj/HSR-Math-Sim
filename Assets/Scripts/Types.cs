using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Types
{
    public enum Stats
    {
        HP,
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
        Effect_RES
    }

    public enum OtherEffects
    {
        Heal,
        Shield,
        Energy
    }

    public enum Element
    {
        All,
        None, //Not sure if this will be used but its anoying to add stuff to enums later
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
        None, //Just here as a placeholder and for testing
        StartOfCombat, //Is an exception because there will be no cause only inflicted.
        BeforeAttack,
        AfterAttack,
        AfterDefeatEnemy,
        EnemyEnterField,
        EnemyLeaveField,
        WeaknessBreak,
        BeforeUlt,
        AfterUlt,
        BeforeSkill,
        AfterSkill,
    }

    public enum TriggerConditions
    {
        None,
        ImTheCause,
        AllyIsTheCause,
        EnemyIsTheCause,
        ImReceiver,
        AllyIsTheReceiver,
        EnemyIsTheReceiver,
        Below50perHealth,
    }
}

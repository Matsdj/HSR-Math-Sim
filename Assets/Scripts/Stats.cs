using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> HP, ATK, DEF, SPD </summary>
[Serializable]
public class BaseStats
{
    public float HP;
    public float ATK;
    public float DEF;
    public float SPD;
}

/// <summary> CRIT rate+dmg, Break, Healboost, Energy Max+rate, Effect Hit+RES </summary>
[Serializable]
public class AdvancedStats
{
    public float CRIT_Rate = 5,
        CRIT_DMG = 50,
        BreakEffect,
        OutgoingHealingBoost,
        MaxEnergy,
        EnergyRegenerationRate,
        EffectHitRate,
        EffectRES,
        Aggro;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> HP, ATK, DEF, SPD </summary>
[Serializable]
public class BaseStats
{
    public int HP;
    public int ATK;
    public int DEF;
    public int SPD;
    public BaseStats(int hP = 1000, int aTK = 600, int dEF = 400, int sPD = 100)
    {
        HP = hP;
        ATK = aTK;
        DEF = dEF;
        SPD = sPD;
    }
}

/// <summary> CRIT rate+dmg, Break, Healboost, Energy Max+rate, Effect Hit+RES </summary>
[Serializable]
public class AdvancedStats
{
    public float CRIT_Rate,
        CRIT_DMG,
        BreakEffect,
        OutgoingHealingBoost,
        MaxEnergy,
        EnergyRegenerationRate,
        EffectHitRate,
        EffectRES;
    public AdvancedStats(float cRIT_Rate, float cRIT_DMG, float breakEffect, float outgoingHealingBoost, float maxEnergy, float energyRegenerationRate, float effectHitRate, float effectRES)
    {
        CRIT_Rate = cRIT_Rate;
        CRIT_DMG = cRIT_DMG;
        BreakEffect = breakEffect;
        OutgoingHealingBoost = outgoingHealingBoost;
        MaxEnergy = maxEnergy;
        EnergyRegenerationRate = energyRegenerationRate;
        EffectHitRate = effectHitRate;
        EffectRES = effectRES;
    }
}

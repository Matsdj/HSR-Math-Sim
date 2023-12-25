using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> HP, ATK, DEF, SPD </summary>
[Serializable]
public class BaseStats
{
    //At lvl 1
    public float HP;
    public float ATK;
    public float DEF;
    public float SPD;

    [HideInInspector] public float finalHP;
    [HideInInspector] public float finalATK;
    [HideInInspector] public float finalDEF;
    [HideInInspector] public float finalSPD;

    [HideInInspector] public float percentHP = 0;
    [HideInInspector] public float flatHP = 0;
    [HideInInspector] public float percentATK = 0;
    [HideInInspector] public float flatATK = 0;
    [HideInInspector] public float percentDEF = 0;
    [HideInInspector] public float flatDEF = 0;
    [HideInInspector] public float percentSPD = 0;
    [HideInInspector] public float flatSPD = 0;
    public void FinalStats(int lvl, int ascension, string LightCone, string[] EquipmentStats)
    {
        //At current lvl
        float tempHP = HP + HP * 0.05f * (lvl - 1) + HP * 0.4f * (ascension);
        float tempATK = ATK + ATK * 0.05f * (lvl - 1) + ATK * 0.4f * (ascension);
        float tempDEF = DEF + DEF * 0.05f * (lvl - 1) + DEF * 0.4f * (ascension);
        float tempSPD = SPD;


        //TODO Lightcone

        //TODO Equipment

        finalHP = tempHP * (1 + percentHP);
        finalATK = tempATK * (1 + percentATK);
        finalDEF = tempDEF * (1 + percentDEF);
        finalSPD = tempSPD * (1 + percentSPD);
    }
}

/// <summary> CRIT rate+dmg, Break, Healboost, Energy Max+rate, Effect Hit+RES </summary>
[Serializable]
public class AdvancedStats
{
    [HideInInspector] public float CRIT_Rate = 5,
        CRIT_DMG = 50,
        BreakEffect,
        OutgoingHealingBoost,
        EnergyRegenerationRate,
        EffectHitRate,
        EffectRES;
    public float MaxEnergy,
        Aggro;
}

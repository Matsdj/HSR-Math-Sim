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
    public float SPD; //Also the base SPD

    [HideInInspector] public float finalHP;
    [HideInInspector] public float finalATK;
    [HideInInspector] public float finalDEF;
    [HideInInspector] public float finalSPD;

    [HideInInspector] public float baseHP = 0;
    [HideInInspector] public float baseATK = 0;
    [HideInInspector] public float baseDEF = 0;

    [HideInInspector] public float percentHP = 0;
    [HideInInspector] public float flatHP = 0;
    [HideInInspector] public float percentATK = 0;
    [HideInInspector] public float flatATK = 0;
    [HideInInspector] public float percentDEF = 0;
    [HideInInspector] public float flatDEF = 0;
    [HideInInspector] public float percentSPD = 0;
    [HideInInspector] public float flatSPD = 0;

    public void Calculate(int lvl, int ascension, LightCone lc)
    {
        //At current lvl
        float charHP = ScaleStat(lvl, ascension, HP);
        float charATK = ScaleStat(lvl, ascension, ATK);
        float charDEF = ScaleStat(lvl, ascension, DEF);

        //Lightcone
        float lightConeHP = ScaleStat(lc.LVL, lc.Ascension, lc.HP);
        float lightConeATK = ScaleStat(lc.LVL, lc.Ascension, lc.ATK);
        float lightConeDEF = ScaleStat(lc.LVL, lc.Ascension, lc.DEF);

        baseHP = charHP + lightConeHP;
        baseATK = charATK + lightConeATK;
        baseDEF = charDEF + lightConeDEF;
    }

    public float ScaleStat(int lvl, int ascension, float baseStat)
    {
        return baseStat + baseStat * 0.05f * (lvl - 1) + baseStat * 0.4f * (ascension);
    }

    public void CalculateAllFinalStats()
    {
        finalHP = baseHP + baseHP * percentHP + flatHP;
        finalATK = baseATK + baseATK * percentATK + flatATK;
        finalDEF = baseDEF + baseDEF * percentDEF + flatDEF;
        finalSPD = SPD + SPD * percentSPD + flatSPD;
    }

    public void CalculateFinalHP()
    {
        finalHP = baseHP + baseHP * percentHP + flatHP;
    }

    public void CalculateFinalATK()
    {
        finalATK = baseATK + baseATK * percentATK + flatATK;
    }

    public void CalculateFinalDEF()
    {
        finalDEF = baseDEF + baseDEF * percentDEF + flatDEF;
    }

    public void CalculateFinalSPD()
    {
        finalSPD = SPD + SPD * percentSPD + flatSPD;
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
    [HideInInspector] public ElementBoost DMG_Boosts, RES_Boosts;

    public class ElementBoost
    {
        public float Physical, Fire, Ice, Lightning, Wind, Quantum, Imaginary;
    }
}

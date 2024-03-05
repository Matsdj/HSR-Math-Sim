using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RuntimeCharacter;

/// <summary> HP, ATK, DEF, SPD </summary>
[Serializable]
public class BaseStats
{
    //At lvl 1
    public float HP;
    public float ATK;
    public float DEF;
    public float SPD; //Also the base SPD
}

/// <summary> CRIT rate+dmg, Break, Healboost, Energy Max+rate, Effect Hit+RES </summary>
[Serializable]
public class AdvancedStats
{
    [HideInInspector]
    public float CRIT_Rate = 5,
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

public class RuntimeStats
{
    public readonly BaseStats BaseStats;
    public readonly AdvancedStats AdvancedStats;

    public RuntimeStats(Character character)
    {
        BaseStats = character.baseStats;
        AdvancedStats = character.advancedStats;
        Calculate(LVL, Ascension, null);
        CalculateAllFinalStats();
        CurrentHP = Final.HP;
    }

    public int LVL = 80;
    public int Ascension = 6;
    public BaseStats Base = new BaseStats(); //Total stats before percentage increase
    public BaseStats Per = new BaseStats(); //Percentage increase for stats
    public BaseStats Flat = new BaseStats(); //flat increase from relics and buffs
    public BaseStats Final = new BaseStats();

    public AdvancedStats Adv = new AdvancedStats();

    private float _currentHP;
    private List<Effect> _shields = new List<Effect>();
    private float _energy;
    public float CurrentHP { get { return _currentHP; } set { _currentHP = MathF.Min(value, Final.HP); } }
    public virtual float Energy { get { return _energy; } set { _energy = MathF.Min(value, Adv.MaxEnergy); } }

    public void Calculate(int lvl, int ascension, LightCone lc)
    {
        //At current lvl
        float charHP = ScaleStat(lvl, ascension, BaseStats.HP);
        float charATK = ScaleStat(lvl, ascension, BaseStats.ATK);
        float charDEF = ScaleStat(lvl, ascension, BaseStats.DEF);

        //Lightcone
        float lightConeHP = 0;
        float lightConeATK = 0;
        float lightConeDEF = 0;
        if (lc)
        {
            lightConeHP = ScaleStat(lc.LVL, lc.Ascension, lc.HP);
            lightConeATK = ScaleStat(lc.LVL, lc.Ascension, lc.ATK);
            lightConeDEF = ScaleStat(lc.LVL, lc.Ascension, lc.DEF);
        }
        Base.HP = charHP + lightConeHP;
        Base.ATK = charATK + lightConeATK;
        Base.DEF = charDEF + lightConeDEF;
        Base.SPD = BaseStats.SPD;

        //Adv
        Adv.MaxEnergy = AdvancedStats.MaxEnergy;
        Adv.Aggro = AdvancedStats.Aggro;
    }

    public float ScaleStat(int lvl, int ascension, float baseStat)
    {
        return baseStat + baseStat * 0.05f * (lvl - 1) + baseStat * 0.4f * (ascension);
    }

    public void CalculateAllFinalStats()
    {
        CalculateFinalHP();
        CalculateFinalATK();
        CalculateFinalDEF();
        CalculateFinalSPD();
    }

    public void CalculateFinalHP()
    {
        Final.HP = Base.HP + Base.HP * Per.HP + Flat.HP;
    }

    public void CalculateFinalATK()
    {
        Final.ATK = Base.ATK + Base.ATK * Per.ATK + Flat.ATK;
    }

    public void CalculateFinalDEF()
    {
        Final.DEF = Base.DEF + Base.DEF * Per.DEF + Flat.DEF;
    }

    public void CalculateFinalSPD()
    {
        Final.SPD = Base.SPD + Base.SPD * Per.SPD + Flat.SPD;
    }
}

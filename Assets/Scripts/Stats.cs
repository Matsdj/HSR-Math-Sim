using System;
using System.Collections;
using System.Collections.Generic;
using Types;
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
    public static float DefaultFloat = -1;

    public ref float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.HP:
                return ref HP;
            case Stats.ATK:
                return ref ATK;
            case Stats.DEF:
                return ref DEF;
            case Stats.SPD:
                return ref SPD;
        }
        Debug.LogError("Unimplemented Stat returning defaultFloat");
        return ref DefaultFloat;
    }
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

    public void CalculateAllFinalStats(Stats stat = Stats.None, bool doError = true)
    {
        switch (stat)
        {
            case Stats.None:
                CalculateFinalHP();
                CalculateFinalATK();
                CalculateFinalDEF();
                CalculateFinalSPD();
                break;
            case Stats.HP:
                CalculateFinalHP();
                break;
            case Stats.ATK:
                CalculateFinalATK();
                break;
            case Stats.DEF:
                CalculateFinalDEF();
                break;
            case Stats.SPD:
                CalculateFinalSPD();
                break;
            default:
                if (doError) Debug.LogError($"{Enum.GetName(typeof(Stats), stat)} does not have a calculateFinal function");
                break;
        }
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

    public float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.None:
                //Debug.LogError("None is not a valid Stat");
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

    public ref float GetStatReference(Stats stat, BaseStats @base, AdvancedStats advanced)
    {
        switch (stat)
        {
            case Stats.HP:
                return ref @base.HP;
            case Stats.ATK:
                return ref @base.ATK;
            case Stats.DEF:
                return ref @base.DEF;
            case Stats.SPD:
                return ref @base.SPD;
            case Stats.CRIT_Rate:
                return ref advanced.CRIT_Rate;
            case Stats.CRIT_DMG:
                return ref advanced.CRIT_DMG;
            case Stats.Break_Effect:
                return ref advanced.BreakEffect;
            case Stats.Outgoing_Healing_Boost:
                return ref advanced.OutgoingHealingBoost;
            case Stats.Max_Energy:
                return ref advanced.MaxEnergy;
            case Stats.Energy_Regeneration_Rate:
                return ref advanced.EnergyRegenerationRate;
            case Stats.Effect_Hit_Rate:
                return ref advanced.EffectHitRate;
            case Stats.Effect_RES:
                return ref advanced.EffectRES;
        }
        Debug.LogError("Unimplemented Stat reference returning defaultFloat");
        return ref BaseStats.DefaultFloat;
    }

    public ref float GetStatReference(Stats stat, BaseStats @base)
    {
        return ref GetStatReference(stat, @base, null);
    }

    public ref float GetStatReference(Stats stat, AdvancedStats advanced)
    {
        return ref GetStatReference(stat, null, advanced);
    }
}

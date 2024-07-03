using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary> HP, ATK, DEF, SPD </summary>
[Serializable]
public class BaseStats
{
    //At lvl 1
    public float HP;
    public float ATK;
    public float DEF;
    public float SPD; //Also the base SPD
    public static float DefaultFloat = 0;

    public ref float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.HP: return ref HP;
            case Stats.ATK: return ref ATK;
            case Stats.DEF: return ref DEF;
            case Stats.SPD: return ref SPD;
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
        BreakEffect = 100,
        OutgoingHealingBoost,
        EnergyRegenerationRate,
        EffectHitRate,
        EffectRES;
    public float MaxEnergy,
        Aggro,
        Toughness = 240;
    [HideInInspector] public Elements ElementDMG;
    [HideInInspector] public Abilities AbilityDMG;
    public Elements ElementRES;
    [HideInInspector] public float Weaken;
    [HideInInspector] public float DEF_PEN;
    [HideInInspector] public Elements RES_PEN;
    [HideInInspector] public float Vulnerability;
    [HideInInspector] public List<float> DMGReduction;

    [Serializable]
    public class Elements
    {
        public float Physical, Fire, Ice, Lightning, Wind, Quantum, Imaginary;
        public ref float Get(Element element)
        {
            switch (element)
            {
                case Element.Physical: return ref Physical;
                case Element.Fire: return ref Fire;
                case Element.Ice: return ref Ice;
                case Element.Lightning: return ref Lightning;
                case Element.Wind: return ref Wind;
                case Element.Quantum: return ref Quantum;
                case Element.Imaginary: return ref Imaginary;
            }
            return ref BaseStats.DefaultFloat;
        }

        public void AddToAll(float amount)
        {
            Physical += amount;
            Fire += amount;
            Ice += amount;
            Lightning += amount;
            Wind += amount;
            Quantum += amount;
            Imaginary += amount;
        }

        public void Add(Element type, float amount)
        {
            if (type != Element.None)
            {
                if (type == Element.All)
                {
                    AddToAll(amount);
                }
                else
                {
                    Get(type) += amount;
                }
            }
        }

        public void Copy(Elements elements)
        {
            if (elements == null) return;
            Physical = elements.Physical;
            Fire = elements.Fire;
            Ice = elements.Ice;
            Lightning = elements.Lightning;
            Wind = elements.Wind;
            Quantum = elements.Quantum;
            Imaginary = elements.Imaginary;
        }
    }

    [Serializable]
    public class Abilities
    {
        public float Basic, Skill, Ult, DOT, FollowUp;

        public ref float Get(AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.BasicAttack: return ref Basic;
                case AbilityType.Skill: return ref Skill;
                case AbilityType.Ultimate: return ref Ult;
                case AbilityType.Effect: return ref DOT;
                case AbilityType.FollowUp: return ref FollowUp;
            }
            return ref BaseStats.DefaultFloat;
        }

        public float Get(AbilityType[] abilityTypes)
        {
            float amount = 0;
            foreach (var abilityType in abilityTypes)
            {
                amount += Get(abilityType);
            }
            return amount;
        }

        public void Add(AbilityType type, float amount)
        {
            if (type != AbilityType.None)
            {
                Get(type) += amount;
            }
        }

        public void Copy(Abilities abilities)
        {
            if (abilities == null) return;
            Basic = abilities.Basic;
            Skill = abilities.Skill;
            Ult = abilities.Ult;
            DOT = abilities.DOT;
            FollowUp = abilities.FollowUp;
        }
    }

    public void Copy(AdvancedStats stats)
    {
        CRIT_Rate = stats.CRIT_Rate;
        CRIT_DMG = stats.CRIT_DMG;
        BreakEffect = stats.BreakEffect;
        OutgoingHealingBoost = stats.OutgoingHealingBoost;
        EnergyRegenerationRate = stats.EnergyRegenerationRate;
        EffectHitRate = stats.EffectHitRate;
        EffectRES = stats.EffectRES;
        MaxEnergy = stats.MaxEnergy;
        Aggro = stats.Aggro;
        Toughness = stats.Toughness;
        ElementDMG = new Elements();
        ElementDMG.Copy(stats.ElementDMG);
        AbilityDMG = new Abilities();
        AbilityDMG.Copy(stats.AbilityDMG);
        ElementRES = new Elements();
        ElementRES.Copy(stats.ElementRES);
        Weaken = stats.Weaken;
        DEF_PEN = stats.DEF_PEN;
        RES_PEN = new Elements();
        RES_PEN.Copy(stats.RES_PEN);
        Vulnerability = stats.Vulnerability;
        DMGReduction = new List<float>();
        if (stats.DMGReduction != null) foreach (float value in stats.DMGReduction)
                DMGReduction.Add(value);
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
        Adv.Copy(AdvancedStats);
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

    public virtual void CalculateFinalSPD()
    {
        Final.SPD = Base.SPD + Base.SPD * Per.SPD + Flat.SPD;
    }

    public float GetStat(Stats stat)
    {
        switch (stat)
        {
            case Stats.None: return 0;
            case Stats.HP: return Final.HP;
            case Stats.ATK: return Final.ATK;
            case Stats.DEF: return Final.DEF;
            case Stats.SPD: return Final.SPD;
            case Stats.CRIT_Rate: return Adv.CRIT_Rate;
            case Stats.CRIT_DMG: return Adv.CRIT_DMG;
            case Stats.Break_Effect: return Adv.BreakEffect;
            case Stats.Outgoing_Healing_Boost: return Adv.OutgoingHealingBoost;
            case Stats.Max_Energy: return Adv.MaxEnergy;
            case Stats.Energy_Regeneration_Rate: return Adv.EnergyRegenerationRate;
            case Stats.Effect_Hit_Rate: return Adv.EffectHitRate;
            case Stats.Effect_RES: return Adv.EffectRES;
            case Stats.CurrentHP: return CurrentHP;
            case Stats.MissingHP: return Final.HP - CurrentHP;
            case Stats.Toughness: return Adv.Toughness;
            case Stats.LVLMultiplier: return LevelMultiplier[LVL - 1];
            case Stats.MaxToughnessMultiplier: return 0.5f + (AdvancedStats.Toughness / 40);
        }
        Debug.LogError("Unimplemented Stat");
        return 0;
    }

    public float GetStat(Stats[] stats)
    {
        float value = 1;
        foreach (var stat in stats)
        {
            value *= GetStat(stat);
        }
        return value;
    }

    public ref float GetStatReference(Stats stat, BaseStats @base, AdvancedStats advanced)
    {
        switch (stat)
        {
            case Stats.HP: return ref @base.HP;
            case Stats.ATK: return ref @base.ATK;
            case Stats.DEF: return ref @base.DEF;
            case Stats.SPD: return ref @base.SPD;
            case Stats.CRIT_Rate: return ref advanced.CRIT_Rate;
            case Stats.CRIT_DMG: return ref advanced.CRIT_DMG;
            case Stats.Break_Effect: return ref advanced.BreakEffect;
            case Stats.Outgoing_Healing_Boost: return ref advanced.OutgoingHealingBoost;
            case Stats.Max_Energy: return ref advanced.MaxEnergy;
            case Stats.Energy_Regeneration_Rate: return ref advanced.EnergyRegenerationRate;
            case Stats.Effect_Hit_Rate: return ref advanced.EffectHitRate;
            case Stats.Effect_RES: return ref advanced.EffectRES;
            case Stats.Weaken: return ref advanced.Weaken;
            case Stats.DEF_PEN: return ref advanced.DEF_PEN;
            case Stats.Vulnerability: return ref advanced.Vulnerability;
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

    public float[] LevelMultiplier = new float[] {
    54f,58f,62f,67.5264f,70.5094f,73.5228f,76.566f,79.6385f,82.7395f,85.8684f,91.4944f,97.068f,102.5892f,108.0579f,113.4743f,
    118.8383f,124.1499f,129.4091f,134.6159f,139.7703f,149.3323f,158.8011f,168.1768f,177.4594f,186.6489f,195.7452f,204.7484f,
    213.6585f,222.4754f,231.1992f,246.4276f,261.181f,275.4733f,289.3179f,302.7275f,315.7144f,328.2905f,340.4671f,352.2554f,
    363.6658f,408.124f,451.7883f,494.6798f,536.8188f,578.2249f,618.9172f,658.9138f,698.2325f,736.8905f,774.9041f,871.0599f,
    964.8705f,1056.4206f,1145.791f,1233.0585f,1318.2965f,1401.575f,1482.9608f,1562.5178f,1640.3068f,1752.3215f,1861.9011f,
    1969.1242f,2074.0659f,2176.7983f,2277.3904f,2375.9085f,2472.416f,2566.9739f,2659.6406f,2780.3044f,2898.6022f,3014.6029f,
        3128.3729f,3239.9758f,3349.473f,3456.9236f,3562.3843f,3665.9099f,3767.5533f};
}

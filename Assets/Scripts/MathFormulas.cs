using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathFormulas
{
    /// <summary>
    /// Base DMG = (Skill Multiplier + Extra Multiplier) * Scaling Attribute + Extra DMG
    /// </summary>
    /// <param name="skillMultiplier">this is the percentage value you can find in the skill description (Deal DMG equal to XX%)</param>
    /// <param name="extraMultiplier">this appears only on some skills, like Dan Heng's Ultimate that deals additional damage to slowed enemies</param>
    /// <param name="scalingAttribute">this is the attribute the skill scales off - in most cases it's ATK</param>
    /// <param name="extraFlatAmount">this is the flat additional damage that appears on some skills</param>
    public static float BaseAmount(float skillMultiplier, float extraMultiplier, float scalingAttribute, float extraFlatAmount, float amountCap = Mathf.Infinity)
    {
        return Mathf.Min((skillMultiplier + extraMultiplier) / 100 * scalingAttribute + extraFlatAmount, amountCap);
    }

    /// <summary>
    /// Wether or not it crit
    /// </summary>
    /// <param name="critChance">0.xx value</param>
    /// <returns></returns>
    public static bool DoCrit(float critChance)
    {
        return critChance > Random.value; //TODO Check if random works in static
    }

    /// <summary>
    /// Crit Multiplier = 1 + Crit DMG
    /// </summary>
    public static float CritMultiplier(float critDMG)
    {
        return 1 + critDMG;
    }

    public static float Crit(float critChance, float critDMG)
    {
        return (critChance > Random.value) ? 1 + critDMG : 1;
    }

    /// <summary>
    /// DMG% Multiplier = 100% + Elemental DMG% + All Type DMG% + DoT DMG% + Other DMG%
    /// </summary>
    /// All DMG% is added together and grouped into one place in the DMG equation. The equation is as follows:
    /// Remove DMG% multipliers if they are not relevant to the calculation. For example, 
    /// if Hook has 38.9% Fire DMG and gains 20% DMG against a Burning/Bleeding enemy from 
    /// “Woof! Walk Time!” then her total DMG% multiplier is 158.9% against a Burning or 
    /// Bleeding enemy, but only 138.9% against a non-Burning or Bleeding enemy.
    /// <param name="elementalDMG"></param>
    /// <param name="allTypeDMG"></param>
    /// <param name="dotDMG"></param>
    /// <param name="otherDMG"></param>
    /// <returns></returns>
    public static float DMGMultiplier(float totalDMG)
    {
        return 1 + totalDMG;
    }

    /// <summary>
    /// Weaken Multiplier = 1 - Weaken
    /// </summary>
    public static float WeakenMultiplier(float weaken)
    {
        return 1 - weaken;
    }

    /// <summary>
    /// DEF Multiplier = 1 - (DEF / (DEF + 200 + 10 * Level Attacker))
    /// </summary>
    /// <param name="levelAttacker">Level of the attacker</param>
    /// <param name="baseDef">Base DEF of the target</param>
    /// <param name="defBonus">DEF% Bonus of the target</param>
    /// <param name="defPenetration">DEF% Penetration of the attacker</param>
    /// <param name="additiveDefBonus">Additive Flat DEF Bonus of the target</param>
    public static float DEFMultiplier(float levelAttacker, float baseDef, float defBonus, float defPenetration, float additiveDefBonus = 0)
    {
        float def = baseDef * Mathf.Max(0, (1 + defBonus - defPenetration)) + additiveDefBonus;
        return 1 - (def / (def + 200 + 10 * levelAttacker));
    }

    /// <summary>
    /// RES Multiplier = 1 - (RES_target - RES_penetration))
    /// </summary>
    public static float RESMultiplier(float resTarget, float resPenetration)
    {
        return 1 - (resTarget - resPenetration);
    }

    /// <summary>
    /// Vulnerability Multiplier = 1 + Vulnerability
    /// </summary>
    public static float VulnerabilityMultiplier(float totalVulnerability)
    {
        return 1 + totalVulnerability;
    }

    /// <summary>
    /// DMG Reduction Multiplier = 1 * (1 - DMG Reduction1) * (1 - DMG Reduction2) * (1 - DMG Reduction3) * (1 - DMG Reduction4) * ...
    /// </summary>
    public static float DMGReductionMultiplier(List<float> dmgReduction)
    {
        float result = 1;
        foreach (float dmgRed in dmgReduction)
        {
            result *= (1 - dmgRed);
        }
        return result;
    }

    /// <summary>
    /// Broken Multiplier = 0.9 + 0.1*isWeaknessBroken
    /// </summary>
    public static float BrokenMultiplier(bool isWeaknessBroken)
    {
        return 0.9f + 0.1f * (isWeaknessBroken ? 1 : 0);
    }
}

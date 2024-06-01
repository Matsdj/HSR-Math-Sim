using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;
using System.Linq;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 2)]
public class Ability : ScriptableObject
{
    public int EnergyGeneration = 20;
    public AbilityType[] AbilityTypes;
    public Targets Targets;
    public OtherEffects MainEffect;
    public Stats ScalingStat;
    public float ScalingPer;
    public float ScalingPerReductionForNonMainTargets;
    public float FlatAmount;
    public float WeaknessBreak = 30;
    public float ExtraWeaknessBreakForMainTarget;
    public Effect ApplyEffectToTarget;
    public Ability[] TriggerThisAbility;
    public bool IsOfType(AbilityType type)
    {
        return AbilityTypes.Contains(type);
    }
}

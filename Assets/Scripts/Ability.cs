using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 2)]
public class Ability : ScriptableObject
{
    public AbilityType AbilityType;
    public AbilityTargets Targets;
    public Stats ScalingStat;
    public float ScalingPer;
    public float ScalingPerReductionForNonMainTargets;
    public Effect ApplyEffectToTarget;
}
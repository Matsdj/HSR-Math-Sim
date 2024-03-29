using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Types;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Ability", order = 2)]
public class Ability : ScriptableObject
{
    public int EnergyGeneration = 20;
    public AbilityType AbilityType; //Might be redundent
    public Targets Targets;
    public OtherEffects MainEffect;
    public Stats ScalingStat;
    public float ScalingPer;
    public float ScalingPerReductionForNonMainTargets;
    public float FlatAmount;
    public Effect ApplyEffectToTarget;
}

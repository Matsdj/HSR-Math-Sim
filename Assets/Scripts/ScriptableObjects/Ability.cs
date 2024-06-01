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
    [Tooltip("This also applies to talent, technique and ult, but it doesn't end turn if it isn't your turn.")]
    public bool EndTurn = true;
    [Tooltip("Basic = 1, Skill = -1. This also applies to talent, technique and ult. The ability will cancel if you don't have enough when it is a negative value")]
    public int Skillpoints = 0;
    public Effect ChangeAbilityWhenThisEffect;
    [Tooltip("First ability in the list is at 1 stack second is at 2 stacks etc.")]
    public Ability[] ChangeAbilityToThis;
    public bool IsOfType(AbilityType type)
    {
        return AbilityTypes.Contains(type);
    }
}

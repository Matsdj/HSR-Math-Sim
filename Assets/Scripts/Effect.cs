using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Effect", order = 4)]
public class Effect : ScriptableObject
{
    public Stats Stat;
    public OtherEffects OtherEffects;
    public Element DMG_Type;
    public float FlatIncrease;
    public float PerIncrease;

    public Stats ScalesOf;
    public bool ScaleOfTargetStats;
    [Tooltip("Forgot what this is for XD")]
    public float PerIncreaseOther;

    public AbilityType[] OnlyAppliesToAbilityType;

    public int StackMax = 1;
    public bool StackCountMultipliesBuff;
    [Tooltip("Stack Count Decrease Amount Per Turn")]
    public int StackCountDecreaseAmountPerTurn;
}

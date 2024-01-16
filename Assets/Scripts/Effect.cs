using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Effect", order = 4)]
public class Effect : ScriptableObject
{
    public Stats Stat;
    public float FlatIncrease;
    public float PerIncrease;

    public Element DMG_Type;
    public AbilityType DMG_Type2;
    public float DMG_PerIncrease;

    public OtherEffects OtherEffects;
    public Stats ScalesOf;
    public float PerIncreaseOther;

    public int StackMax;
    public bool StackCountMultipliesBuff;
    public int StackCountDecreaseAmountPerTurn;
}

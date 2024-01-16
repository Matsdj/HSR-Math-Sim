using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Effect", order = 2)]
public class Effect : ScriptableObject
{
    public Stats Stat;
    public float StatIncrease;
    public float StatPerIncrease;
    public Element DMG_Type;
    public float DMG_PerIncrease;

    public int StackMax;
    public bool StackCountMultipliesBuff;
    public int StackCountDecreaseAmountPerTurn;
}

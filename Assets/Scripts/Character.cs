using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    public Sprite sprite;
    public int LVL = 1;
    public int Ascension = 0;
    public BaseStats baseStats;
    public AdvancedStats advancedStats;
    public int Rarity = 4;

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    public Sprite sprite;
    public int LVL = 80;
    public BaseStats baseStats;
    public AdvancedStats advancedStats;

    
}

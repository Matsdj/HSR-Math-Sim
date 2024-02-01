using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "ScriptableObjects/Relic", order = 5)]
public class Relic : ScriptableObject
{
    public Mechanics[] SetEffect1;
    public Mechanics[] SetEffect2;
    public Sprite Head;
    public Sprite Hand;
    public Sprite Body;
    public Sprite Feet;
    public Sprite Sphere;
    public Sprite Rope;
}

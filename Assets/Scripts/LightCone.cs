using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Light Cone", menuName = "ScriptableObjects/LightCone", order = 3)]
public class LightCone : ScriptableObject
{
    public Sprite Sprite;
    public Types.Path Path;
    public int LVL = 1;
    public int Ascension;
    public float HP;
    public float ATK;
    public float DEF;
    public Mechanics[] mechanics;
}

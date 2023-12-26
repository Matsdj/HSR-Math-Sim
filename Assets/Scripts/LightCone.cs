using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Light Cone", menuName = "ScriptableObjects/LightCone", order = 1)]
public class LightCone : ScriptableObject
{
    public Types.Path Path;
    public float HP;
    public float ATK;
    public float DEF;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Passive", menuName = "ScriptableObjects/Passive", order = 4)]
public class Passive : Ability
{
    public Mechanics[] Passives;
}

using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Relic", menuName = "ScriptableObjects/Relic", order = 5)]
public class Relic : ScriptableObject
{
    public Stats mainStat;
    public int LVL = 1;
    public int flatHP;
    public int flatATK;
    public int flatDEF;
    public int flatSPD;
    public float percentHP;
    public float percentATK;
    public float percentDEF;
    public float critRate;
    public float critDMG;
    public float outgoingHealing;
    public float effectHitRate;
    public float effectResist;
    public float physicalDMG;
    public float fireDMG;
    public float iceDMG;
    public float lightningDMG;
    public float windDMG;
    public float quantumDMG;
    public float imaginaryDMG;



    public Mechanics[] SetEffect1;
    public Mechanics[] SetEffect2;
    public Sprite Head;
    public Sprite Hand;
    public Sprite Body;
    public Sprite Feet;
    public Sprite Sphere;
    public Sprite Rope;
}

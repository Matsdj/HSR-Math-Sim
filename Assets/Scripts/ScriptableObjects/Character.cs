using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class Character : ScriptableObject
{
    public Sprite sprite;
    public Path path;
    public Element element;
    public bool IsEliteOrBoss;
    public int LVL = 1;
    public int Ascension = 0;
    public BaseStats baseStats;
    public AdvancedStats advancedStats;
    public int Rarity = 4;

    public Ability Basic;
    public Ability Skill;
    public Ultimate Ultimate;
    public Passive Talent;
    public Passive Technique;

    public Effect[] Traces; //Essentially just effects that are applied ASAP
    public LightCone LightCone;
    public Relic[] Relics = new Relic[6];

    public AudioClip audio;
}

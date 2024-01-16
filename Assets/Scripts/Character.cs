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
    public int LVL = 1;
    public int Ascension = 0;
    public BaseStats baseStats;
    public AdvancedStats advancedStats;
    public int Rarity = 4;

    public Ability Basic;
    public Ability Skill;
    public Ability Ultimate;
    public Mechanics Talent;
    public Ability Technique;

    public List<float> AttackPercentages = new List<float>();

    public AudioClip audio;
}

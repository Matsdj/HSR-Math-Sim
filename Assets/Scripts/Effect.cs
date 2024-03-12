using System;
using System.Collections.Generic;
using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Effect", order = 4)]
public class Effect : ScriptableObject
{
    public Stats StatToBuff;
    public OtherEffects OtherEffect; //Only works if StatToBuff is none
    public Triggers[] OtherEffectTriggers; //Trigger Condition is always 
    public Element DMG_Type; //Only applies if OtherEffect is Deal DMG
    public float FlatIncrease; //Amount of flat Statbuff or DMG
    public float PerIncrease; //Amount of Percentage increase for StatToBuff

    public Stats ScalesOf;
    public bool ScaleOfTargetStats;
    public float PerIncreaseScalingStat; //Uses a percentage of the stat as flat amount

    [Tooltip("Some buffs only apply when ")]
    public AbilityType[] OnlyAppliesToAbilityType;

    public int Duration = 1;
    public bool CountDownAtStart, CountDownAtEnd;

    public int StackMax = 1;
    public bool StackCountMultipliesBuff;
    public bool LoseStackOnTrigger;
}

public class RuntimeEffect
{
    public readonly Effect Base;

    public int Duration { get => _duration; }
    public int StackCount { get => _stackCount; }
    private int _duration;
    private int _stackCount;
    private RuntimeCharacter _character;
    private Func<Effect, bool> _removeSelfAction;

    public RuntimeEffect(Effect @base, RuntimeCharacter character, Func<Effect, bool> removeSelfAction)
    {
        Base = @base;
        _duration = Base.Duration;
        _stackCount = 1;
        _character = character;

        if (Base.CountDownAtStart) character.Events[Triggers.OnTurnStart].Add(DurationCountDown);
        if (Base.CountDownAtEnd) character.Events[Triggers.OnTurnEnd].Add(DurationCountDown);

        foreach(Triggers trigger in Base.OtherEffectTriggers)
        {
            character.Events[trigger].Add(DoOtherEffect);
        }

        _removeSelfAction = removeSelfAction;
    }

    public void Apply()
    {
        _duration = Base.Duration;
        if (_stackCount < Base.StackMax) _stackCount++;
    }

    private void DurationCountDown(RuntimeCharacter receiver, RuntimeCharacter cause)
    {
        _duration--;
        if (_duration == 0) RemoveSelf();
    }

    private void DoOtherEffect(RuntimeCharacter receiver, RuntimeCharacter cause)
    {
        float amount = MathFormulas.BaseAmount(Base.PerIncreaseScalingStat,0,_character.GetStat(Base.ScalesOf), Base.FlatIncrease);
        Combat.MainEffect(_character, Base.OtherEffect, new List<RuntimeCharacter>() { _character }, amount);

        if (Base.LoseStackOnTrigger)
        {
            _stackCount--;
            if (_stackCount == 0) RemoveSelf();
        }
    }

    private void RemoveSelf()
    {
        if (Base.CountDownAtStart) _character.Events[Triggers.OnTurnStart].Remove(DurationCountDown);
        if (Base.CountDownAtEnd) _character.Events[Triggers.OnTurnEnd].Remove(DurationCountDown);
        foreach (Triggers trigger in Base.OtherEffectTriggers)
        {
            _character.Events[trigger].Remove(DoOtherEffect);
        }
        _removeSelfAction.Invoke(Base);
    }
}

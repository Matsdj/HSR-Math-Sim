using System;
using System.Collections.Generic;
using Types;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "ScriptableObjects/Effect", order = 4)]
public class Effect : ScriptableObject
{
    public float BaseChanceToApply;
    [Tooltip("If IsFixed is true then it ignores effect RES")]
    public bool IsFixed;

    public Buff[] Buffs;
    public OtherWithTrigger[] OtherEffect;

    public int Duration = 1;
    public bool CountDownAtStart, CountDownAtEnd;

    public int StackMax = 1;
    public bool StackCountMultipliesEffect;
    public bool LoseStackOnTrigger;
    public bool CanStackWithSameType = true;

    public Mechanics[] targetPassives; //This is used as if it is a new passives of the target
}

[Serializable]
public class Buff
{
    public Stats StatToBuff;
    public float FlatIncrease;
    public float PerIncrease;
}

[Serializable]
public class OtherWithTrigger
{
    public Triggers[] Triggers;
    public OtherEffect[] Other;
}

[Serializable]
public class OtherEffect
{
    public OtherEffects Effect; //Only works if StatToBuff is none
    public Element DMG_Type; //Only applies if OtherEffect is Deal DMG

    public float FlatIncrease; //Amount of flat Statbuff or DMG
    public float Multiplier;
    public ScalesOf[] ScalesOf;

    [Tooltip("Currently only applicable to bleed")]
    public float MultiplierOnEliteAndBoss = 1;
    public ScalesOf[] ScaleCap;
    public float ScaleCapMultiplier;

    [Tooltip("Some buffs only apply when")]
    public AbilityType[] OnlyAppliesToAbilityType;

    public float Amount(RuntimeCharacter cause, RuntimeCharacter target, float stackCount = 1)
    {
        float amount = ScaleAmount(ScalesOf, cause, target) * Multiplier * stackCount;
        if (target.BasedOfCharacter.IsEliteOrBoss) amount *= MultiplierOnEliteAndBoss;
        if (ScaleCap.Length != 0)
        {
            float cap = ScaleAmount(ScaleCap, cause, target) * ScaleCapMultiplier * stackCount;

            amount = Mathf.Min(cap, amount);
        }
        return amount;
    }

    private float ScaleAmount(ScalesOf[] scales, RuntimeCharacter cause, RuntimeCharacter target)
    {
        float value = 1;
        foreach (var scale in scales)
        {
            if (scale.UseTargetStat) value *= target.GetStat(scale.stat);
            else value *= cause.GetStat(scale.stat);
        }
        return value;
    }
}

[Serializable]
public class ScalesOf
{
    public Stats stat;
    public bool UseTargetStat;
}

public class RuntimeEffect
{
    public readonly Effect Base;
    public int Duration { get => _duration; }
    public int StackCount { get => _stackCount; }
    private int _duration;
    private int _stackCount;
    private RuntimeCharacter _cause;
    private RuntimeCharacter _target;
    private Func<KeyValuePair<Effect, int>, bool> _removeSelfAction;

    public event RuntimeCharacter.SimpleFunc OnRemove;
    private List<RuntimeOtherEffect> _otherEffects;

    public RuntimeEffect(Effect @base, RuntimeCharacter cause, RuntimeCharacter target, Func<KeyValuePair<Effect, int>, bool> removeSelfAction)
    {
        Base = @base;
        Apply();
        _cause = cause;
        _target = target;

        _otherEffects = new List<RuntimeOtherEffect>();
        //Trigger
        foreach (OtherWithTrigger other in Base.OtherEffect)
        {
            _otherEffects.Add(new RuntimeOtherEffect(other, OnRemove, this));
        }
        //Passive
        Combat.AddPassives(Base.targetPassives, target, OnRemove);

        _removeSelfAction = removeSelfAction;

        //Countdown
        if (Base.CountDownAtStart) target.Events[Triggers.OnTurnStart].Add(DurationCountDown);
        if (Base.CountDownAtEnd) target.Events[Triggers.OnTurnEnd].Add(DurationCountDown);
    }

    public void Apply()
    {
        _duration = Base.Duration;

        if (Base.StackCountMultipliesEffect) ApplyBuff(true);
        if (_stackCount < Base.StackMax) _stackCount++;
        if (Base.StackCountMultipliesEffect) ApplyBuff();
    }

    public void ApplyBuff(bool removeInstead = false)
    {
        foreach (Buff buff in Base.Buffs)
        {
            //Statbuff
            if (buff.StatToBuff != Stats.None)
            {
                float flat = buff.FlatIncrease;
                float per = buff.PerIncrease;
                if (removeInstead) { flat *= -1; per *= -1; }
                if (Base.StackCountMultipliesEffect) { flat *= StackCount; per *= StackCount; }

                if (buff.FlatIncrease != 0) _target.GetStatReference(buff.StatToBuff, _target.Flat) += flat;
                if (buff.PerIncrease != 0) _target.GetStatReference(buff.StatToBuff, _target.Per) += per;
                _target.CalculateAllFinalStats(buff.StatToBuff, false);
            }
        }
    }

    private void DurationCountDown(RuntimeCharacter receiver = null, RuntimeCharacter cause = null)
    {
        Debug.Log($"Count down effect {Base.name}");
        _duration--;
        if (_duration == 0) RemoveSelf();
    }

    public void RemoveSelf()
    {
        if (Base.CountDownAtStart) _target.Events[Triggers.OnTurnStart].Remove(DurationCountDown);
        if (Base.CountDownAtEnd) _target.Events[Triggers.OnTurnEnd].Remove(DurationCountDown);

        ApplyBuff(true);
        foreach (OtherWithTrigger otherWithTrigger in Base.OtherEffect)
        {
            foreach (OtherEffect other in otherWithTrigger.Other)
            {
                if (other.Effect == OtherEffects.Shield)
                {
                    _target.Shields.Remove(this);
                }
            }
        }

        OnRemove.Invoke();
        _removeSelfAction.Invoke(new KeyValuePair<Effect, int>(Base, _target.Id));
    }

    public string Info()
    {
        string info = "";
        info += $"[{Base.name},[Duration:{Duration}]";
        //foreach (OtherEffect buffOrOther in Base.BuffOrOther)
        //{
        //    info += $",{TypesUtility.GetName(buffOrOther.StatToBuff)}:[";
        //    if (buffOrOther.FlatIncrease != 0) info += $"Flat:{buffOrOther.FlatIncrease},";
        //    if (buffOrOther.PerIncrease != 0) info += $"Per:{buffOrOther.PerIncrease},";
        //    if (buffOrOther.ScalesOf.Length != 0)
        //        info += $"ScaleWith{TypesUtility.GetName(buffOrOther.ScalesOf)}:{buffOrOther.Multiplier},";
        //    info += "]";
        //}
        //info += "], ";
        return info;
    }

    public class RuntimeOtherEffect
    {
        private RuntimeEffect _parent;
        private OtherWithTrigger _base;
        public RuntimeOtherEffect(OtherWithTrigger @base, RuntimeCharacter.SimpleFunc onRemove, RuntimeEffect parent)
        {
            onRemove += Remove;
            _base = @base;
            _parent = parent;

            foreach(Triggers trigger in @base.Triggers)
            {
                if (trigger == Triggers.Immediate) DoOtherEffect();
                else parent._target.Events[trigger].Add(DoOtherEffect);
            }
        }

        private void DoOtherEffect(RuntimeCharacter receiver = null, RuntimeCharacter cause = null)
        {
            foreach (OtherEffect effect in _base.Other)
            {
                float amount = effect.Amount(_parent._cause, _parent._target);
                if (_parent.Base.StackCountMultipliesEffect) amount *= _parent.StackCount;
                Combat.instance.MainEffect(_parent._cause, effect.Effect, new List<RuntimeCharacter>() { _parent._target }, amount, _parent);
            }

            if (_parent.Base.LoseStackOnTrigger)
            {
                _parent._stackCount--;
                if (_parent._stackCount == 0) _parent.RemoveSelf();
            }
        }

        private void Remove()
        {
            foreach (Triggers trigger in _base.Triggers)
            {
                if (trigger != Triggers.Immediate) _parent._target.Events[trigger].Remove(DoOtherEffect);
            }
        }
    }
}

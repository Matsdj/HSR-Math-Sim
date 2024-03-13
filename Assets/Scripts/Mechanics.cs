using UnityEngine;
using Types;
using System;

[Serializable]
public class Mechanics
{
    [Tooltip("If it doesn't have a trigger the mechanic is ignored. If the trigger is None its always active")]
    public Triggers[] Trigger;
    public TriggerConditions[] TriggerCondition;
    public CauseConditions[] CauseCondition;
    public Effect[] TargetNeedsEffects;
    [Tooltip("This does not trigger if the target has the filled in effect in TargetNeedsEffects")]
    public bool excludeIfEffect;
    public Stats StatCondition;
    public AboveOrBelow AboveOrBelow;
    public float FlatAmount;
    public float PerAmount;

    [Tooltip("False means that it is comparing the conditions with OR, True means its And")]
    public bool UseAnd = false;
    public Targets target;
    public Effect effect;
    public int effectStackCountInflicted = 1;
}

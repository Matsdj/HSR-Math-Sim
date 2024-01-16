using UnityEngine;
using Types;
using System;

[Serializable]
public class Mechanics
{
    public Triggers[] Trigger;
    public TriggerConditions[] Condition;
    [Tooltip("False means that it is comparing the conditions with OR, True means its And")]
    public bool UseAnd = false;
    public Targets target;
    public Effect effect;
    public int effectStackCountInflicted = 1;
}

using UnityEngine;
using Types;

public class Mechanics
{
    public TriggerWithCondition Trigger;
    public class TriggerWithCondition
    {
        public Triggers[] Trigger;
        public TriggerConditions[] Condition;
        public bool ConditionsAreAnd = false; //False means that its considered an OR, True means its And
    }
}

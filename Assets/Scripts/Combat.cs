using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Combat : MonoBehaviour
{
    public Character[] Allies;
    public Character[] Enemies;
    public List<ActionCharacter> ActionOrder;
    public List<string> ActionHistory;
    public HorizontalLayoutGroup AllyUI;
    public HorizontalLayoutGroup EnemyUI;
    public VerticalLayoutGroup ActionOrderUI;
    public VerticalLayoutGroup ActionHistoryUI;
    private float _latestActionValue = 0;

    public void Awake()
    {
        ActionOrder = new List<ActionCharacter>();
        foreach (Character character in Allies)
        {
            AddCharacter(character);
        }
        foreach (Character character in Enemies)
        {
            AddCharacter(character);
        }
        SortActionOrder();
    }

    public void AddCharacter(Character character)
    {
        ActionCharacter action = new ActionCharacter(character, _latestActionValue);
        ActionOrder.Add(action);
    }

    public void DoTurn(ActionCharacter action)
    {
        action.DoTurn();
        SortActionOrder();
    }

    public void SortActionOrder()
    {
        ActionOrder.Sort((a, b) => a.ActionValue.CompareTo(b.ActionValue));

        string debug = "ActionOrder: ";
        foreach (ActionCharacter action in ActionOrder)
        {
            debug += $"(Name:{action.Character.name}, ActionValue:{action.ActionValue}), ";
        }
        Debug.Log(debug);
    }

    public class ActionCharacter
    {
        public Character Character;
        public float ActionValue;

        public ActionCharacter(Character character, float actionValue)
        {
            Character = character;
            ActionValue = actionValue;
            DoTurn();
        }

        public void DoTurn()
        {
            Character.baseStats.CalculateFinalSPD();
            ActionValue += 10000 / Character.baseStats.finalSPD;
        }
    }
}

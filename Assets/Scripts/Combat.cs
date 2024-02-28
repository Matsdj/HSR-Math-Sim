using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.UI;

public class Combat : MonoBehaviour
{
    //Setup
    public Character[] Allies;
    public Character[] Enemies;

    //UI
    public HorizontalLayoutGroup AllyUI;
    public HorizontalLayoutGroup EnemyUI;
    public CombatCharacterPiece CharacterButtonUIPrefab;
    public RectTransform SelectedCharacterUI;
    public VerticalLayoutGroup ActionOrderUI;
    public VerticalLayoutGroup ActionHistoryUI;
    public CharacterGridPiece ActionOrderUIPrefab;

    //Private
    //Runtime
    private List<ActionCharacter> _actionOrder;
    private List<string> _actionHistory;
    private float _latestActionValue = 0;
    private ActionCharacter _Target;
    private List<ActionCharacter>[] _characterLocations;

    public void Awake()
    {
        _actionOrder = new List<ActionCharacter>();
        _characterLocations = new List<ActionCharacter>[2];
        for (int i = 0; i < _characterLocations.Length; i++) _characterLocations[i] = new List<ActionCharacter>();
        foreach (Character character in Allies)
        {
            AddCharacter(character, AllyUI, 0);
        }
        foreach (Character character in Enemies)
        {
            AddCharacter(character, EnemyUI, 1);
        }
        SortActionOrder();
    }

    public void FixedUpdate()
    {
        if (_actionOrder[0].Team != 0)
        {
            DoTurn(_actionOrder[0]);
        }
    }

    public void AddCharacter(Character character, HorizontalLayoutGroup UIparent, int team)
    {
        //Logic
        ActionCharacter action = new ActionCharacter(character, _latestActionValue, team);
        _actionOrder.Add(action);
        _characterLocations[team].Add(action);

        //UI
        CombatCharacterPiece piece = Instantiate(CharacterButtonUIPrefab, UIparent.transform);
        piece.Apply(character, action, Target);
    }

    public void DoTurn(ActionCharacter character)
    {
        character.DoTurn();
        SortActionOrder();
    }

    public void SortActionOrder()
    {
        _actionOrder.Sort((a, b) => a.ActionValue.CompareTo(b.ActionValue));

        foreach (Transform child in ActionOrderUI.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (ActionCharacter character in _actionOrder)
        {
            CharacterGridPiece piece = Instantiate(ActionOrderUIPrefab, ActionOrderUI.transform);
            piece.Apply(character.Character.BasedOfCharacter, OpenActionOrderInfo);
        }

        //string debug = "ActionOrder: ";
        //foreach (ActionCharacter action in ActionOrder)
        //{
        //    debug += $"(Name:{action.Character.BasedOfCharacter.name}, ActionValue:{action.ActionValue}), ";
        //}
        //Debug.Log(debug);
    }

    public void Target(ActionCharacter character, CombatCharacterPiece piece)
    {
        _Target = character;
        SelectedCharacterUI.position = piece.transform.position;
    }

    public void OpenActionOrderInfo(Character character)
    {

    }

    public void Basic()
    {
        AbilityLogic(_actionOrder[0], _actionOrder[0].Character.BasedOfCharacter.Basic, _Target);
        DoTurn(_actionOrder[0]);
    }

    public void AbilityLogic(ActionCharacter character, Ability ability, ActionCharacter target)
    {
        List<ActionCharacter> targets = new List<ActionCharacter>();
        targets.Add(target);

        switch (ability.Targets)
        {
            case Targets.EnemySingle:
                if (_Target.Team != character.Team)
                {
                    Debug.Log($"Do MainEffect. Stat:{character.Character.GetStat(ability.ScalingStat)}, Multiplier:{ability.ScalingPer / 100}");
                    float amount = character.Character.GetStat(ability.ScalingStat) * ability.ScalingPer / 100 + ability.FlatAmount;
                    MainEffect(ability.MainEffect, targets, amount);
                }
                break;
            case Targets.EnemyArea:
                if (_Target.Team != character.Team)
                {
                    
                }
                break;
            case Targets.EnemyAll:

                break;
            case Targets.Self:

                break;
            case Targets.AllySingle:
                if (_Target.Team != character.Team)
                {

                }
                break;
            case Targets.AllyOthers:

                break;
            case Targets.AllyArea:
                if (_Target.Team != character.Team)
                {

                }
                break;
            case Targets.AllyAll:

                break;
            default:
                Debug.LogError("Unimplemented ability target type");
                break;
        }
    }
    public void MainEffect(OtherEffects effect, List<ActionCharacter> targets, float amount)
    {
        switch (effect)
        {
            case OtherEffects.None:
                break;
            case OtherEffects.Heal:
                foreach (ActionCharacter target in targets) target.Character.Stats.CurrentHP += amount;
                break;
            case OtherEffects.Shield:
                break;
            case OtherEffects.Energy:
                foreach (ActionCharacter target in targets) target.Character.Stats.Energy += amount;
                break;
            case OtherEffects.DealDMG:
                foreach (ActionCharacter target in targets) target.Character.Invoke(Triggers.BeforeTakingDamage);
                foreach (ActionCharacter target in targets)
                {
                    Debug.Log($"Deal {amount} damage to {target.Character.BasedOfCharacter.name} with {target.Character.Stats.CurrentHP} HP");
                    target.Character.Stats.CurrentHP -= amount;
                    target.Character.Invoke(Triggers.AfterTakingDamage);
                }
                break;
        }
    }

    public class ActionCharacter
    {
        public RuntimeCharacter Character;
        public float ActionValue;
        public int Team;

        public ActionCharacter(Character character, float actionValue, int team)
        {
            Character = new RuntimeCharacter(character);
            ActionValue = actionValue;
            DoTurn();
            Team = team;
        }

        public void DoTurn()
        {
            Character.Stats.CalculateFinalSPD();
            ActionValue += 10000 / Character.Stats.Final.SPD;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.UI;
using static MathFormulas;

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
    private Team[] _teams;

    public void Awake()
    {
        _actionOrder = new List<ActionCharacter>();
        _teams = new Team[2];
        for (int i = 0; i < _teams.Length; i++) _teams[i] = new Team();
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
        if (_actionOrder[0].TeamId != 0)
        {
            DoTurn(_actionOrder[0]);
        }
    }

    public void AddCharacter(Character character, HorizontalLayoutGroup UIparent, int teamId)
    {
        //Logic
        ActionCharacter action = new ActionCharacter(character, _latestActionValue, teamId, _teams[teamId], _teams[teamId].Count);
        _actionOrder.Add(action);
        _teams[teamId].Add(action);

        //UI
        CombatCharacterPiece piece = Instantiate(CharacterButtonUIPrefab, UIparent.transform);
        piece.Apply(character, action, this);
        piece.AddOnCharacterClickListener(Target);
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
            piece.Apply(character.BasedOfCharacter, OpenActionOrderInfo);
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
        ActionCharacter character = _actionOrder[0];
        if (AbilityLogic(character, character.BasedOfCharacter.Basic, _Target))
        {
            character.Team.SkillPoints++;
            DoTurn(character);
        }
            
    }

    public void Skill()
    {
        ActionCharacter character = _actionOrder[0];
        if (character.Team.SkillPoints > 0 && AbilityLogic(character, character.BasedOfCharacter.Skill, _Target))
        {
            character.Team.SkillPoints--;
            DoTurn(character);
        }
    }

    public void Ult(ActionCharacter character)
    {
        if (character.Energy >= character.BasedOfCharacter.Ultimate.EnergyCost && AbilityLogic(character, character.BasedOfCharacter.Ultimate, _Target))
        {
            character.Energy -= character.BasedOfCharacter.Ultimate.EnergyCost;
            DoTurn(character);
        }
    }

    public void Ult2(ActionCharacter character)
    {
        if (!(character.BasedOfCharacter.Ultimate is DualUlt)) return;
        Ultimate dualUlt = ((DualUlt)character.BasedOfCharacter.Ultimate).Ult2;
        if (character.Energy >= character.BasedOfCharacter.Ultimate.EnergyCost 
            && AbilityLogic(character, dualUlt, _Target))
        {
            character.Energy -= dualUlt.EnergyCost;
            DoTurn(character);
        }
    }

    public bool AbilityLogic(ActionCharacter character, Ability ability, ActionCharacter target)
    {
        character.Energy += ability.EnergyGeneration;
        List<ActionCharacter> targets = new List<ActionCharacter>();
        float amount = character.GetStat(ability.ScalingStat) * ability.ScalingPer / 100 + ability.FlatAmount;

        switch (ability.Targets)
        {
            case Targets.EnemySingle:
                if (target.TeamId == character.TeamId) return false;
                targets.Add(target);
                break;
            case Targets.EnemyArea:
                if (target.TeamId == character.TeamId) return false;
                AreaTargeting(target, targets);
                break;
            case Targets.EnemyAll:
                if (target.TeamId == character.TeamId) return false;
                foreach (ActionCharacter c in target.Team) targets.Add(c);
                break;
            case Targets.Self:
                targets.Add(character);
                break;
            case Targets.AllySingle:
                if (_Target.TeamId != character.TeamId) return false;
                targets.Add(target);
                break;
            case Targets.AllyOthers:
                foreach (ActionCharacter c in character.Team) targets.Add(c);
                targets.Remove(character);
                break;
            case Targets.AllyArea:
                if (_Target.TeamId != character.TeamId) return false;
                AreaTargeting(character, targets);
                break;
            case Targets.AllyAll:
                foreach (ActionCharacter c in character.Team) targets.Add(c);
                break;
            default:
                Debug.LogError("Unimplemented ability target type");
                break;
        }
        MainEffect(character, ability.MainEffect, targets, amount);
        return true;
    }

    public void AreaTargeting(ActionCharacter target, List<ActionCharacter> list)
    {
        list.Add(target);
        if (target.Position > 0) list.Add(target.Team[target.Position - 1]);
        if (target.Position < target.Team.Count - 1) list.Add(target.Team[target.Position + 1]);
    }

    public void MainEffect(ActionCharacter character, OtherEffects effect, List<ActionCharacter> targets, float amount)
    {
        switch (effect)
        {
            case OtherEffects.None:
                break;
            case OtherEffects.Heal:
                foreach (ActionCharacter target in targets) target.CurrentHP += amount;
                break;
            case OtherEffects.Buff:
                break;
            case OtherEffects.Energy:
                foreach (ActionCharacter target in targets) target.Energy += amount;
                break;
            case OtherEffects.DealDMG:
                foreach (ActionCharacter target in targets) target.Invoke(Triggers.BeforeTakingDamage);
                foreach (ActionCharacter target in targets)
                {
                    DoDamage(character, target, amount);
                    target.Invoke(Triggers.AfterTakingDamage);
                }
                break;
        }
    }

    public static void DoDamage(RuntimeCharacter character, RuntimeCharacter target, float baseDMG)
    {
        float crit = Crit(character.Adv.CRIT_Rate / 100, character.Adv.CRIT_DMG / 100);
        float dmg = DMGMultiplier(0); //TODO
        float weaken = WeakenMultiplier(0); //TODO
        float def = DEFMultiplier(character.LVL, target.Base.DEF, 0, 0); //TODO
        float res = RESMultiplier(0, 0); //TODO
        float vulnerability = VulnerabilityMultiplier(0); //TODO
        float dmgReduction = DMGReductionMultiplier(new List<float>()); //TODO
        float broken = BrokenMultiplier(false); //TODO
        float total = baseDMG * crit * dmg * weaken * def * res * vulnerability * dmgReduction * broken;
        target.CurrentHP -= total;
    }

    public class ActionCharacter : RuntimeCharacter
    {
        public float ActionValue;
        public int TeamId;
        public int Position;
        public Team Team;

        public ActionCharacter(Character character, float actionValue, int teamId, Team team, int position) : base(character)
        {
            ActionValue = actionValue;
            DoTurn();
            TeamId = teamId;
            Team = team;
            Position = position;
        }

        public void DoTurn()
        {
            ActionValue += 10000 / Final.SPD;
        }
    }

    public class Team : List<ActionCharacter>
    {
        public int SkillPoints { get { return _skillPoints; } set { _skillPoints = Mathf.Min(value, MaxSkillPoints); } }
        public int MaxSkillPoints = 5;
        private int _skillPoints = 3;
    }
}

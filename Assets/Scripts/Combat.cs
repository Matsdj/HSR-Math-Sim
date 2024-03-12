using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public TMP_Text SkillpointsText;
    public VerticalLayoutGroup ActionOrderUI;
    public VerticalLayoutGroup ActionHistoryUI;
    public CharacterGridPiece ActionOrderUIPrefab;

    //Private
    //Runtime
    private List<RuntimeCharacter> _actionOrder;
    private List<string> _actionHistory;
    private float _latestActionValue = 0;
    private RuntimeCharacter _Target;
    private Team[] _teams;

    public void Awake()
    {
        _actionOrder = new List<RuntimeCharacter>();
        //For now this is hard coded to 2, but is made decently flexible for future plans with more than 2 teams
        _teams = new Team[2];
        for (int i = 0; i < _teams.Length; i++) _teams[i] = new Team();
        _teams[0].SkillpointsUI = SkillpointsText;
        _teams[0].EnemyTeam = _teams[1];
        _teams[1].EnemyTeam = _teams[0];
        foreach (Character character in Allies)
        {
            AddCharacter(character, AllyUI, 0);
        }
        foreach (Character character in Enemies)
        {
            AddCharacter(character, EnemyUI, 1);
        }
        foreach (RuntimeCharacter character in _actionOrder) character.Invoke(Triggers.StartOfCombat, character);
        SortActionOrder();
    }

    public void FixedUpdate()
    {
        if (_actionOrder[0].TeamId != 0)
        {
            DoTurn(_actionOrder[0]);
        }
    }

    public RuntimeCharacter AddCharacter(Character @base, HorizontalLayoutGroup UIparent, int teamId)
    {
        //Logic
        RuntimeCharacter character = new RuntimeCharacter(@base, _latestActionValue, teamId, _teams[teamId], _teams[teamId].Count);
        _actionOrder.Add(character);
        _teams[teamId].Add(character);

        //UI
        CombatCharacterPiece piece = Instantiate(CharacterButtonUIPrefab, UIparent.transform);
        piece.Apply(@base, character, this);
        piece.AddOnCharacterClickListener(Target);

        return character;
    }

    public void DoTurn(RuntimeCharacter character)
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

        foreach (RuntimeCharacter character in _actionOrder)
        {
            CharacterGridPiece piece = Instantiate(ActionOrderUIPrefab, ActionOrderUI.transform);
            piece.Apply(character.BasedOfCharacter, OpenActionOrderInfo);
        }

        //string debug = "ActionOrder: ";
        //foreach (RuntimeCharacter action in ActionOrder)
        //{
        //    debug += $"(Name:{action.Character.BasedOfCharacter.name}, ActionValue:{action.ActionValue}), ";
        //}
        //Debug.Log(debug);
    }

    public void Target(RuntimeCharacter character, CombatCharacterPiece piece)
    {
        _Target = character;
        SelectedCharacterUI.position = piece.transform.position;
    }

    public void OpenActionOrderInfo(Character character)
    {

    }

    public void Talent(RuntimeCharacter character)
    {
        foreach (Mechanics passive in character.BasedOfCharacter.Talent.Passives)
        {
            foreach (Triggers trigger in passive.Trigger)
            {
                foreach(TriggerConditions condition in passive.Condition)
                {
                    switch (condition)
                    {
                        case TriggerConditions.None:
                            break;
                        case TriggerConditions.ImTheCause:
                            break;
                        case TriggerConditions.AllyIsTheCause:
                            break;
                        case TriggerConditions.EnemyIsTheCause:
                            break;
                        case TriggerConditions.ImReceiver:
                            character.Events[trigger].Add(passive, character);
                            break;
                        case TriggerConditions.AllyIsTheReceiver:
                            foreach (RuntimeCharacter ally in character.Team)
                            {
                                if (ally != character) ally.Events[trigger].Add(passive, character);
                            }
                            break;
                        case TriggerConditions.EnemyIsTheReceiver:
                            character.Team.EnemyTeam.Events[trigger].Add(passive, character);
                            break;
                        default:
                            Debug.LogError($"Unimplemented condition: {condition}, {Enum.GetName(typeof(TriggerConditions), condition)}");
                            break;
                    }
                }
            }
        }
    }

    public void Basic()
    {
        RuntimeCharacter character = _actionOrder[0];
        if (AbilityLogic(character, character.BasedOfCharacter.Basic, _Target))
        {
            character.Team.SkillPoints++;
            DoTurn(character);
        }
    }

    public void Skill()
    {
        RuntimeCharacter character = _actionOrder[0];
        if (character.Team.SkillPoints > 0 && AbilityLogic(character, character.BasedOfCharacter.Skill, _Target))
        {
            character.Team.SkillPoints--;
            DoTurn(character);
        }
    }

    public void Ult(RuntimeCharacter character)
    {
        if (character.Energy >= character.BasedOfCharacter.Ultimate.EnergyCost && AbilityLogic(character, character.BasedOfCharacter.Ultimate, _Target))
        {
            character.Energy -= character.BasedOfCharacter.Ultimate.EnergyCost;
            DoTurn(character);
        }
    }

    public void Ult2(RuntimeCharacter character)
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

    public bool AbilityLogic(RuntimeCharacter character, Ability ability, RuntimeCharacter target)
    {
        character.Invoke(Triggers.BeforeAttack);
        character.Energy += ability.EnergyGeneration;
        List<RuntimeCharacter> targets = GetTargets(character, ability.Targets, target);
        if (targets.Count == 0) return false;
        float amount = character.GetStat(ability.ScalingStat) * ability.ScalingPer / 100 + ability.FlatAmount;
        MainEffect(character, ability.MainEffect, targets, amount);
        character.Invoke(Triggers.AfterAttack);
        return true;
    }

    public static List<RuntimeCharacter> GetTargets(RuntimeCharacter character, Targets targetType, RuntimeCharacter target)
    {
        List<RuntimeCharacter> targets = new List<RuntimeCharacter>();
        switch (targetType)
        {
            case Targets.EnemySingle:
                if (target.TeamId != character.TeamId) targets.Add(target);
                break;
            case Targets.EnemyArea:
                if (target.TeamId != character.TeamId) AreaTargeting(target, targets);
                break;
            case Targets.EnemyAll:
                foreach (RuntimeCharacter c in character.Team.EnemyTeam) targets.Add(c);
                break;
            case Targets.Self:
                targets.Add(character);
                break;
            case Targets.AllySingle:
                if (target.TeamId == character.TeamId) targets.Add(target);
                break;
            case Targets.AllyOthers:
                foreach (RuntimeCharacter c in character.Team) targets.Add(c);
                targets.Remove(character);
                break;
            case Targets.AllyArea:
                if (target.TeamId == character.TeamId) AreaTargeting(character, targets);
                break;
            case Targets.AllyAll:
                foreach (RuntimeCharacter c in character.Team) targets.Add(c);
                break;
            default:
                Debug.LogError("Unimplemented ability target type");
                break;
        }
        return targets;
    }

    public static void AreaTargeting(RuntimeCharacter target, List<RuntimeCharacter> list)
    {
        list.Add(target);
        if (target.Position > 0) list.Add(target.Team[target.Position - 1]);
        if (target.Position < target.Team.Count - 1) list.Add(target.Team[target.Position + 1]);
    }

    public static void MainEffect(RuntimeCharacter cause, OtherEffects effect, List<RuntimeCharacter> targets, float amount)
    {
        switch (effect)
        {
            case OtherEffects.None:
                break;
            case OtherEffects.Heal:
                foreach (RuntimeCharacter target in targets)
                {
                    target.CurrentHP += amount;
                    target.Invoke(Triggers.Heal, cause);
                }
                break;
            case OtherEffects.Buff:
                break;
            case OtherEffects.Energy:
                foreach (RuntimeCharacter target in targets)
                {
                    target.Energy += amount;
                    target.Invoke(Triggers.EnergyChange, cause);
                }
                break;
            case OtherEffects.DealDMG:
                foreach (RuntimeCharacter target in targets) target.Invoke(Triggers.BeforeTakingDamage, cause);
                foreach (RuntimeCharacter target in targets)
                {
                    DoDamage(cause, target, amount);
                    target.Invoke(Triggers.AfterTakingDamage, cause);
                }
                break;
        }
    }

    public static void DoDamage(RuntimeCharacter cause, RuntimeCharacter receiver, float baseDMG)
    {
        float crit = Crit(cause.Adv.CRIT_Rate / 100, cause.Adv.CRIT_DMG / 100);
        float dmg = DMGMultiplier(0); //TODO
        float weaken = WeakenMultiplier(0); //TODO
        float def = DEFMultiplier(cause.LVL, receiver.Base.DEF, 0, 0); //TODO
        float res = RESMultiplier(0, 0); //TODO
        float vulnerability = VulnerabilityMultiplier(0); //TODO
        float dmgReduction = DMGReductionMultiplier(new List<float>()); //TODO
        float broken = BrokenMultiplier(false); //TODO
        float total = baseDMG * crit * dmg * weaken * def * res * vulnerability * dmgReduction * broken;
        receiver.CurrentHP -= total;
    }

    public class Team : List<RuntimeCharacter>
    {
        public int SkillPoints
        {
            get { return _skillPoints; }
            set
            {
                _skillPoints = Mathf.Min(value, MaxSkillPoints);
                if (SkillpointsUI) SkillpointsUI.text = $"{_skillPoints}/{MaxSkillPoints}";
            }
        }
        public int MaxSkillPoints = 5;
        private int _skillPoints = 3;

        public TMP_Text SkillpointsUI;

        /// <summary>Each event has Reciever first and cause as the second variable</summary>
        public CharacterEvents Events;

        public Team EnemyTeam;

        public Team()
        {
            Events = new CharacterEvents();
        }
    }

    public class CharacterEvents : Dictionary<Triggers, CharacterAction>
    {
        public CharacterEvents()
        {
            Triggers[] triggers = (Triggers[])Enum.GetValues(typeof(Triggers));
            foreach (Triggers trigger in triggers)
            {
                Add(trigger, new CharacterAction());
            }
        }
        public void Invoke(Triggers trigger, RuntimeCharacter receiver, RuntimeCharacter cause)
        {
            this[trigger].Invoke(receiver, cause);
        }
    }

    public class CharacterAction : List<Action<RuntimeCharacter, RuntimeCharacter>>
    {
        public List<CharacterMechanicOnTrigger> Mechanics;

        public CharacterAction()
        {
            Mechanics = new List<CharacterMechanicOnTrigger>();
        }

        public void Add(Mechanics mechanic, RuntimeCharacter source)
        {
            Mechanics.Add(new CharacterMechanicOnTrigger(mechanic, source));
        }

        public void Invoke(RuntimeCharacter receiver, RuntimeCharacter cause)
        {
            foreach (Action<RuntimeCharacter, RuntimeCharacter> action in this)
            {
                action.Invoke(receiver, cause);
            }
            foreach(CharacterMechanicOnTrigger mechanic in Mechanics)
            {
                mechanic.Invoke(receiver, cause);
            }
        }

        public class CharacterMechanicOnTrigger
        {
            private Mechanics _mechanic;
            private RuntimeCharacter _source;

            public CharacterMechanicOnTrigger(Mechanics mechanic, RuntimeCharacter source)
            {
                _mechanic = mechanic;
                _source = source;
            }

            public void Invoke(RuntimeCharacter receiver, RuntimeCharacter cause)
            {
                if (CorrectCause(cause)) Effect(Combat.GetTargets(_source, _mechanic.target, null));
            }

            private bool CorrectCause(RuntimeCharacter cause)
            {
                foreach (TriggerConditions condition in _mechanic.Condition)
                {
                    switch (condition)
                    {
                        case TriggerConditions.None:
                            break;
                        case TriggerConditions.ImTheCause:
                            if (_source == cause) return true;
                            break;
                        case TriggerConditions.AllyIsTheCause:
                            foreach (RuntimeCharacter ally in _source.Team)
                            {
                                if (ally != _source && ally == cause) return true;
                            }
                            break;
                        case TriggerConditions.EnemyIsTheCause:
                            foreach (RuntimeCharacter enemy in _source.Team.EnemyTeam)
                            {
                                if (enemy == cause) return true;
                            }
                            break;
                        case TriggerConditions.ImReceiver:
                            break;
                        case TriggerConditions.AllyIsTheReceiver:
                            break;
                        case TriggerConditions.EnemyIsTheReceiver:
                            break;
                        default:
                            Debug.LogError($"Unimplemented condition: {condition}, {Enum.GetName(typeof(TriggerConditions), condition)}");
                            break;
                    }
                }
                return false;
            }

            private void Effect(List<RuntimeCharacter> characters)
            {
                foreach(var character in characters)
                {
                    character.Effects.Add(_mechanic.effect);
                }
            }
        }
    }
}

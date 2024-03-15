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
    //Singleton
    public static Combat instance;

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
    public Info Info;
    public CharacterGridPiece ActionOrderUIPrefab;
    public ActionHistoryPiece ActionHistoryUIPrefab;

    //Runtime
    private List<RuntimeCharacter> _actionOrder;
    private List<ActionHistoryPiece> _actionHistory;
    private float _currentActionValue = 0;
    public float CurrentActionValue { get => _currentActionValue; }
    private RuntimeCharacter _Target;
    private Team[] _teams;

    public void Awake()
    {
        //Singleton
        if (instance == null) instance = this;
        else
        {
            Debug.LogError("Combat already has an instance!");
            Destroy(gameObject);
        }

        _actionOrder = new List<RuntimeCharacter>();
        _actionHistory = new List<ActionHistoryPiece>();
        _actionHistory.Add(Instantiate(ActionHistoryUIPrefab, ActionHistoryUI.transform));
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
        _actionOrder[0].Invoke(Triggers.OnTurnStart);
        _actionHistory[0].Setup(_actionOrder[0], 0);
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
        RuntimeCharacter character = new RuntimeCharacter(@base, _currentActionValue, teamId, _teams[teamId], _teams[teamId].Count);
        _actionOrder.Add(character);
        _teams[teamId].Add(character);
        character.Invoke(Triggers.EnemyEnterField);
        TalentSetup(character);

        //UI
        CombatCharacterPiece piece = Instantiate(CharacterButtonUIPrefab, UIparent.transform);
        piece.Apply(@base, character, this);
        piece.AddOnCharacterClickListener(Target);

        return character;
    }

    public void DoTurn(RuntimeCharacter character)
    {
        character.Invoke(Triggers.OnTurnEnd);
        character.DoTurn();
        SortActionOrder();
        _currentActionValue = _actionOrder[0].ActionValue;
        _actionHistory.Add(Instantiate(ActionHistoryUIPrefab, ActionHistoryUI.transform).Setup(_actionOrder[0], _actionHistory.Count));
        _actionOrder[0].Invoke(Triggers.OnTurnStart);
        
        
    }

    public void SortActionOrder()
    {
        _actionOrder.Sort((a, b) => a.ActionValue.CompareTo(b.ActionValue));
        
        foreach (Transform child in ActionOrderUI.transform)
        {
            Destroy(child.gameObject);
        }

        float actionValueDiff = _actionOrder[0].ActionValue - _currentActionValue;
        foreach (RuntimeCharacter character in _actionOrder)
        {
            character.UpdateTurnPercentage(actionValueDiff, _currentActionValue);

            //UI
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

    public void TalentSetup(RuntimeCharacter character)
    {
        if (character.BasedOfCharacter.Talent == null) return;
        foreach (Mechanics passive in character.BasedOfCharacter.Talent.Passives)
        {
            foreach (Triggers trigger in passive.Trigger)
            {
                //Debug.Log($"Adding Talent to Trigger:{Enum.GetName(typeof(Triggers), trigger)}");
                foreach (TriggerConditions condition in passive.TriggerCondition)
                {
                    switch (condition)
                    {
                        case TriggerConditions.None:
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
                    //Debug.Log($"{character.BasedOfCharacter.name} now has {character.Events[trigger].Mechanics.Count} Mechanics added to it's CharacterActions. Condition:{condition}");
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
        BaseUlt(character, character.BasedOfCharacter.Ultimate);
    }

    public void Ult2(RuntimeCharacter character)
    {
        if (!(character.BasedOfCharacter.Ultimate is DualUlt)) return;
        Ultimate dualUlt = ((DualUlt)character.BasedOfCharacter.Ultimate).Ult2;
        BaseUlt(character, dualUlt);
    }

    private void BaseUlt(RuntimeCharacter character, Ultimate ultimate)
    {
        if (character.Energy >= ultimate.EnergyCost && AbilityLogic(character, ultimate, _Target))
        {
            character.Energy -= ultimate.EnergyCost;
        }
    }

    public bool AbilityLogic(RuntimeCharacter character, Ability ability, RuntimeCharacter target)
    {
        List<RuntimeCharacter> targets = GetTargets(character, ability.Targets, target);
        if (targets.Count == 0) return false;
        //Before triggers
        character.Invoke(Triggers.BeforeAttack);
        if (ability.AbilityType == AbilityType.Skill) character.Invoke(Triggers.BeforeUlt);
        if (ability.AbilityType == AbilityType.Ultimate) character.Invoke(Triggers.BeforeUlt);
        //Do Effect
        character.Energy += ability.EnergyGeneration;
        float amount = character.GetStat(ability.ScalingStat) * ability.ScalingPer / 100 + ability.FlatAmount;
        AddTurnInfo($"{ability.name}");
        MainEffect(character, ability.MainEffect, targets, amount);
        //After triggers
        character.Invoke(Triggers.AfterAttack);
        if (ability.AbilityType == AbilityType.Skill) character.Invoke(Triggers.AfterSkill);
        if (ability.AbilityType == AbilityType.Ultimate) character.Invoke(Triggers.AfterUlt);

        return true;
    }

    public static List<RuntimeCharacter> GetTargets(RuntimeCharacter character, Targets targetType, RuntimeCharacter target)
    {
        List<RuntimeCharacter> targets = new List<RuntimeCharacter>();
        switch (targetType)
        {
            case Targets.EnemySingle:
                if (target == null) return targets;
                if (target.TeamId != character.TeamId) targets.Add(target);
                break;
            case Targets.EnemyArea:
                if (target == null) return targets;
                if (target.TeamId != character.TeamId) AreaTargeting(target, targets);
                break;
            case Targets.EnemyAll:
                foreach (RuntimeCharacter c in character.Team.EnemyTeam) targets.Add(c);
                break;
            case Targets.Self:
                targets.Add(character);
                break;
            case Targets.AllySingle:
                if (target == null) return targets;
                if (target.TeamId == character.TeamId) targets.Add(target);
                break;
            case Targets.AllyOthers:
                foreach (RuntimeCharacter c in character.Team) targets.Add(c);
                targets.Remove(character);
                break;
            case Targets.AllyArea:
                if (target == null) return targets;
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

    public void MainEffect(RuntimeCharacter cause, OtherEffects effect, List<RuntimeCharacter> targets, float amount, RuntimeEffect cause2 = null)
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
                    AddTurnInfo($"Heal:{amount}");
                }
                break;
            case OtherEffects.Shield:
                foreach (RuntimeCharacter target in targets)
                {
                    target.Shields.Add(cause2 ,amount);
                    AddTurnInfo($"New Shield:{amount}");
                }
                break;
            case OtherEffects.Energy:
                foreach (RuntimeCharacter target in targets)
                {
                    target.Energy += amount;
                    AddTurnInfo($"Energy Regen:{amount}");
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
        string targetsInfo = "Targets:";
        foreach (RuntimeCharacter target in targets) targetsInfo += target.Name + ",";
    }

    public void DoDamage(RuntimeCharacter cause, RuntimeCharacter receiver, float baseDMG)
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
        float excess = receiver.DoDamageToShield(total);
        receiver.CurrentHP -= excess;
        AddTurnInfo($"Total DMG:{total}");
    }

    public void AddTurnInfo(string info)
    {
        ref string body = ref _actionHistory[_actionHistory.Count - 1].Body;
        if (body.Length == 0) info = info.TrimStart('\n');
        body += info + " ";
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
        public new int Count { get => base.Count + Mechanics.Count; }

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
            foreach (CharacterMechanicOnTrigger mechanic in Mechanics)
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
                bool correctCause = CorrectCause(cause);
                if (correctCause) Effect(GetTargets(_source, _mechanic.target, null));
                Combat.instance.AddTurnInfo($"[{_mechanic.effect.name} Applied:{correctCause}]");
            }

            private bool CorrectCause(RuntimeCharacter cause)
            {
                if (_mechanic.CauseCondition.Length == 0) return true;
                foreach (CauseConditions condition in _mechanic.CauseCondition)
                {
                    switch (condition)
                    {
                        case CauseConditions.None:
                            break;
                        case CauseConditions.ImTheCause:
                            if (_source == cause) return true;
                            break;
                        case CauseConditions.AllyIsTheCause:
                            foreach (RuntimeCharacter ally in _source.Team)
                            {
                                if (ally != _source && ally == cause) return true;
                            }
                            break;
                        case CauseConditions.EnemyIsTheCause:
                            foreach (RuntimeCharacter enemy in _source.Team.EnemyTeam)
                            {
                                if (enemy == cause) return true;
                            }
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
                foreach (var character in characters)
                {
                    character.Effects.Add(_mechanic.effect);
                }
            }
        }
    }
}

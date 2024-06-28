using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Types;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Combat.CharacterAction;
using static MathFormulas;

public class Combat : MonoBehaviour
{
    //Singleton
    public static Combat instance;

    //Setup
    public Character[] Allies;
    public Character[] Enemies;
    public Effect[] BreakEffects;

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
    private RuntimeCharacter _target;
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
        _actionHistory = new List<ActionHistoryPiece> { Instantiate(ActionHistoryUIPrefab, ActionHistoryUI.transform) };
        //For now this is hard coded to 2, but is made decently flexible for future plans with more than 2 teams
        _teams = new Team[2];
        for (int i = 0; i < _teams.Length; i++) _teams[i] = new Team(i);
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
        if (_actionOrder[0].Team.Id != 0)
        {
            DoTurn(_actionOrder[0]);
        }
    }

    public RuntimeCharacter AddCharacter(Character @base, HorizontalLayoutGroup UIparent, int teamId)
    {
        //Logic
        RuntimeCharacter character = new RuntimeCharacter(@base, _currentActionValue, _teams[teamId], _teams[teamId].Count, _actionOrder.Count);
        _actionOrder.Add(character);
        _teams[teamId].Add(character);
        character.Invoke(Triggers.EnterField);
        TalentSetup(character);

        //UI
        CombatCharacterPiece piece = Instantiate(CharacterButtonUIPrefab, UIparent.transform);
        piece.Apply(@base, character, this);
        piece.AddOnCharacterClickListener(Target);

        return character;
    }

    public void RemoveCharacter(RuntimeCharacter character)
    {
        _actionOrder.Remove(character);
        _teams[character.Team.Id].Remove(character);
    }

    public void DoTurn(RuntimeCharacter character)
    {
        character.Invoke(Triggers.OnTurnEnd);
        character.DoTurn();
        _currentActionValue = _actionOrder[0].ActionValue;
        _actionHistory.Add(Instantiate(ActionHistoryUIPrefab, ActionHistoryUI.transform).Setup(_actionOrder[0], _actionHistory.Count));
        _actionOrder[0].Invoke(Triggers.OnTurnStart);
    }

    public void SortActionOrder()
    {
        if (_actionOrder.Count == 0) return;
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
        _target = character;
        SelectedCharacterUI.position = piece.transform.position;
    }

    public void OpenActionOrderInfo(Character character)
    {

    }

    public void TalentSetup(RuntimeCharacter character)
    {
        character.AddPassives();
    }

    public static void AddPassives(Mechanics[] passives, RuntimeCharacter character, RuntimeCharacter.SimpleFunc removeEvent)
    {
        foreach (Mechanics passive in passives)
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
                            character.Events[trigger].Add(passive, character, removeEvent);
                            break;
                        case TriggerConditions.AllyIsTheReceiver:
                            foreach (RuntimeCharacter ally in character.Team)
                            {
                                if (ally != character) ally.Events[trigger].Add(passive, character, removeEvent);
                            }
                            break;
                        case TriggerConditions.EnemyIsTheReceiver:
                            character.Team.EnemyTeam.Events[trigger].Add(passive, character, removeEvent);
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
        if (AbilityLogic(character, character.BasedOfCharacter.Basic, _target))
        {
            character.Team.SkillPoints++;
        }
    }

    public void Skill()
    {
        RuntimeCharacter character = _actionOrder[0];
        if (character.Team.SkillPoints > 0 && AbilityLogic(character, character.BasedOfCharacter.Skill, _target))
        {
            character.Team.SkillPoints--;
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
        if (character.Energy >= ultimate.EnergyCost && AbilityLogic(character, ultimate, _target))
        {
            character.Energy -= ultimate.EnergyCost;
        }
    }

    public bool AbilityLogic(RuntimeCharacter character, Ability ability, RuntimeCharacter target)
    {
        //Change Ability depending on effect
        if (ability.ChangeAbilityWhenThisEffect != null)
        {
            RuntimeEffect effect = character.Effects.HasEffect(ability.ChangeAbilityWhenThisEffect);
            if (effect != null)
            {
                int i = effect.StackCount - 1;
                return AbilityLogic(character, ability.ChangeAbilityToThis[i], target);
            }
        }
        //Skill Points
        if (ability.Skillpoints < 0)
        {
            if (character.Team.SkillPoints >= -ability.Skillpoints) character.Team.SkillPoints += ability.Skillpoints;
            else return false;
        }
        if (ability.Skillpoints > 0) character.Team.SkillPoints += ability.Skillpoints;
        //Targets
        TargetCharacters targets = new TargetCharacters(character, ability.Targets, target, character);
        if (targets.Count == 0) return false;
        //Before triggers
        character.Invoke(Triggers.BeforeAttack);
        if (ability.IsOfType(AbilityType.Skill)) character.Invoke(Triggers.BeforeUlt);
        if (ability.IsOfType(AbilityType.Ultimate)) character.Invoke(Triggers.BeforeUlt);
        //Do Effect
        character.Energy += ability.EnergyGeneration;
        float amount = BaseAmount(ability.ScalingPer, 0, character.GetStat(ability.ScalingStat), ability.FlatAmount);
        AddTurnInfo($"{ability.name}");
        MainEffect(character, ability.MainEffect, ability.AbilityTypes, targets, amount);
        WeaknessBreak(targets, ability.WeaknessBreak, ability.ExtraWeaknessBreakForMainTarget);
        Effect(targets, ability.ApplyEffectToTarget);
        //Trigger Abilities
        foreach (Ability TriggeredAbility in ability.TriggerThisAbility)
        {
            AbilityLogic(character, TriggeredAbility, target);
        }
        //After triggers
        character.Invoke(Triggers.AfterAttack);
        if (ability.IsOfType(AbilityType.Skill)) character.Invoke(Triggers.AfterSkill);
        if (ability.IsOfType(AbilityType.Ultimate)) character.Invoke(Triggers.AfterUlt);

        if (ability.EndTurn && _actionOrder[0] == character) DoTurn(character);
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cause"></param>
    /// <param name="effect"></param>
    /// <param name="targets"></param>
    /// <param name="amount"></param>
    /// <param name="cause2">This is needed for shields to make sure the effect is removed when the shield hits 0</param>
    public void MainEffect(RuntimeCharacter cause, OtherEffects effect, AbilityType[] abilityTypes, List<RuntimeCharacter> targets, float amount, RuntimeEffect cause2 = null)
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
                    target.Shields.Add(cause2, amount);
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
            case OtherEffects.ActionAdvance:
                foreach (RuntimeCharacter target in targets)
                {
                    target.ActionAdvanceForward(amount);
                    AddTurnInfo($"Action Advanceforward:{amount}");
                }
                break;
            case OtherEffects.DealDMG:
                foreach (RuntimeCharacter target in targets) target.Invoke(Triggers.BeforeTakingDamage, cause);
                foreach (RuntimeCharacter target in targets)
                {
                    DoDamage(cause, target, amount, true, abilityTypes);
                    target.Invoke(Triggers.AfterTakingDamage, cause);
                }
                break;
            case OtherEffects.SkipTurn:
                foreach (RuntimeCharacter target in targets) DoTurn(target);
                break;
        }
        string targetsInfo = "Targets:";
        foreach (RuntimeCharacter target in targets) targetsInfo += target.Name + ",";
        AddTurnInfo(targetsInfo);
    }

    public void DoDamage(RuntimeCharacter cause, RuntimeCharacter receiver, float baseDMG, bool canCrit, AbilityType[] abilityTypes)
    {
        Element element = cause.BasedOfCharacter.element;
        float crit = canCrit ? Crit(cause.Adv.CRIT_Rate / 100, cause.Adv.CRIT_DMG / 100) : 1;
        float dmg = (abilityTypes.Length == 0) ? 1: DMGMultiplier(cause.Adv.ElementDMG.Get(element) + cause.Adv.AbilityDMG.Get(abilityTypes));
        float weaken = WeakenMultiplier(cause.Adv.Weaken);
        float def = DEFMultiplier(cause.LVL, receiver.Base.DEF, receiver.Per.DEF, cause.Adv.DEF_PEN, receiver.Flat.DEF);
        float res = RESMultiplier(receiver.Adv.ElementRES.Get(element), cause.Adv.RES_PEN.Get(element));
        float vulnerability = VulnerabilityMultiplier(receiver.Adv.Vulnerability);
        float dmgReduction = DMGReductionMultiplier(cause.Adv.DMGReduction);
        float broken = BrokenMultiplier(receiver.IsWeaknessBroken);
        float total = baseDMG * crit * dmg * weaken * def * res * vulnerability * dmgReduction * broken;
        float excess = receiver.DoDamageToShield(total);
        receiver.CurrentHP -= excess;
        AddTurnInfo($"Total DMG:{total}");
    }

    public static void Effect(TargetCharacters targets, Effect effect)
    {
        if (!effect) return;
        foreach (var target in targets)
        {
            Effect(target, targets.Cause, effect);
        }
    }

    public static void Effect(RuntimeCharacter target, RuntimeCharacter cause, Effect effect)
    {
        bool hit = UnityEngine.Random.value < (effect.IsFixed ? effect.BaseChanceToApply : effect.BaseChanceToApply * cause.Adv.EffectHitRate * (1 - target.Adv.EffectRES));
        if (hit) target.Effects.Add(effect, cause);
    }

    public void WeaknessBreak(TargetCharacters targets, float amount, float mainAmount)
    {
        foreach (var target in targets) WeaknessBreak(target, amount, targets.Cause);
        WeaknessBreak(targets.Main, mainAmount, targets.Cause);
    }

    public void WeaknessBreak(RuntimeCharacter target, float amount, RuntimeCharacter cause)
    {
        if (target.Adv.Toughness > 0) target.Adv.Toughness -= amount;
        if (target.Adv.Toughness <= 0 && !target.IsWeaknessBroken)
        {
            AddTurnInfo($"WeaknessBreaked:{target.Name}, Cause:{cause.Name}");
            target.IsWeaknessBroken = true;
            target.Invoke(Triggers.WeaknessBreak, cause);
            target.ActionAdvanceForward(-0.25f);
            float MaxToughnessMultiplier = 0.5f + target.BasedOfCharacter.advancedStats.Toughness / 120;
            float baseDMG = MaxToughnessMultiplier * cause.GetStat(Stats.LVLMultiplier) * cause.Adv.BreakEffect / 100;

            AbilityType[] abilityTypes = new AbilityType[] { };
            switch (cause.BasedOfCharacter.element)
            {
                case Element.None:
                    break;
                case Element.All:
                    break;
                case Element.Physical:
                    DoDamage(cause, target, 2 * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[0]);
                    break;
                case Element.Fire:
                    DoDamage(cause, target, 2 * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[1]);
                    break;
                case Element.Ice:
                    DoDamage(cause, target, 1 * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[2]);
                    break;
                case Element.Lightning:
                    DoDamage(cause, target, 1 * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[3]);
                    break;
                case Element.Wind:
                    DoDamage(cause, target, 1.5f * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[4]);
                    Effect(target, cause, BreakEffects[4]);
                    Effect(target, cause, BreakEffects[4]);
                    break;
                case Element.Quantum:
                    DoDamage(cause, target, .5f * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[5]);
                    break;
                case Element.Imaginary:
                    DoDamage(cause, target, .5f * baseDMG, false, abilityTypes);
                    Effect(target, cause, BreakEffects[6]);
                    break;
                default:
                    break;
            }
        }
    }

    public static void AddTurnInfo(string info)
    {
        ref string body = ref instance._actionHistory[instance._actionHistory.Count - 1].Body;
        if (body.Length == 0) info = info.TrimStart('\n');
        body += info + " ";
    }

    public class TargetCharacters : List<RuntimeCharacter>
    {
        public RuntimeCharacter Main;
        public RuntimeCharacter Cause;

        public TargetCharacters(RuntimeCharacter character, Targets targetType, RuntimeCharacter target, RuntimeCharacter cause)
        {
            switch (targetType)
            {
                case Targets.EnemySingle:
                    if (target != null && target.Team.Id == character.Team.EnemyTeam.Id) Add(target);
                    break;
                case Targets.EnemyArea:
                    if (target != null && target.Team.Id == character.Team.EnemyTeam.Id) AreaTargeting(target);
                    break;
                case Targets.EnemyAll:
                    if (target != null && target.Team.Id == character.Team.EnemyTeam.Id) Main = target;
                    foreach (RuntimeCharacter c in character.Team.EnemyTeam) Add(c);
                    break;
                case Targets.EnemyRandom:
                    int id = UnityEngine.Random.Range(0, character.Team.EnemyTeam.Count);
                    Add(character.Team.EnemyTeam[id]);
                    break;
                case Targets.Self:
                    Add(character);
                    break;
                case Targets.AllySingle:
                    if (target != null && target.Team.Id == character.Team.Id) Add(target);
                    break;
                case Targets.AllyOthers:
                    foreach (RuntimeCharacter c in character.Team) Add(c);
                    Remove(character);
                    break;
                case Targets.AllyArea:
                    if (target != null && target.Team.Id == character.Team.Id) AreaTargeting(target);
                    break;
                case Targets.AllyAll:
                    if (target != null && target.Team.Id == character.Team.Id) Main = target;
                    foreach (RuntimeCharacter c in character.Team) Add(c);
                    break;
                default:
                    Debug.LogError("Unimplemented ability target type");
                    break;
            }
            Cause = cause;
        }

        public new void Add(RuntimeCharacter character)
        {
            if (Main == null) Main = character;
            base.Add(character);
        }

        private void AreaTargeting(RuntimeCharacter target)
        {
            Add(target);
            if (target.Position > 0) Add(target.Team[target.Position - 1]);
            if (target.Position < target.Team.Count - 1) Add(target.Team[target.Position + 1]);
        }
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

        public int Id;

        public Team(int id)
        {
            Events = new CharacterEvents();
            Id = id;
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

        public void Add(Mechanics mechanic, RuntimeCharacter source, RuntimeCharacter.SimpleFunc removeFunction)
        {
            CharacterMechanicOnTrigger characterMechanicOnTrigger = new CharacterMechanicOnTrigger(mechanic, source, Mechanics);
            Mechanics.Add(characterMechanicOnTrigger);
            if (removeFunction != null) removeFunction += characterMechanicOnTrigger.RemoveFromList;
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
            private List<CharacterMechanicOnTrigger> _list;

            public CharacterMechanicOnTrigger(Mechanics mechanic, RuntimeCharacter source, List<CharacterMechanicOnTrigger> list)
            {
                _mechanic = mechanic;
                _source = source;
                _list = list;
            }

            public void Invoke(RuntimeCharacter receiver, RuntimeCharacter cause)
            {
                bool correctCause = CorrectCause(cause);
                if (correctCause) Effect(new TargetCharacters(_source, _mechanic.target, null, _source), _mechanic.effect);
                AddTurnInfo($"[{_mechanic.effect.name} Applied:{correctCause}]");
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

            public void RemoveFromList()
            {
                _list.Remove(this);
            }
        }
    }
}

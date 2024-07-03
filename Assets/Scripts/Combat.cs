using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Types;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Ability;
using static Combat.CharacterAction;
using static MathFormulas;
using static UnityEngine.GraphicsBuffer;

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
        TargetCharacters targets = new TargetCharacters(character, target, ability.Targets);
        if (targets.Count == 0) return false;
        //Before triggers
        character.Invoke(Triggers.BeforeAttack);
        if (ability.IsOfType(AbilityType.Skill)) character.Invoke(Triggers.BeforeUlt);
        if (ability.IsOfType(AbilityType.Ultimate)) character.Invoke(Triggers.BeforeUlt);
        //Do Effect
        character.Energy += ability.EnergyGeneration;
        float amount = BaseAmount(ability.ScalingPer, 0, character.GetStat(ability.ScalingStat), ability.FlatAmount);
        float adjacentAmount = BaseAmount(ability.ScalingPerForNonMainTargets, 0, character.GetStat(ability.ScalingStat), ability.FlatAmount);
        AddTurnInfo($"{ability.name}");
        MainEffect(character, ability, targets, amount, adjacentAmount);
        WeaknessBreak(targets, ability.WeaknessBreak, ability.WeaknessBreakForNonMainTargets);
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
    public void MainEffect(RuntimeCharacter cause, OtherEffects effect, AbilityType[] abilitytypes, TargetCharacters targets, float amount, float adjacentAmount, Ability ability, RuntimeEffect cause2)
    {
        switch (effect)
        {
            case OtherEffects.None:
                break;
            case OtherEffects.Heal:
                foreach (RuntimeCharacter target in targets)
                {
                    float heal = targets.Amount(target, amount, adjacentAmount);
                    target.CurrentHP += heal;
                    target.Invoke(Triggers.Heal, cause);
                    AddTurnInfo($"Heal:{heal}");
                }
                break;
            case OtherEffects.Shield:
                foreach (RuntimeCharacter target in targets)
                {
                    if (cause2 != null)
                    {
                        float shield = targets.Amount(target, amount, adjacentAmount);
                        target.Shields.Add(cause2, shield);
                        AddTurnInfo($"New Shield:{shield}");
                    }
                    else
                    {
                        Debug.LogError("Tried to create a shield without an associated effect");
                    }
                }
                break;
            case OtherEffects.Energy:
                foreach (RuntimeCharacter target in targets)
                {
                    float energy = targets.Amount(target, amount, adjacentAmount);
                    target.Energy += energy;
                    AddTurnInfo($"Energy Regen:{energy}");
                }
                break;
            case OtherEffects.ActionAdvance:
                foreach (RuntimeCharacter target in targets)
                {
                    float advance = targets.Amount(target, amount, adjacentAmount);
                    target.ActionAdvanceForward(advance);
                    AddTurnInfo($"Action Advanceforward:{advance}");
                }
                break;
            case OtherEffects.DealDMG:
                foreach (RuntimeCharacter target in targets) target.Invoke(Triggers.BeforeTakingDamage, cause);
                if (ability != null) DoMultihitDamage(cause, targets, ability, amount, adjacentAmount, abilitytypes);
                else DoDamage(cause, targets.Main, amount, false, abilitytypes);
                break;
            case OtherEffects.SkipTurn:
                foreach (RuntimeCharacter target in targets) DoTurn(target);
                break;
        }
        string targetsInfo = "Targets:";
        foreach (RuntimeCharacter target in targets) targetsInfo += target.Name + ",";
        AddTurnInfo(targetsInfo);
    }

    /// <summary>
    /// MainEffect version for abilities
    /// </summary>
    /// <param name="cause"></param>
    /// <param name="ability"></param>
    /// <param name="targets"></param>
    /// <param name="amount"></param>
    public void MainEffect(RuntimeCharacter cause, Ability ability, TargetCharacters targets, float amount, float adjacentAmount)
    {
        MainEffect(cause, ability.MainEffect, ability.AbilityTypes, targets, amount, adjacentAmount, ability, null);
    }

    /// <summary>
    /// MainEffect version for effects
    /// </summary>
    /// <param name="cause"></param>
    /// <param name="targets"></param>
    /// <param name="amount"></param>
    public void MainEffect(RuntimeEffect cause, OtherEffect otherEffect, TargetCharacters targets, float amount, float adjacentAmount)
    {
        MainEffect(cause.Cause, otherEffect.Effect, new AbilityType[] { AbilityType.Effect }, targets, amount, adjacentAmount, null, cause);
    }

    public void DoMultihitDamage(RuntimeCharacter cause, TargetCharacters targets, Ability ability, float amount, float adjacentAmount, AbilityType[] abilityTypes)
    {
        foreach (MultiHit hit in ability.MultiHits)
        {
            TargetCharacters hitTargets = new TargetCharacters(cause, targets.Main, hit.Target);
            foreach (RuntimeCharacter character in hitTargets)
            {
                float finalAmount = hitTargets.Amount(character, amount, adjacentAmount);
                DoDamage(cause, character, finalAmount * hit.PerOfDMG / 100, true, abilityTypes);
                float weaknessBreak = hit.PerOfToughnessDMG * hitTargets.Amount(character, ability.WeaknessBreak, ability.WeaknessBreakForNonMainTargets);
                WeaknessBreak(character, weaknessBreak, cause);
            }
        }
    }

    public void DoDamage(RuntimeCharacter cause, RuntimeCharacter receiver, float baseDMG, bool canCrit, AbilityType[] abilityTypes)
    {
        receiver.Invoke(Triggers.AfterTakingDamage, cause);
        Element element = cause.BasedOfCharacter.element;
        float crit = canCrit ? Crit(cause.Adv.CRIT_Rate / 100, cause.Adv.CRIT_DMG / 100) : 1;
        float dmg = (abilityTypes.Length == 0) ? 1 : DMGMultiplier(cause.Adv.ElementDMG.Get(element) + cause.Adv.AbilityDMG.Get(abilityTypes));
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

    public void WeaknessBreak(TargetCharacters targets, float amount, float adjacentAmount)
    {
        foreach (var target in targets) WeaknessBreak(target, targets.Amount(target, amount, adjacentAmount), targets.Cause);
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

        public TargetCharacters(RuntimeCharacter cause, RuntimeCharacter target, Targets targetType, ValidTeam validTeam = ValidTeam.All) : base()
        {
            if (target == null) return;
            if (validTeam == ValidTeam.Enemy && target.Team.Id != cause.Team.EnemyTeam.Id) return;
            if (validTeam == ValidTeam.Ally && target.Team.Id != cause.Team.Id) return;
            if (validTeam == ValidTeam.None) return;

            switch (targetType)
            {
                case Targets.Single:
                    Add(target);
                    break;
                case Targets.Blast:
                    Add(AreaTargeting(target));
                    break;
                case Targets.AOE:
                    Main = target;
                    foreach (RuntimeCharacter c in target.Team) Add(c);
                    break;
                case Targets.Self:
                    Add(cause);
                    break;
                case Targets.Others:
                    foreach (RuntimeCharacter c in target.Team) Add(c);
                    if (target.Team.Contains(cause)) Remove(cause);
                    break;
                case Targets.Random:
                    int id = UnityEngine.Random.Range(0, target.Team.Count);
                    Add(target.Team[id]);
                    break;
                case Targets.RandomAdjacent:
                    List<RuntimeCharacter> characters = AreaTargeting(target, true);
                    Add(characters[UnityEngine.Random.Range(0, characters.Count)]);
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

        public void Add(List<RuntimeCharacter> characters)
        {
            foreach (RuntimeCharacter character in characters)
            {
                Add(character);
            }
        }

        private List<RuntimeCharacter> AreaTargeting(RuntimeCharacter target, bool excludeMain = false)
        {
            List<RuntimeCharacter> characters = new List<RuntimeCharacter>();
            if (!excludeMain) characters.Add(target);
            if (target.Position > 0) characters.Add(target.Team[target.Position - 1]);
            if (target.Position < target.Team.Count - 1) characters.Add(target.Team[target.Position + 1]);
            return characters;
        }

        public float Amount(RuntimeCharacter target, float main, float other)
        {
            if (other == 0) other = main;
            return (target == Main) ? main : other;
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
                if (correctCause) Effect(new TargetCharacters(_source, receiver, _mechanic.target), _mechanic.effect);
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

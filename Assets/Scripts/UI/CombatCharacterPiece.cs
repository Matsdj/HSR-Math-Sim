using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Combat;

public class CombatCharacterPiece : CharacterGridPiece, IPointerClickHandler
{
    public RectTransform HealthBar;
    public RectTransform ToughnessBar;
    public Button UltButton;
    public Button UltButton2;
    private RuntimeCharacter _runtimeCharacter;
    private UnityEvent<RuntimeCharacter, CombatCharacterPiece> _runtimeCharacterEvent;
    private Combat _combat;

    protected override void Setup(Character c)
    {
        base.Setup(c);
        _runtimeCharacterEvent = new UnityEvent<RuntimeCharacter, CombatCharacterPiece>();
        UltButton.gameObject.SetActive(false);
        UltButton2.gameObject.SetActive(false);
    }

    public void Apply(Character c, RuntimeCharacter RuntimeCharacter, Combat combat)
    {
        Setup(c);
        _runtimeCharacter = RuntimeCharacter;
        _runtimeCharacter.Team.EnemyTeam.Events[Triggers.AfterDealingDamage].Add(UpdateHealthBar);
        _runtimeCharacter.Team.EnemyTeam.Events[Triggers.AfterDealingDamage].Add(UpdateToughnessBar);
        _runtimeCharacter.Team.Events[Triggers.ChargingEnergy].Add(ShowUltButton);
        _runtimeCharacter.Events[Triggers.OnDeath].Add(DestroySelf);
        _combat = combat;

    }

    public void AddOnCharacterClickListener(UnityAction<RuntimeCharacter, CombatCharacterPiece> onCharacterClick)
    {
        _runtimeCharacterEvent.AddListener(onCharacterClick);
    }

    private void UpdateHealthBar(TargetCharacters targets)
    {
        if (targets.Contains(_runtimeCharacter))
        {
            float x = _runtimeCharacter.CurrentHP / _runtimeCharacter.Final.HP;
            HealthBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
        }
    }

    private void UpdateToughnessBar(TargetCharacters targets)
    {
        if (targets.Contains(_runtimeCharacter))
        {
            float x = _runtimeCharacter.Adv.Toughness / _runtimeCharacter.AdvancedStats.Toughness;
            ToughnessBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
        }
    }

    private void ShowUltButton(TargetCharacters targets)
    {
        if (targets.Contains(_runtimeCharacter))
        {
            bool active = _runtimeCharacter.Energy >= _runtimeCharacter.Adv.MaxEnergy;
            //Debug.Log($"Energy:{_runtimeCharacter.Energy}, Max:{_runtimeCharacter.Adv.MaxEnergy}, ShowUltButton:{active}");
            UltButton.gameObject.SetActive(active);
            if (_runtimeCharacter.BasedOfCharacter.Ultimate is DualUlt) UltButton2.gameObject.SetActive(active);
        }
    }

    public override void OnClick()
    {
        _runtimeCharacterEvent.Invoke(_runtimeCharacter, this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            _combat.Info.ChangeInfoSource(_runtimeCharacter.CharacterInfo);
    }

    public void OnClickUlt()
    {
        _combat.Ult(_runtimeCharacter);
    }

    public void OnClickUlt2()
    {
        _combat.Ult2(_runtimeCharacter);
    }

    public void DestroySelf(TargetCharacters targets = null)
    {
        Destroy(this);
    }
}

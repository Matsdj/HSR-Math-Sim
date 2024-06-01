using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatCharacterPiece : CharacterGridPiece, IPointerClickHandler
{
    public RectTransform HealthBar;
    public RectTransform ToughnessBar;
    public Button UltButton;
    public Button UltButton2;
    private RuntimeCharacter _RuntimeCharacter;
    private UnityEvent<RuntimeCharacter, CombatCharacterPiece> _RuntimeCharacterEvent;
    private Combat _combat;

    protected override void Setup(Character c)
    {
        base.Setup(c);
        _RuntimeCharacterEvent = new UnityEvent<RuntimeCharacter, CombatCharacterPiece>();
        UltButton.gameObject.SetActive(false);
        UltButton2.gameObject.SetActive(false);
    }

    public void Apply(Character c, RuntimeCharacter RuntimeCharacter, Combat combat)
    {
        Setup(c);
        _RuntimeCharacter = RuntimeCharacter;
        _RuntimeCharacter.Events[Triggers.AfterTakingDamage].Add(UpdateHealthBar);
        _RuntimeCharacter.Events[Triggers.AfterTakingDamage].Add(UpdateToughnessBar);
        _RuntimeCharacter.Events[Triggers.EnergyChange].Add(ShowUltButton);
        _RuntimeCharacter.Events[Triggers.OnDeath].Add(DestroySelf);
        _combat = combat;

    }

    public void AddOnCharacterClickListener(UnityAction<RuntimeCharacter, CombatCharacterPiece> onCharacterClick)
    {
        _RuntimeCharacterEvent.AddListener(onCharacterClick);
    }

    private void UpdateHealthBar(RuntimeCharacter reciever, RuntimeCharacter cause)
    {
        float x = reciever.CurrentHP / reciever.Final.HP;
        HealthBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
    }

    private void UpdateToughnessBar(RuntimeCharacter reciever, RuntimeCharacter cause)
    {
        float x = reciever.Adv.Toughness / reciever.AdvancedStats.Toughness;
        ToughnessBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
    }

    private void ShowUltButton(RuntimeCharacter reciever, RuntimeCharacter cause)
    {
        bool active = reciever.Energy >= reciever.Adv.MaxEnergy;
        Debug.Log($"Energy:{reciever.Energy}, Max:{reciever.Adv.MaxEnergy}, ShowUltButton:{active}");
        UltButton.gameObject.SetActive(active);
        if (_RuntimeCharacter.BasedOfCharacter.Ultimate is DualUlt) UltButton2.gameObject.SetActive(active);
    }

    public override void OnClick()
    {
        _RuntimeCharacterEvent.Invoke(_RuntimeCharacter, this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            _combat.Info.ChangeInfoSource(_RuntimeCharacter.CharacterInfo);
    }

    public void OnClickUlt()
    {
        _combat.Ult(_RuntimeCharacter);
    }

    public void OnClickUlt2()
    {
        _combat.Ult2(_RuntimeCharacter);
    }

    public void DestroySelf(RuntimeCharacter receiver, RuntimeCharacter cause)
    {
        Destroy(this);
    }
}

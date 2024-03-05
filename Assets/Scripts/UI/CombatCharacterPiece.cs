using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Combat;

public class CombatCharacterPiece : CharacterGridPiece
{
    public RectTransform HealthBar;
    public Button UltButton;
    public Button UltButton2;
    private ActionCharacter _actionCharacter;
    private UnityEvent<ActionCharacter, CombatCharacterPiece> _actionCharacterEvent;
    private Combat _combat;

    protected override void Setup(Character c)
    {
        base.Setup(c);
        _actionCharacterEvent = new UnityEvent<ActionCharacter, CombatCharacterPiece>();
        UltButton.gameObject.SetActive(false);
        UltButton2.gameObject.SetActive(false);
    }

    public void Apply(Character c, ActionCharacter actionCharacter, Combat combat)
    {
        Setup(c);
        _actionCharacter = actionCharacter;
        _actionCharacter.Events[Types.Triggers.AfterTakingDamage].Add(UpdateHealthBar);
        _actionCharacter.Events[Types.Triggers.EnergyChange].Add(ShowUltButton);
        _combat = combat;

    }

    public void AddOnCharacterClickListener(UnityAction<ActionCharacter, CombatCharacterPiece> onCharacterClick)
    {
        _actionCharacterEvent.AddListener(onCharacterClick);
    }

    private void UpdateHealthBar(RuntimeCharacter character)
    {
        float x = character.CurrentHP / character.Final.HP;
        Debug.Log($"HP:{character.CurrentHP}, MaxHP:{character.Final.HP}, {x}");
        HealthBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
    }

    private void ShowUltButton(RuntimeCharacter character)
    {
        bool active = character.Energy >= character.Adv.MaxEnergy;
        Debug.Log($"Energy:{character.Energy}, Max:{character.Adv.MaxEnergy}");
        UltButton.gameObject.SetActive(active);
        if (_actionCharacter.BasedOfCharacter.Ultimate is DualUlt) UltButton2.gameObject.SetActive(active);
    }

    public override void OnClick()
    {
        _actionCharacterEvent.Invoke(_actionCharacter, this);
    }

    public void OnClickUlt()
    {
        _combat.Ult(_actionCharacter);
    }

    public void OnClickUlt2()
    {
        _combat.Ult2(_actionCharacter);
    }
}

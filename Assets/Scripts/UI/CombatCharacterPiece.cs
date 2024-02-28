using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Combat;

public class CombatCharacterPiece : CharacterGridPiece
{
    public RectTransform HealthBar;
    public RectTransform UltButton;
    private ActionCharacter _actionCharacter;
    private UnityEvent<ActionCharacter, CombatCharacterPiece> _actionCharacterEvent;

    protected override void Apply(Character c)
    {
        base.Apply(c);
        _actionCharacterEvent = new UnityEvent<ActionCharacter, CombatCharacterPiece>();
    }

    public void Apply(Character c, ActionCharacter actionCharacter, UnityAction<ActionCharacter, CombatCharacterPiece> function)
    {
        Apply(c);
        _actionCharacter = actionCharacter;
        _actionCharacterEvent.AddListener(function);
        _actionCharacter.Character.Events[Types.Triggers.AfterTakingDamage].Add(UpdateHealthBar);

    }

    public void UpdateHealthBar(RuntimeCharacter character)
    {
        float x = character.Stats.CurrentHP / character.Stats.Final.HP;
        Debug.Log($"HP:{character.Stats.CurrentHP}, MaxHP:{character.Stats.Final.HP}, {x}");
        HealthBar.localScale = new Vector3(Mathf.Clamp(x, 0, 1), 1, 1);
    }

    protected override void OnClick()
    {
        _actionCharacterEvent.Invoke(_actionCharacter, this);
        image.sprite = _actionCharacter.Character.BasedOfCharacter.sprite;
    }
}

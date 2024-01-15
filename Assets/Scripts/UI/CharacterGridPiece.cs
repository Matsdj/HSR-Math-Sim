using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterGridPiece : MonoBehaviour
{
    public Image image;
    public TMP_Text text;
    public Button button;
    private Character _character;
    private AddCharacter _addCharacter;

    public void Apply(Character c, AddCharacter addCharacter)
    {
        image.sprite = c.sprite;
        text.text = c.name;
        _character = c;
        button.onClick.AddListener(OnClick);
        _addCharacter = addCharacter;
    }

    private void OnClick()
    {
        _addCharacter.gameObject.SetActive(true);
        _addCharacter.UseCharacterPreset(_character);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddCharacter : MonoBehaviour
{
    public Image icon;
    
    public void UseCharacterPreset(Character character)
    {
        icon.sprite = character.sprite;
    }
}

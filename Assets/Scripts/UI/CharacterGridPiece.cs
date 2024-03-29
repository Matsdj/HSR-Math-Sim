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
    //Add characterUI
    private UnityEvent<Character> _addCharacterEvent;


    protected virtual void Setup(Character c)
    {
        image.sprite = c.sprite;
        text.text = c.name;
        _character = c;
        _addCharacterEvent = new UnityEvent<Character>();
    }

    //This function is used in Character adding UI
    public void Apply(Character c, UnityAction<Character> function)
    {
        Setup(c);
        _addCharacterEvent.AddListener(function);
    }

    public virtual void OnClick()
    {
        _addCharacterEvent.Invoke(_character);
    }
}

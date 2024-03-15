using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionHistoryPiece : CharacterGridPiece, IPointerClickHandler
{
    private int TurnId;
    private float ActionValue;
    public string Body;

    public ActionHistoryPiece Setup(RuntimeCharacter character, int turnId)
    {
        Setup(character.BasedOfCharacter);
        text.text = turnId.ToString();
        TurnId = turnId;
        ActionValue = character.ActionValue;
        return this;
    }

    public override void OnClick()
    {
        Combat.instance.Info.ChangeInfoSource(ActionHistoryInfo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) OnClick();
    }

    public string[] ActionHistoryInfo()
    {
        string header = $"Turn {TurnId} at {ActionValue} ActionValue";
        return new string[] { header, Body };
    }
}

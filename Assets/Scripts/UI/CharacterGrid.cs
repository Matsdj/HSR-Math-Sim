using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterGrid : MonoBehaviour
{
    public CharacterCollection collection;
    public CharacterGridPiece prefab;

    public AddCharacter AddCharacter;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
        foreach (Character c in collection.list)
        {
            Instantiate(prefab, transform).Apply(c, AddCharacter);
        }
    }
}

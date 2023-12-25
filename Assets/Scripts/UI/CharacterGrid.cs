using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGrid : MonoBehaviour
{
    public CharacterCollection collection;
    public GameObject prefab;

    void Start()
    {
        foreach (Character c in collection.list)
        {
            Instantiate(prefab);
        }
    }
}

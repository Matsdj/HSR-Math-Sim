using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveJson : MonoBehaviour
{
    public Character characterToSave;

    private string _saveLocation { get { return Application.persistentDataPath + $"/{characterToSave.name}.json"; } }
    public void Save()
    {
        string json = JsonUtility.ToJson(characterToSave);
        Debug.Log($"FilePath:{Application.persistentDataPath}");
        File.WriteAllText(_saveLocation, json);
    }

    public void Load()
    {
        string json = File.ReadAllText(_saveLocation);
        Character loadedCharacter = JsonUtility.FromJson<Character>(json);
        Debug.Log(loadedCharacter.name);
    }
}

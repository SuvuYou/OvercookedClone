using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyValuePairType
{
    public Vector2Int Key;
    public EditableItem Value;
}

[System.Serializable]
public class SerializableDictionary
{
    [SerializeField]
    private List<KeyValuePairType> keyValuePairs = new ();

    public Dictionary<Vector2Int, EditableItem> ToDictionary()
    {
        var dictionary = new Dictionary<Vector2Int, EditableItem>();
        foreach (var pair in keyValuePairs)
        {
            dictionary[pair.Key] = pair.Value;
        }
        
        return dictionary;
    }

    public void FromDictionary(Dictionary<Vector2Int, EditableItem> dictionary)
    {
        keyValuePairs.Clear();
        foreach (var kvp in dictionary)
        {
            keyValuePairs.Add(new KeyValuePairType { Key = kvp.Key, Value = kvp.Value });
        }
    }
}
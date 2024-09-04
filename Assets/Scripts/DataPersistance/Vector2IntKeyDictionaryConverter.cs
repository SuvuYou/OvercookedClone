using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Vector2IntKeyDictionaryConverter<TValue> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<Vector2Int, TValue>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (Dictionary<Vector2Int, TValue>)value;
        var jsonObject = new JObject();

        foreach (var kvp in dictionary)
        {
            var key = $"({kvp.Key.x}, {kvp.Key.y})";
            var val = JToken.FromObject(kvp.Value, serializer);
            jsonObject.Add(key, val);
        }

        jsonObject.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var dictionary = new Dictionary<Vector2Int, TValue>();

        foreach (var kvp in jsonObject)
        {
            var keyString = kvp.Key.Trim('(', ')');
            var parts = keyString.Split(',');
            var key = new Vector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
            var value = kvp.Value.ToObject<TValue>(serializer);
            dictionary.Add(key, value);
        }

        return dictionary;
    }
}
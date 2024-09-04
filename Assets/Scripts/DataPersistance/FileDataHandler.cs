using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class FileDataHandler
{
    private string _fileDirectoryPath;
    private string _fileName;

    public FileDataHandler(string fileDirectoryPath, string fileName)
    {
        _fileDirectoryPath = fileDirectoryPath;
        _fileName = fileName;
    }

    public void Save(GameData data)
    {
        var fullpath = Path.Combine(_fileDirectoryPath, _fileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector2IntKeyDictionaryConverter<int>());

            string dataJson = JsonConvert.SerializeObject(data, settings);

            using (FileStream stream = new FileStream(fullpath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataJson);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't save the save file: " + fullpath + " " + e);
        }
    }

    public GameData Load()
    {
        GameData data = null;
        var fullpath = Path.Combine(_fileDirectoryPath, _fileName);

        if (File.Exists(fullpath))
        {
            try
            {
                string dataJson = "";

                using (FileStream stream = new FileStream(fullpath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataJson = reader.ReadToEnd();
                    }
                }

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new Vector2IntKeyDictionaryConverter<int>());

                data = JsonConvert.DeserializeObject<GameData>(dataJson, settings);
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't load the save file: " + fullpath + " " + e);
            }
        }

        return data;
    }

}

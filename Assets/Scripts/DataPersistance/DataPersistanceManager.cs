using System;
using System.Collections.Generic;
using UnityEngine;

public class DataPersistanceManager : MonoBehaviour
{
    public static DataPersistanceManager Instance;

    [SerializeField] private string _saveFileName;
    [SerializeField] private SerializableDictionary DEFAULT_MAP_ITEMS;
    [SerializeField] private AvailablePurchasableItemsSO _availablePurchasableItemsSO;

    private FileDataHandler _dataHandler;

    public event Action<GameData> OnLoadGameData;
    public event Action<GameData, Action<GameData>> OnSaveGameData;

    public GameData Data { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);

            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        _dataHandler = new FileDataHandler(fileDirectoryPath: Application.persistentDataPath, fileName: _saveFileName);
        NewGame();
    }

    public void NewGame()
    {
        var mapItems = new Dictionary<Vector2Int, int>();

        foreach (var pair in DEFAULT_MAP_ITEMS.ToDictionary()) mapItems.Add(pair.Key, _availablePurchasableItemsSO.GetIndexByEditableItem(pair.Value));
        
        Data = new GameData(mapItems);
    }

    public void SaveData()
    {
        OnSaveGameData?.Invoke(Data, _modifyGameData);
        _dataHandler.Save(Data);
    }

    public void LoadData()
    {
        Data = _dataHandler.Load();

        if (Data == null) NewGame();

        OnLoadGameData?.Invoke(Data);
    } 

    public void DeleteSaveFile()
    {
        _dataHandler.Delete();
    }

    private void _modifyGameData(GameData newData) => Data = newData;
}

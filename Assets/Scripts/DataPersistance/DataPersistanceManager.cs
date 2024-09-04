using System;
using System.Linq;
using UnityEngine;

public class DataPersistanceManager : MonoBehaviour
{
    public static DataPersistanceManager Instance;

    [SerializeField]
    private string _saveFileName;

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
        Data = new GameData();
    }

    public void SaveData()
    {
        OnSaveGameData?.Invoke(Data, _modifyGameData);
        _dataHandler.Save(Data);
    }

    public void LoadData()
    {
        Data = _dataHandler.Load();
        OnLoadGameData?.Invoke(Data);

        if (Data == null) NewGame();
    } 

    private void _modifyGameData(GameData newData) => Data = newData;
}

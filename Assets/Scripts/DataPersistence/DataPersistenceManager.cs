using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{

    [Header("File Storage Config")]

    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private GameData gameData;

    private List<IDataPersistence> dataPersistenceObjects;

    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroying newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("On scene loaded yes");
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private void Start()
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        //this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        //SaveGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();

        if(this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }

        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }

        //Debug.Log("Health = " + gameData.currentHealth);
        //Debug.Log("Respawn Point = " + gameData.respawnPoint);
    }

    public void SaveGame()
    {
        //Debug.Log("Saving");
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        //Debug.Log("Health = " + gameData.currentHealth);
        //Debug.Log("Respawn Point = " + gameData.respawnPoint);

        dataHandler.Save(gameData);
    }

    // Test the save game function
    private void OnApplicationQuit()
    {
        //SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

}

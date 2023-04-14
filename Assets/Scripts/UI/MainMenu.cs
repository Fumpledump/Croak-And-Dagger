using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class MainMenu : MonoBehaviour
{
    private MenuMap menuMap;
    private InputAction escape;

    [SerializeField] private GameObject settings;

    void Awake()
    {
        PlayerPrefs.SetFloat("Music", AudioListener.volume);
        menuMap = new MenuMap();
        Cursor.visible = true;
        settings.SetActive(false);
    }

    private void OnEnable()
    {
        escape = menuMap.Menu.Escape;
        escape.Enable();
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        escape.Disable();
    }

    public void ActivateMenu()
    {
        settings.SetActive(true);
    }

    public void DeactivateMenu()
    {
        settings.SetActive(false);
    }

    public void SettingToggle()
    {
        if (!settings.activeInHierarchy)
        {
            settings.SetActive(true);
        }
        else
        {
            settings.SetActive(false);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (sceneName == "Exit") {
            UnityEngine.Application.Quit();
            UnityEngine.Debug.Log(sceneName);
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame(string sceneName)
    {
        DataPersistenceManager.instance.NewGame();
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    
}
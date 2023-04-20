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

    void Awake()
    {
        AudioManager.instance.StopSounds();
        PlayerPrefs.SetFloat("Music", AudioListener.volume);
        menuMap = new MenuMap();
        Cursor.visible = true;
        AudioManager.instance.Play("Menu");
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

    public void LoadScene(string sceneName)
    {
        AudioManager.instance.StopSounds();
        if (sceneName == "Exit") {
            UnityEngine.Application.Quit();
            UnityEngine.Debug.Log(sceneName);
            return;
        }
        AudioManager.instance.Play(AudioManager.instance.ActiveSong());
        SceneManager.LoadScene(sceneName); 
    }

    public void NewGame(string sceneName)
    {
        AudioManager.instance.StopSounds();
        AudioManager.instance.Play(sceneName);
        DataPersistenceManager.instance.NewGame();
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    
}
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
using System;

public class SettingsMenu : MonoBehaviour
{
    private MenuMap menuMap;
    private InputAction escape;

    [SerializeField] private Slider musicSlider = null;

    void Awake()
    {
        PlayerPrefs.SetFloat("Music", 0.5f);
        menuMap = new MenuMap();
        Cursor.visible = true;
    }

    void Start()
    {
        LoadVolume();
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
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame(string sceneName)
    {
        DataPersistenceManager.instance.NewGame();
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    public void SaveVolume()
    {
        float musicValue = musicSlider.value;
        PlayerPrefs.SetFloat("Music", musicValue);
        LoadVolume();
    }

    public void LoadVolume()
    {
        float musicValue = PlayerPrefs.GetFloat("Music");
        musicSlider.value = musicValue;
        AudioListener.volume = musicValue;
    }


}
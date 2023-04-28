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
    [SerializeField] private GameObject settingsUI;

    [SerializeField] private GameObject healthStatic;
    [SerializeField] private GameObject health;
    //[SerializeField] private GameObject pauseUI;

    void Awake()
    {
        menuMap = new MenuMap();
        Cursor.visible = true;
    }

    void Start()
    {
        LoadVolume();
    }

    void Update()
    {
        AudioListener.volume = musicSlider.value;
        //PlayerPrefs.SetFloat("Music", musicSlider.value);
    }

    private void OnEnable()
    {
        escape = menuMap.Menu.Escape;
        escape.Enable();
    }

    private void OnDisable()
    {
        escape.Disable();
    }



    public void LoadScene(string sceneName)
    {
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

    public void ToggleActive()
    {
        if (!settingsUI.activeInHierarchy)
        {
            settingsUI.SetActive(true);
            Time.timeScale = 0;
            //AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            healthStatic.SetActive(false);
            health.SetActive(false);
        } 
        else
        {
            settingsUI.SetActive(false);
            healthStatic.SetActive(true);
            health.SetActive(true);
        }
    }


}
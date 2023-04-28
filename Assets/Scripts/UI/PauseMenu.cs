using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PauseMenu : MonoBehaviour
{
    private MenuMap menuMap;
    private InputAction escape;
    private float currentChoice;

    [SerializeField] private GameObject resume;
    [SerializeField] private GameObject options;
    [SerializeField] private GameObject exit;

    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject settingsUI;

    [SerializeField] private GameObject healthStatic;
    [SerializeField] private GameObject health;

    [SerializeField] private bool isPaused;


    // Start is called before the first frame update
    void Awake()
    {
        menuMap = new MenuMap();
        resume.GetComponent<Image>().color = Color.white;
        exit.GetComponent<Image>().color = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        escape = menuMap.Menu.Escape;

        escape.Enable();
        escape.performed += Pause;

    }

    private void OnDisable()
    {
        escape.Disable();
    }

    public void Pause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        ActivateMenu();
    }

    public void ActivateMenu()
    {
        if (!settingsUI.activeInHierarchy)
        {
            Time.timeScale = 0;
            //AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pauseUI.SetActive(true);
            healthStatic.SetActive(false);
            health.SetActive(false);
        } 
    }

    public void DeactivateMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        pauseUI.SetActive(false);
        Cursor.visible = false;
        isPaused = false;
        healthStatic.SetActive(true);
        health.SetActive(true);
    }

    public void GoToTitle()
    {
        DeactivateMenu();
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToOptions()
    {
        DeactivateMenu();
        SceneManager.LoadScene("Settings");
    }


}


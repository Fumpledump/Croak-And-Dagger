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
    private InputAction toggleUp;
    private InputAction toggleDown;
    private InputAction select;
    private float currentChoice;

    [SerializeField] private GameObject resume;
    [SerializeField] private GameObject exit;

    [SerializeField] private GameObject pauseUI;
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
        toggleUp = menuMap.Menu.ToggleUp;
        toggleDown = menuMap.Menu.ToggleDown;
        select = menuMap.Menu.Select;

        escape.Enable();
        toggleUp.Enable();
        toggleDown.Enable();
        select.Enable();

        currentChoice = 0;
        resume.GetComponent<Image>().color = Color.white;
        exit.GetComponent<Image>().color = Color.grey;


        escape.performed += Pause;
        toggleUp.performed += TogUp;
        toggleDown.performed += TogDown;
        select.performed += Select;

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

    void ActivateMenu()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        pauseUI.SetActive(true);
    }

    public void DeactivateMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        pauseUI.SetActive(false);
        isPaused = false;
    }

    void TogUp(InputAction.CallbackContext context)
    {
        currentChoice = 0;
        resume.GetComponent<Image>().color = Color.white;
        exit.GetComponent<Image>().color = Color.grey;
        UnityEngine.Debug.Log(currentChoice);
    }

    void TogDown(InputAction.CallbackContext context)
    {
        currentChoice = 1;
        resume.GetComponent<Image>().color = Color.grey;
        exit.GetComponent<Image>().color = Color.white;
        UnityEngine.Debug.Log(currentChoice);
    }

    void Select(InputAction.CallbackContext context)
    {
        if (currentChoice == 0)
        {
            DeactivateMenu();
        }
        else if (currentChoice == 1)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}


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
    [SerializeField] private GameObject exit;
    [SerializeField] private GameObject options;
    [SerializeField] private Button rB;
    [SerializeField] private Button oB;
    [SerializeField] private Button eB;


    [SerializeField] private GameObject pauseUI;
    [SerializeField] private bool isPaused;


    // Start is called before the first frame update
    void Awake()
    {
        menuMap = new MenuMap();
        rB = resume.GetComponent<Button>();
        oB = options.GetComponent<Button>();
        eB = exit.GetComponent<Button>();
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

    void ActivateMenu()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseUI.SetActive(true);
    }

    public void DeactivateMenu()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        pauseUI.SetActive(false);
        Cursor.visible = false;
        isPaused = false;
        rB.interactable = true;
        oB.interactable = true;
        eB.interactable = true;
    }

    public void GoToTitle()
    {
        DeactivateMenu();
        SceneManager.LoadScene("MainMenu");
    }

    
}


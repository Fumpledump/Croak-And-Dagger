using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMenu : MonoBehaviour
{
    [Header("Script Set Up")]
    [SerializeField] private GameObject CheatMenuUI;
    [SerializeField] private StarterAssetsInputs input; // Inputs

    // Start is called before the first frame update
    void Start()
    {
        CheatMenuUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (input.cheat)
        {
            Cheat();
        }
        input.cheat = false;
    }

    public void Cheat()
    {
        Debug.Log("Yeet");
        Time.timeScale = 0f;
        CheatMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
    public void Resume()
    {
        Debug.Log("Wheat");
        Time.timeScale = 1f;
        CheatMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
}

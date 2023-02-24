using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;
using StarterAssets;
using TMPro;
using UnityEngine.SceneManagement;

// Script that handels Narrative Sequences & Triggers for the Player and Holds Narrative Data.
public class NarrativeHandler : MonoBehaviour, IDataPersistence
{
    [Header("Command Set Up")]
    public List<GameObject> dialogueCameras = new List<GameObject>();

    [Header("Trigger Information")]
    public bool inTrigger; // Can the player start a dialog sequence?
    public bool inDialog; // Is the player in a dialog sequence?
    public NarrativeTrigger currentTrigger; // The Current Trigger the Player is in

    [Header("Narrative Data")]
    public int relationshipValue; // Player's Relationship Value with their Child
    public string croakName; // Name of Player's Child

    [Header("Script Set Up")]
    [SerializeField] private DialogueRunner dialogSystem; // Yarnspinner
    [SerializeField] private GameObject dialogPrompt; // Indicator that lets the player know they can start a dialog sequence.
    [SerializeField] private GameObject hud; // Hud
    [SerializeField] private StarterAssetsInputs input; // Inputs

    //TODO: Probably should move narrative input handeling to another script as I don't see this being used for much
    [Header("Narrative Input")]
    public GameObject NameInput; // Inputted Name by the Player
    public GameObject NameWarning; // Warning for Invalid Name

    private GameObject player; // Player GameObject
    private static NarrativeHandler instance; // Singleton for the Narrative Handler

    private Coroutine dialogeRoutine;

    public static NarrativeHandler Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Make NarrativeHandler a Singleton
        if (instance != null)
        {
            Debug.LogError("Found more than one NarrativeHandler in the scene!");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        player = this.transform.gameObject; // Grab Player GameObject
        NarrativeIntegrator.CheckIntegrator();
    }

    // Update is called once per frame
    private void Update()
    {
        // Starts a Dialog Sequence if the player is in a trigger for one
        if (inTrigger && !inDialog)
        {
            if (!currentTrigger.automatic)
            {
                dialogPrompt.gameObject.SetActive(inTrigger && !inDialog); // If the player can start a dialog sequence and is not already in one show the prompt
            }

            if (input.interact || currentTrigger.automatic)
            {
                ActivateControls(false);


                dialogeRoutine = StartCoroutine(DialogueStart());
            }
            else
            {
                input.interact = false;
            }
        }
        else
        {
            dialogPrompt.gameObject.SetActive(false); // If the player can start a dialog sequence and is not already in one show the prompt
        }
    }

    IEnumerator DialogueStart(float delay = 0.25f)
    {
        if (player.GetComponent<ThirdPersonController>().Grounded)
        {
            yield return new WaitForSeconds(delay);
        }
        else
        {
            yield return new WaitForSeconds(delay * 4);
        }

        
        dialogSystem.StartDialogue(currentTrigger.node);
        StopCoroutine(dialogeRoutine);
    }

    // Enables and Disables the Controls
    public void ActivateControls(bool activate)
    {
        Debug.Log($"Controls set to: {activate}");

        inDialog = !activate;

        // Disable Controls
        player.GetComponent<ThirdPersonController>().inDialog = !activate;
        player.GetComponent<FrogCharacter>().inDialog = !activate;

        // Disable UI
        hud.gameObject.SetActive(activate);

        // Cursor Settings
        if (activate)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Runs whenever a dialogue sequence has ended.
    public void DialogComplete()
    {
        ActivateControls(true);
        inTrigger = false;
        inDialog = false;
        input.interact = false;

        if (currentTrigger.loadLevel != string.Empty)
        {
            SceneManager.LoadScene(currentTrigger.loadLevel);
        }

        // Set Trigger to complete so it can not be reactivated
        if (currentTrigger != null)
        {
            if (!currentTrigger.repeatable)
            {
                currentTrigger.triggerComplete = true;
            }
            currentTrigger = null;
        }
    }

    // Loads Narrative Data
    public void LoadData(GameData data)
    {
        this.relationshipValue = data.relationshipValue;
        this.croakName = data.croakName;
    }

    // Saves Narrative Data
    public void SaveData(ref GameData data)
    {
        Debug.Log(data.croakName);
        data.relationshipValue = this.relationshipValue;
        data.croakName = this.croakName;
        Debug.Log(this.croakName);
    }

    // Sets the Player Child's Name
    public void SetName()
    {
        TMP_InputField inputField = NameInput.GetComponent<TMP_InputField>();

        if (inputField.text.Length > 9)
        {
            NameWarning.SetActive(true);
        }
        else
        {
            NameWarning.SetActive(false);

            this.croakName = inputField.text;
            NameInput.SetActive(false);
        }
    }
}

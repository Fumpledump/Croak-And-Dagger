using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

// Designates Attributes for Narrative Triggers so that they can be placed and edited throughout a level.
public class NarrativeTrigger : MonoBehaviour, IDataPersistence
{
    [Header("Trigger Data")]
    public string node; // Name of the associated Yarn Script that should run when this trigger is activated
    public bool automatic; // Should this trigger run as soon as the player enters it?
    public bool repeatable; // Can this trigger be ran multiple times?
    public bool triggerComplete; // Is the trigger complete?
    public string loadLevel; // If inputed with the name of a level after the dialogue is complete the level will be loaded


    private NarrativeHandler narrativeHandler;

    [SerializeField] private string id;

    [ContextMenu("Generate guid for id")]

    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private void Start()
    {
        narrativeHandler = NarrativeHandler.Instance; // Grab NarrativeHandler Singleton
    }

    // Checks if the Player is inside the Narrative Trigger
    private void OnTriggerEnter(Collider col)
    {
        if(!narrativeHandler)
            narrativeHandler = NarrativeHandler.Instance; // Grab NarrativeHandler Singleton

        if (col.tag == "Player" && !triggerComplete)
        {
            narrativeHandler.inTrigger = true;
            narrativeHandler.currentTrigger = this;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player" && !triggerComplete)
        {
            narrativeHandler.inTrigger = false;
            narrativeHandler.currentTrigger = null;
        }
    }

    public void LoadData(GameData data)
    {
        data.narrativeTriggersHit.TryGetValue(id, out triggerComplete);
    }

    public void SaveData(ref GameData data)
    {
        if (data.narrativeTriggersHit.ContainsKey(id))
        {
            data.narrativeTriggersHit.Remove(id);
        }
        data.narrativeTriggersHit.Add(id, triggerComplete);
    }
}

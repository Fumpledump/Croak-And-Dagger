using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

// Designates Attributes for Narrative Triggers so that they can be placed and edited throughout a level.
public class NarrativeTrigger : MonoBehaviour
{
    [Header("Trigger Data")]
    public string node; // Name of the associated Yarn Script that should run when this trigger is activated
    public bool automatic; // Should this trigger run as soon as the player enters it?
    public bool repeatable; // Can this trigger be ran multiple times?
    public bool triggerComplete; // Is the trigger complete?
    public string loadLevel; // If inputed with the name of a level after the dialogue is complete the level will be loaded


    private NarrativeHandler narrativeHandler;

    private void Start()
    {
        narrativeHandler = NarrativeHandler.Instance; // Grab NarrativeHandler Singleton
    }

    // Checks if the Player is inside the Narrative Trigger
    private void OnTriggerEnter(Collider col)
    {
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
}

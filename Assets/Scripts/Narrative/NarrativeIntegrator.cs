using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Main Handler for all commands and functions that interact with Yarnspinner.
public class NarrativeIntegrator
{
    private static NarrativeHandler narrativeHandler = NarrativeHandler.Instance; // Get Ref to Narrative Handler

    // Sets the Camera for Dialogue Sequences in the game.
    // Can be used to showcase different npcs and or set pieces to go along with the narrative.
    [YarnCommand("ChangeCamera")]
    private static void ChangeCamera(int cameraIndex)
    {
        CheckHandler();

        // Check if Camera Index Exists
        if (cameraIndex > narrativeHandler.dialogueCameras.Count)
        {
            Debug.LogError("No Dialogue Camera of that Index was found!");
            return;
        }

        // Check if Camera List has Items
        if (narrativeHandler.dialogueCameras.Count <= 0)
        {
            Debug.LogError("No Cameras in Dialogue Cameras List!");
            return;
        }

        // Disable all cameras
        foreach (GameObject cam in narrativeHandler.dialogueCameras)
        {
            cam.SetActive(false);
        }

        // Enable Dialogue Camera
        narrativeHandler.dialogueCameras[cameraIndex].SetActive(true);
    }

    // Sets the Relationship Value to the given value
    [YarnCommand("SetRelationshipValue")]
    private static void SetRelationshipValue(int value)
    {
        narrativeHandler.relationshipValue = value;
    }

    // Adds to the current Relationship Value
    [YarnCommand("AddRelationshipValue")]
    private static void AddRelationshipValue(int value)
    {
        narrativeHandler.relationshipValue += value;
    }

    // Gets the current Relationship Value so it can be displayed in Dialogue
    [YarnFunction("GetRelationshipValue")]
    private static int GetRelationshipValue()
    {
        return narrativeHandler.relationshipValue;
    }

    // Sets Name for Player's Child
    [YarnCommand("InputName")]
    private static void InputName()
    {
        narrativeHandler.NameInput.SetActive(true);
    }

    // Gets current name for Player's Child so it can be displayed in Dialogue
    [YarnFunction("GetName")]
    private static string GetName()
    {
        return narrativeHandler.croakName;
    }

    public static void CheckHandler()
    {
        if (narrativeHandler == null)
        {
            Debug.Log("Narrative Handler is Null. Grabbing Narrative Handler");
            narrativeHandler = NarrativeHandler.Instance;
        }
    }
}

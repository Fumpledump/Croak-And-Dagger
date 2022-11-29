using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GameData
{
    public string croakName;
    public int relationshipValue;
    public int currentHealth;
    public Vector3 respawnPoint;
    public SerializableDictionary<string, bool> checkpointsHit;
    public SerializableDictionary<string, bool> fireflysHit;
    public SerializableDictionary<string, bool> enemies;

    public GameData()
    {
        this.croakName = "Croak";
        this.relationshipValue = 0;
        this.currentHealth = 100;
        respawnPoint = new Vector3(17, 2, -22);
        checkpointsHit = new SerializableDictionary<string, bool>();
        fireflysHit = new SerializableDictionary<string, bool>();
        enemies = new SerializableDictionary<string, bool>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;

    [ContextMenu("Generate guid for id")]

    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private bool checkpointHit;
    // Start is called before the first frame update
    void Start()
    {
        checkpointHit = false;
    }

    public void LoadData(GameData data)
    {
        data.checkpointsHit.TryGetValue(id, out checkpointHit);
    }

    public void SaveData(ref GameData data)
    {
        if (data.checkpointsHit.ContainsKey(id))
        {
            data.checkpointsHit.Remove(id);
        }
        data.checkpointsHit.Add(id, checkpointHit);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !checkpointHit)
        {
            //Debug.Log("Checkpoint");
            checkpointHit = true;
            other.GetComponent<FrogCharacter>().respawnPoint = transform.position;
            DataPersistenceManager.instance.SaveGame();
        }
    }
}

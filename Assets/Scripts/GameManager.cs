using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    // Basic Singleton
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

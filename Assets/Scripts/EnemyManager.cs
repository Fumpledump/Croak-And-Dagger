using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    // For pooling
    public List<Enemy> enemies;

    public List<GameObject> enemyGroups = new List<GameObject>();

    // Basic Singleton
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }


    public void EnemyGroupDefeated(GameObject group)
    {
        bool status = true;

        foreach (GameObject child in group.transform)
        {
            Enemy enemy = child.GetComponent<Enemy>();

            if (!enemy.isDead)
            {
                status = false;
            }
        }

        if (status == true)
        {
            Debug.Log("All Enemies in the Group are Defeated!");
        }
    }
}

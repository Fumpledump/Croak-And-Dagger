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
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }


    public void EnemyGroupDefeated(GameObject group)
    {
        bool status = true;

        foreach (Enemy enemy in enemies)
        {
            if (!enemy.isDead && enemy.group == group)
            {
                status = false;
            }
        }

        if (status == true)
        {
            group.GetComponent<EnemyGroup>().GroupDefeated();
        }
    }
}

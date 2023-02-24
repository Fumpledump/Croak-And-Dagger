using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField] private UnityEvent groupDefeated;

    public void GroupDefeated()
    {
        groupDefeated.Invoke();
    }
}

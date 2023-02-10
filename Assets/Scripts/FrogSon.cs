using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;

public class FrogSon : MonoBehaviour
{
    // Can be divided into mesh, renderer etc
    public GameObject weaponObject;
    public int attackDamage;
    // (-100 , 100)
    public int relationshipValue;

    // Player tracking
    public GameObject player;
    protected Transform target;
    protected UnityEngine.AI.NavMeshAgent agent;

    [Range(1f, 20f)]
    public float croakRadius;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.myFrog.gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if(distance <= croakRadius)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            player.GetComponent<FrogCharacter>().UnSheathWeapon();
        }
    }

    void SwitchWeapons()
    {

    }
}

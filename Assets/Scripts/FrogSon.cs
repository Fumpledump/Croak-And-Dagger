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

    // Jump stuff
    private bool grounded = true;
    private float groundedOffset = -0.14f;
    private float groundedRadius = 0.24f;
    public LayerMask groundLayers;

    private float playerGroundedValue;
    private float jumpTimeout = 0.50f;
    private float jumpTimeoutDelta;

    private float gravity = -15.0f;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    [Range(1f, 20f)]
    public float croakRadius;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.myFrog.gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = player.transform;
        playerGroundedValue = target.position.y;
        jumpTimeoutDelta = jumpTimeout;
        //Debug.Log("ground value" + playerGroundedValue);
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= croakRadius)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            player.GetComponent<FrogCharacter>().UnSheathWeapon();
        }

        //JumpAndGravity();

    }

    private void GroundedCheck()
    {
        // set sphere position, with offset

            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
                transform.position.z);

        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

    }

    private void JumpAndGravity()
    {
        //Debug.Log(target.position.y);

        if (grounded)
        {
            // stop our velocity dropping infinitely when grounded
            if(verticalVelocity < 1f)
            {
                verticalVelocity = 2f;
            }

            //Jump
            if (target.position.y > playerGroundedValue && jumpTimeoutDelta <= 0.0f)
            {
                Debug.Log("jump croak");
                verticalVelocity = 4.5f;
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
            //Debug.Log("grounded?");
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;
        }

        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        transform.position = new Vector3(transform.position.x, verticalVelocity, transform.position.z);
    }

    void SwitchWeapons()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState
{
    MoveTowards,
    MoveAway,
    Attack,
    Idle
}

public class Enemy : MonoBehaviour, IDamageable, IGrabbable
{
    [SerializeField]
    protected int health;
    public int level;
    public int attackDamage;
    const int damage = 10;
    public Animator anim;
    public GameObject player;
    public float lastGotHit = 0;
    public float getHitCooldown = 0.55f;
    public Slider healthSlider;
    public GameObject weapon;

    // Player Tracking
    public float lookRadius = 10f;
    protected Transform target;
    protected NavMeshAgent agent;

    public LayerMask whatIsGround, whatIsPlayer;
    public Vector3 walkPoint;
    bool walkPointSet;

    private float startWaitTime;
    private float waitTime;

    private bool lostPlayer;
    private Vector3 lastPlayerDestination;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    // Test
    protected float reviveCooldown = 2f;
    public float deathTime = 0;
    protected bool isDead = false;
    protected bool inAir = false;
    public ParticleSystem onHitVFX;

    // Enemy Field of View
    public float radius;
    [Range(0, 360)]

    public float angle;

    public LayerMask obstructionMask;

    public bool canSeePlayer;

    private Rigidbody rigidbody;
    
    // Start is called before the first frame update
    void Start()
    {
        health = 100;
        if (GetComponent<Animator>() != null)
            anim = GetComponent<Animator>();
        attackDamage = level * damage;
        player = GameManager.instance.myFrog.gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = player.transform;
        timeBetweenAttacks = 2.0f;
        startWaitTime = 3;
        waitTime = startWaitTime;
        lostPlayer = true;

        // Makes the field of view not run all the time to help with performance
        StartCoroutine(FOVRoutine());
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HUDUpdate();
        EnemyAI();
        if(anim.GetBool("Hit"))
            ResetHit();

        if (player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("AttackJump") && !inAir)
        {
            Debug.Log("here");
            inAir = true;
            GetComponent<Rigidbody>().AddForce(Vector3.up * 500);
        }
    }

    private void HUDUpdate()
    {
        healthSlider.value = 100 - health;
    }

    public void GetHit()
    {
        health -= player.GetComponent<FrogCharacter>().attackDamage;

        // Makes it so if the player attacks while the enemy can't see
        // they will turn back ground and fight back
        if (!canSeePlayer)
            canSeePlayer = true;
    }

    public virtual void GetHit(int attackDamage)
    {
        if(anim.GetBool("Hit") && Time.time - lastGotHit == 0f)
        {
            health -= attackDamage;
            anim.SetInteger("Health", health);
            onHitVFX.Play();       
        }
    }

    void ResetHit()
    {
        if(Time.time - lastGotHit > getHitCooldown)
            anim.SetBool("Hit", false);
    }

    protected virtual void EnemyAI()
    {
        anim.SetFloat("Speed", agent.speed);

        if (health <= 0 && !isDead)
        {
            deathTime = Time.time;
            isDead = true;
        }
        
        // Revives the enemy
        /**
        if (Time.time - deathTime > reviveCooldown && isDead)
        {
            health = 100;
            anim.SetInteger("Health", 100);
            isDead = false;

        }
        **/

        // Depending on the distance of the player and the enemy view distance
        // The enemy will enter a different state
        float distance = Vector3.Distance(target.position, transform.position);
        if (!canSeePlayer && !isDead) 
        {
            // Checks if the player was lost while chasing them
            if (!lostPlayer)
            {
                // If they were then the enemy goes to the last location the player was seen
                agent.SetDestination(lastPlayerDestination);
                Vector3 distanceToWalkPoint = transform.position - lastPlayerDestination;

                // Once the player has gotten close enough to where the player last was
                // The enemy will stop moving and stand at the last seen spot for a bit
                // Then the walkpoint is set to false and the lost player is set to true
                // So that the enemy will continue to patrol again
                if (distanceToWalkPoint.magnitude < 3f)
                {
                    StopEnemy();
                    if (waitTime <= 0)
                    {
                        walkPointSet = false;
                        lostPlayer = true;
                        waitTime = startWaitTime;
                    }
                    else
                    {
                        waitTime -= Time.deltaTime;
                    }
                }
            }
            else
            {
                Patrolling();
            }
        }
        if (canSeePlayer && !isDead && !player.GetComponent<FrogCharacter>().isDead) ChasePlayer();
        // Checks if the distance of the enemy is at the stopping distance
        // If so then that means the enemy can start attacking the player
        if (distance <= agent.stoppingDistance + 1 && !isDead && !player.GetComponent<FrogCharacter>().isDead) AttackPlayer();

    }

    private void OnTriggerExit(Collider other)
    {
        anim.SetBool("Hit", false);
    }

    private void Patrolling()
    {
        // Goes to the walkpoint set
        // Makes it look as though the enemy is aimlessly walking around
        if (!walkPointSet)
        {
            SearchWalkPoint();
            agent.SetDestination(walkPoint);
            StartEnemy(2);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 3f)
        {
            StopEnemy();
            if (waitTime <= 0)
            {
                walkPointSet = false;
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

    }

    // Stops the Enemy completely 
    // and sets up the idle animation
    private void StopEnemy()
    {
        agent.isStopped = true;
        agent.speed = 0;
        anim.SetFloat("Speed", 0);
    }

    // Starts the Enemy back up with a given speed
    // and sets the animation according to the speed set
    private void StartEnemy(int speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
        anim.SetFloat("Speed", speed);
    }

    private void SearchWalkPoint()
    {
        //Makes a random walk pont for the enemy to go to
        float randomZ = Random.Range(-10, 10);
        float randomX = Random.Range(-10, 10);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        
        // Checks if the waypoint is still on the ground
        // if so then the waypoint is set
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        // Sets the enemy to move towards the player
        agent.speed = 3;
        gameObject.GetComponent<NavMeshAgent>().isStopped = false;
        lastPlayerDestination = target.position;
        agent.SetDestination(target.position);
        lostPlayer = false;
    }

    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(weapon.transform.position, 2f);

        foreach (Collider hit in hits)
        {
            if (hit.tag == "Player")
            {
                player.GetComponent<FrogCharacter>().currentHealth -= damage;
                GameManager.instance.hudUpdate = true;
                //Debug.Log(player.GetComponent<FrogCharacter>().currentHealth);
            }
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        if(!isDead)
        transform.LookAt(target);

        // Makes it so the player doesn't keep moving while attacking the player
        agent.speed = 0;
        gameObject.GetComponent<NavMeshAgent>().isStopped = true;

        if (!alreadyAttacked)
        {
            // Attack code
            anim.SetBool("Attack", true);

            // Checks if the enemy has hit the player
            CheckHit();

            // If so then the enemy attack is reset
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        anim.SetBool("Attack", false);
    }

    // Waits a few seconds before running the field of view check
    // Makes the performance of the game a bit better

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    // Checks the field of view of the enemy
    private void FieldOfViewCheck()
    {
        // Checks for any players in the specific view radius
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, whatIsPlayer);

        // If there is a player in the radius
        if (rangeChecks.Length != 0)
        {
            Transform currentTarget = rangeChecks[0].transform;
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;

            // Checks if the player is in the specific view triangle in front of the enemy
            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

                // If it is and nothing is blocking the view then the enemy can see the player
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeePlayer = true;
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }
    public IEnumerator Grab(Transform t_player, float pullSpeed)
    {
        rigidbody.isKinematic = false;
        //perform linear interpolation
        Vector3 origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 destination;
        float pullTime = (t_player.position - transform.position).sqrMagnitude / pullSpeed;
        float timer = 0;
        while(timer < pullTime){
            //destination and pullTime need to be updated each frame to account for player movement during the pull
            destination = t_player.position + ((transform.position - t_player.position).normalized);
            pullTime = (t_player.position - transform.position).sqrMagnitude / pullSpeed;

            transform.position = Vector3.Lerp(origin, destination, timer/pullTime);

            timer += Time.deltaTime;
            yield return null;
        }
        rigidbody.isKinematic = true;
    }

    public bool GetSwingable() { return false; }
}

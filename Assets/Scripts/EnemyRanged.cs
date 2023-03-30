using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyRanged : Enemy
{
    private EnemyState enemyState = EnemyState.Idle;
    private float _rotationVelocity;
    [SerializeField] private float idealRange;
    [SerializeField] private float rangeTolerance;
    [SerializeField] GameObject projectile;
    public float attackInterval = 2.0f;
    public float projectileSpeed = 700f;
    float attackCooldown = 0f;
    bool patrolling = false;

    protected override void EnemyAI()
    {
        anim.SetInteger("MovementState", (int)enemyState);


        float distance = Vector3.Distance(target.position, transform.position);

        if(patrolling && distance > lookRadius)
        {
            enemyState = EnemyState.Patrol;
        }

        else if (distance > lookRadius || isDead)
        {
            enemyState = EnemyState.Idle;
        }

        else if(distance > idealRange + rangeTolerance && chasingTimeOut <= 0)
        {
            enemyState = EnemyState.MoveTowards;
            patrolling = false;
        }   
        
        else if(distance < idealRange - rangeTolerance)
        {
            enemyState = EnemyState.MoveAway;
            patrolling = false;
        }

        else
        {
            enemyState = EnemyState.Attack;
        }
            
        

        switch(enemyState)
        {
            case EnemyState.MoveTowards:
                if (agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(target.position);
                }
                else
                {
                    chasingTimeOut = 3f;
                    patrolling = true;
                    walkPointSet = false;
                    waitTime = startWaitTime * 2;
                }
                break;

            case EnemyState.MoveAway:
                agent.SetDestination(transform.position + (transform.position - target.position));
                break;

            case EnemyState.Attack:
                agent.SetDestination(transform.position);
                if (attackCooldown <= 0.0f)
                {
                    Attack();
                    attackCooldown = attackInterval;
                }
                else
                {
                    attackCooldown -= Time.deltaTime;
                }
                break;
            case EnemyState.Idle:
                waitTime -= Time.deltaTime;
                agent.ResetPath();
                break;
            case EnemyState.Patrol:
                RangedPatrolling();
                waitTime -= Time.deltaTime;
                break;
        }

        if (enemyState != EnemyState.Idle && enemyState != EnemyState.Patrol)
        {
            //Save old Y Rotation position
            float oldRotY = transform.rotation.eulerAngles.y;

            //Set new Look-At-Rotation
            transform.LookAt(player.transform.position);

            //Smooths Horizontal Rotation
            float RotationSmoothTime = 0.25f;
            float yRot = Mathf.SmoothDampAngle(oldRotY, transform.rotation.eulerAngles.y, ref _rotationVelocity, RotationSmoothTime);

            /*//Clamps vertical rotation
            float xRot = transform.rotation.eulerAngles.x;
            if (xRot < 325 && xRot > 270) { xRot = 325; }*/

            //Applies Changes
            transform.rotation = Quaternion.Euler(0.0f, yRot, 0.0f);
        }

        if (health <= 0 && !isDead)
        {
            isDead = true;
            enemyManager.EnemyGroupDefeated(group); // Check Enemy Trigger in Manager
            deathTime = Time.time;
            anim.SetTrigger("Die");
        }

        if (Time.time - deathTime > reviveCooldown && isDead)
        {
            gameObject.SetActive(false);
        }

        if(chasingTimeOut >= 0)
        {
            chasingTimeOut -= Time.deltaTime;
        }

        //Checking if enemy should enter patrol mode - Is idle and has waited long enough since the last patrol
        if(waitTime <= 0 && enemyState == EnemyState.Idle) 
        {
            patrolling = true;
            walkPointSet = false;
            waitTime = startWaitTime * 2;
        }

        /**
        if (Time.time - deathTime > reviveCooldown && isDead)
        {
            //Destroy(gameObject);
            
            health = 100;
            anim.SetInteger("Health", 100);
            isDead = false;
        }
        **/
    }

    void Attack()
    {
        GameObject spiderAttack = Instantiate(projectile, 
            transform.position
            + new Vector3(0.0f, 1.1f, 0.0f) //move up so it doesn't originate in the floor
            + (target.transform.position - transform.position).normalized, // move towards target so it doesn't collide with enemy
            transform.rotation);
        anim.SetTrigger("Attack");

        spiderAttack.GetComponent<Rigidbody>().AddForce((target.transform.position - transform.position).normalized * 500.0f);
    }

    //Runs Patrol Process
    private void RangedPatrolling()
    {
        // Goes to the walkpoint set
        // Makes it look as though the enemy is aimlessly walking around
        if (!walkPointSet || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            SearchWalkPoint(0.5f);
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f || waitTime <= 0)
        {
            patrolling = false;

            if (waitTime <= 0)
            {
                walkPointSet = false;
                waitTime = startWaitTime*2;
            }
        }
    }

    public override void GetHit(int attackDamage)
    {
        if (Time.time - lastGotHit == 0f)
        {
            anim.SetTrigger("Hit");
            health -= attackDamage;
            onHitVFX.Play();
        }
    }
}

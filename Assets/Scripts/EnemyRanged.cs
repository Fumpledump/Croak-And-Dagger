using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyRanged : Enemy
{
    private EnemyState enemyState;
    [SerializeField] private float idealRange;
    [SerializeField] private float rangeTolerance;
    [SerializeField] GameObject projectile;
    public float attackInterval = 2.0f;
    public float projectileSpeed = 700f;
    float attackCooldown = 0f;

    protected override void EnemyAI()
    {

        anim.SetInteger("MovementState", (int)enemyState);


        //anim.SetFloat("Speed", agent.speed);

        float distance = Vector3.Distance(target.position, transform.position);

        if (distance > lookRadius || isDead)
        {
            enemyState = EnemyState.Idle;
        }  

        else if(distance > idealRange + rangeTolerance)
        {
            enemyState = EnemyState.MoveTowards;
        }   
        
        else if(distance < idealRange - rangeTolerance)
        {
            enemyState = EnemyState.MoveAway;
        }   
        
        else
        {
            enemyState = EnemyState.Attack;
        }
            
        

        switch(enemyState)
        {
            case EnemyState.MoveTowards:
                agent.isStopped = false;
                agent.SetDestination(target.position);
                break;

            case EnemyState.MoveAway:
                agent.SetDestination(transform.position + (transform.position - target.position));
                agent.isStopped = false;
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
                agent.isStopped = true;
                break;
        }

        if (enemyState != EnemyState.Idle)
        {
            transform.LookAt(player.transform.position);

            //Clamps vertical rotation
            Vector3 myRotation = transform.rotation.eulerAngles;
            if(myRotation.x < 325 && myRotation.x > 270){ myRotation.x = 325;}

            //Applies rotational changes
            transform.rotation = Quaternion.Euler(myRotation);
        }

        if (health <= 0 && !isDead)
        {
            deathTime = Time.time;
            isDead = true;
            anim.SetTrigger("Die");
        }

        if (Time.time - deathTime > reviveCooldown && isDead)
        {
          this.gameObject.SetActive(false);

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

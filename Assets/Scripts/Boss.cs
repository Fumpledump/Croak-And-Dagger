using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField]
    float findingRadius = 10f;
    
    bool isAttacking = false;
    float attackTime = 0f;
    public bool awake = false;
    [SerializeField]
    public List<GameObject> weapons = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        // This should be 500
        // If it is lower it is due to testing
        health = 500;
        maxHealth = health;
        damage = 1;
        healthSlider.maxValue = 500;
        healthSlider.value = 0;
        anim = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        FindPlayer();
        HUDUpdate();

        if (anim.GetBool("Hit"))
            ResetHit();
    }

    void FindPlayer()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            deathTime = Time.time;
            //this.gameObject.SetActive(false);
        }

        if (Time.time - deathTime > despawnCooldown && isDead)
        {
            this.gameObject.SetActive(false);

        }

        if (!isDead)
        {
            if (Vector3.Distance(transform.position, GameManager.instance.myFrog.transform.position) < findingRadius && !isAttacking)
            {
                agent.SetDestination(GameManager.instance.myFrog.transform.position);
                anim.SetFloat("Speed", 1);
            }

            if (Vector3.Distance(transform.position, GameManager.instance.myFrog.transform.position) < 2 && !isAttacking)
            {
                // Play Animation and attack
                if (Random.Range(0, 10) > 3)
                {
                    anim.SetBool("Triple", true);
                    isAttacking = true;
                }

                else
                {
                    anim.SetBool("Heavy", true);
                    isAttacking = true;
                }
            }
        }

        if (isAttacking)
        {
            attackTime += Time.deltaTime;
            CheckHit(weapons);

            if (attackTime > 2.5f)
            {
                isAttacking = false;
                anim.SetBool("Triple", false);
                anim.SetBool("Heavy", false);
                attackTime = 0;
            }
        }
    }

    public void BossHit(int attackDamage)
    {
        if (anim.GetBool("Hit") && Time.time - lastGotHit == 0f)
        {
            anim.SetInteger("Health", health);
            health -= attackDamage;
            onHitVFX.Play();

            canSeePlayer = true;
        }
    }
}

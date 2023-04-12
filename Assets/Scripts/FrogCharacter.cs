using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using StarterAssets;
using System.Linq;
using System;


public class FrogCharacter : MonoBehaviour, IDamageable, IDataPersistence
{
    // General
    [SerializeField] private Camera camera;
    [SerializeField] StarterAssetsInputs inputs;
    [SerializeField] ParticleSystem collectFireflyParticles;

    [SerializeField]
    int sheathTime = 2;
    public int currentHealth;
    public int attackDamage;
    public float currentEnergy;               // FROGMINA IN GAME LOL
    public float speed;
    public int maxhealth;
    public int maxEnergy;
    public int fireflies = 0;
    public bool Invunerable
    {
        get
        {
            if (invulnTimer >= 0) return true;
            else return false;
        }
    }
    private float invulnTimer = 0;
    private float invulnDuration = 1.5f;
    // stretch goals
    public int skillPoints;
    public int level;

    // Combat
    public Animator anim;
    public bool isAttacking = false;
    public float cooldownTime = 1f;
    public float nextAttackTime = 0f;
    //public int noOfAttacks = 0;
    public float lastAttackTime = 0;
    public float deltaTimeBetweenCombos = 1f;
    public List<GameObject> weapon;
    public GameObject weaponTrail;
    float maxComboDelay = 0.55f;

    // Croak Transform
    public GameObject croakPopPrefab;
    private GameObject croakPop;
    private ParticleSystem croakPopVFX;
    private GameObject weaponPop;
    private ParticleSystem weaponPopVFX;

    // Revamped Combat
    private int curMaceAttack = 0;
    private float timeSinceLastAttack = 0;
    private float attackTimeBuffer = 0.35f;
    private float comboTimeBuffer = 0.8f;
    private float comboWalkOutTimer = 0.6f;
    private CapsuleCollider weaponCollider;
    private List<GameObject> hitEnemies;
    [SerializeField] private GameObject targetSwitcher;

    // probably switch to the frog son
    public FrogSon Son;
    private float croakTimer = 4;

    // Narrative
    public bool inDialog;

    //Shader/VFX
    private float noiseScale = 0.1f;
    private Material croakMat;
    private Material swordMat;
    private int dissolvePercent = 0;
    private int materializePercent = 0;

    // Death and Respawn
    public bool isDead = false;
    public float deathTime = 0;
    protected float reviveCooldown = 5f;
    public Vector3 respawnPoint;

    // Tongue
    [Header("Tongue Settings")]
    [SerializeField] float tongueLength = 1.0f; //how far away from the player can the tongue reach to grab things
    [SerializeField] float pullSpeed = 50.0f; //how quickly a grabbed object will be pulled to the player
    private float tongueAttackCheckRadius = 1f;
    private bool tonguePressed = false;
    private float maxSwingingDistance = 20f;
    private bool canSwing = true;
    private LineRenderer tongueLine;
    private Vector3 grapplePoint;
    public Transform tongueTip;
    private Spring spring;
    private bool tongueAttack = false;
    [SerializeField] int quality;
    [SerializeField] float damper;
    [SerializeField] float strength;
    [SerializeField] float velocity;
    [SerializeField] float waveCount;
    [SerializeField] float waveHeight;
    [SerializeField] AnimationCurve affectCurve;



    // Start is called before the first frame update
    void Start()
    {
        inputs = GetComponent<StarterAssetsInputs>();
        anim = GetComponent<Animator>();
        level = 1;
        currentEnergy = 100;
        maxhealth = 100;
        maxEnergy = 100;
        attackDamage = 20;
        speed = gameObject.GetComponent<ThirdPersonController>().MoveSpeed;
        
        croakMat = weapon[2].GetComponent<Renderer>().material;
        swordMat = weapon[0].GetComponent<Renderer>().material;

        respawnPoint = transform.position;

        // Start with weapon sheathed
        weapon[0].SetActive(false); // weapon
        weapon[2].SetActive(GameManager.instance.croakEnabled); // croak
      
        //weapon[2].transform.position = new Vector3(transform.position.x - transform.forward.x, transform.position.y, transform.position.z);
        timeSinceLastAttack = attackTimeBuffer;

        weaponCollider = weapon[0].GetComponent<CapsuleCollider>();

        hitEnemies = new List<GameObject>();

        // Set Croak's positon



        // Initialize croak Effect
        croakPop = Instantiate(croakPopPrefab, Vector3.zero, Quaternion.identity);
        weaponPop = Instantiate(croakPopPrefab, Vector3.zero, Quaternion.identity);
        // get croak pop particle effect
        croakPopVFX = croakPop.GetComponent<ParticleSystem>();
        croakPop.transform.position = weapon[2].transform.position;
        croakPop.SetActive(true);
        weaponPopVFX = weaponPop.GetComponent<ParticleSystem>();
        weaponPop.transform.position = weapon[1].transform.position;
        weaponPop.SetActive(true);

        // Get Line render for tongue
        tongueLine = GetComponent<LineRenderer>();
        spring = new Spring();
        spring.SetTarget(0);
    }

    public void LoadData(GameData data)
    {
        Debug.Log(transform.position);
        this.respawnPoint = data.respawnPoint;
        this.currentHealth = data.currentHealth;
        this.fireflies = data.fireflies;
        //this.currentHealth = 80;
        currentEnergy = 100;
        this.GetComponent<CharacterController>().enabled = false;
        this.transform.position = respawnPoint;
        this.GetComponent<CharacterController>().enabled = true;
        //weapon[2].GetComponent<NavMeshAgent>().enabled = false;
        weapon[2].transform.position = new Vector3(transform.position.x - transform.forward.x, transform.position.y, transform.position.z); ;
        //weapon[2].GetComponent<NavMeshAgent>().enabled = true;
        GameManager.instance.hudUpdate = true;
    }

    public void SaveData(ref GameData data)
    {
        if (!isDead)
        {
            //data.respawnPoint = new Vector3(17, 2, -22);
            data.respawnPoint = this.respawnPoint;
            data.currentHealth = this.currentHealth;
            data.fireflies = this.fireflies;
        }
    }

    private void Update()
    {
        //Debug.Log(currentHealth);
        timeSinceLastAttack += Time.deltaTime;
        if (invulnTimer > 0)
        {
            invulnTimer -= Time.deltaTime;
        }
        

        // if the time since the last attack is greater than the input buffer, end the combo
        if (timeSinceLastAttack > comboTimeBuffer)
        {
            EndAttackCombo();

            if(!tongueAttack)
            anim.SetBool("TongueAttack", false);

            if(croakTimer > 0)
            {
                croakTimer -= Time.deltaTime;
            }
            else if(GameManager.instance.croakEnabled)
            {
                SheathWeapon();
            }

        }

        RegenerateEnergy();
        //PComboDone();
        //SheathWeapon();

        if (currentHealth <= 0 && !isDead) Dead();


        if (Time.time - deathTime > reviveCooldown && isDead) Respawn();


        if (Time.time - lastAttackTime > maxComboDelay){
            //noOfAttacks = 0;
        }

        if (inDialog)
        {
            timeSinceLastAttack = 0f;
        }
        // If Player is in Dialog Sequence disable combat controls until finished
        if (inDialog) return;


        if (inputs.pAttack && !GameManager.instance.myFrog.isDead)
        {
            Debug.Log("time: "+timeSinceLastAttack+", buffer: "+attackTimeBuffer);
            // mace combo
            if(timeSinceLastAttack > attackTimeBuffer && GameManager.instance.croakEnabled && anim.GetInteger("MaceAttack") < 3) MaceAttack();
            // final hit takes double recovery before starting chain again
            else if (timeSinceLastAttack > attackTimeBuffer * 2 && anim.GetInteger("MaceAttack") == 3) MaceAttack();


            isAttacking = true;
            inputs.pAttack = false;
        }
        if (inputs.hAttack)
        {
            //HeavyAttack();

            inputs.hAttack = false;
        }

        if (inputs.holdingTongue)
        {
            TongueGrab();
            //tongueLine.positionCount = 2;
        } 
        else if(!inputs.holdingTongue)
        {

            //tongueLine.positionCount = 0;
            //tonguePressed = false;
            //inputs.reportTongueChange = false;

            //handle ending tongue swing
            GetComponent<ThirdPersonController>().CancelSwing();
            // Tongue attack
            if (tongueAttack)
            {
                bool enemyNear = false;
                // check for an enemy within the radius before setting off attack
                Collider[] collisions = Physics.OverlapSphere(this.transform.position, tongueAttackCheckRadius);
                foreach (Collider c in collisions)
                {
                    if (c.gameObject.GetComponent<Enemy>() != null)
                    {
                        enemyNear = true;
                    }
                }
                if (enemyNear)
                {
                    anim.SetBool("TongueAttack", tongueAttack);
                    TongueAttack();
                }
            }
        }

        // grounded check for swining
        if (GetComponent<ThirdPersonController>().Grounded == true)
        {
            canSwing = true;
        }
        //update material


        if (weapon[2].activeSelf && weapon[0].activeSelf)
        {
            //swordMat.SetFloat("_CutoffHeight", swordMat.GetFloat("_CutoffHeight") - 0.01f);
        }
        //croakMat.SetFloat("_CutoffHeight", croakMat.GetFloat("_CutoffHeight") - 0.01f);
        else
        {
            if (weapon[2].activeSelf)
            {
                croakMat.SetFloat("_CutoffHeight", weapon[2].GetComponent<Transform>().position.y + 1);
            }

            if (weapon[0].activeSelf)
            {
                swordMat.SetFloat("_CutoffHeight", weapon[0].GetComponent<Transform>().position.y + 1);
            }
        }

    }

    private void LateUpdate()
    {
        DrawTongue();
    }
    #region COMBAT_SYSTEM_V2
    private void MaceAttack()
    {
        // causes some bugs.... dont include this for now
        if(targetSwitcher.GetComponent<TargetSwitch>().currentTarget != null)
        {
            //this.gameObject.transform.LookAt(targetSwitcher.GetComponent<TargetSwitch>().currentTarget.transform);
        }

        hitEnemies.Clear();
        UnSheathWeapon();
        timeSinceLastAttack = 0;
        
        if (curMaceAttack + 1 > 3) // loop attack
        {
            curMaceAttack = 1;
        }
        else if (anim.GetBool("Grounded") || anim.GetInteger("MaceAttack") == 0) // can start, but only continue combo if grounded
        {
            curMaceAttack++;
            // trigger air attack motion for third person controller if airborn for first attack
            if (!anim.GetBool("Grounded"))
            {
                gameObject.GetComponent<ThirdPersonController>().AirAttack();
            }
        }
        gameObject.GetComponent<ThirdPersonController>().ComboDirectionReset();
        anim.SetInteger("MaceAttack", curMaceAttack);
        weaponTrail.active = true;
        croakTimer = 4;
    }

    public void EndAttackCombo()
    {
        Debug.Log("called end");
        curMaceAttack = 0;
        anim.SetInteger("MaceAttack", curMaceAttack);
        isAttacking = false;

        //timeSinceLastAttack = 0;
        //SheathWeapon();
        weaponTrail.active = false;
        //croakTimerActive = true;
    }

    private void SheathWeapon()
    {
        if (weapon[2].activeSelf) return; // if weapon already sheathed
        weapon[0].SetActive(false); // weapon
        weapon[2].SetActive(true); // croak

        // Set Croak's Position
        //weapon[2].GetComponent<NavMeshAgent>().enabled = false;
        weapon[2].transform.position = new Vector3(transform.position.x - transform.forward.x, transform.position.y, transform.position.z); ;
        //weapon[2].GetComponent<NavMeshAgent>().enabled = true;

        weaponPop.transform.position = weapon[0].transform.position;
        croakPop.transform.position = weapon[2].transform.position;
        weaponPopVFX.Play();
        croakPopVFX.Play();
    }
    public void UnSheathWeapon()
    {
        if (weapon[0].activeSelf) return; // if weapon already unsheathed
        croakTimer = 10;
        weapon[0].SetActive(true); // weapon
        weapon[2].SetActive(false); // croak

        weaponPop.transform.position = weapon[0].transform.position;
        croakPop.transform.position = weapon[2].transform.position;
        weaponPopVFX.Play();
        croakPopVFX.Play();
    }

    public void CheckHit(GameObject enemy)
    {
        if (!hitEnemies.Contains(enemy) && isAttacking)
        {
            enemy.GetComponent<Enemy>().lastGotHit = Time.time;
            if (enemy.tag == "Enemy")
            {
                enemy.GetComponent<Animator>().SetBool("Hit", true);
                enemy.GetComponent<Enemy>().GetHit(attackDamage);
            }
            else if(enemy.tag == "Boss")
            {
                enemy.GetComponent<Animator>().SetBool("Hit", true);
                enemy.GetComponent<Boss>().BossHit(attackDamage);
            }

            hitEnemies.Add(enemy);
        }
    }

    #endregion

    #region COMBAT_SYSTEM_V1

    /*
    void CheckHit()
    {
        Collider[] hits = Physics.OverlapSphere(weapon[0].transform.position, 0.5f);
        //Collider[] hits2 = Physics.OverlapSphere(GameManager.instance.myFrog.weapon[1].transform.position, 0.5f);
        //hits = hits.Concat(hits2).ToArray();
        foreach (Collider hit in hits)
        {
            if (hit.tag == "Enemy")
            {
                hit.gameObject.GetComponent<Animator>().SetBool("Hit", true);
                hit.gameObject.GetComponent<Enemy>().lastGotHit = Time.time;
                hit.gameObject.GetComponent<Enemy>().GetHit(attackDamage);
            }
        }
    }
    
    public void HeavyAttack()
    {
        if (noOfAttacks >= 2 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && GetComponent<StarterAssetsInputs>().hAttack
           && anim.GetCurrentAnimatorStateInfo(0).IsName("PAttack2"))
        {
            anim.SetBool("PAttack2", false);
            anim.SetBool("HAttackC", true);
            CheckHit();
        }
        else
        {
            anim.SetBool("HAttack", true);
            CheckHit();
        }
    }
    
    public void PrimaryAttack()
    {
        Debug.Log("primary attack");
        lastAttackTime = Time.time;
        if (noOfAttacks == 1)
        {
            Debug.Log("should do things");
            // play PAttack1
            anim.SetBool("PAttack1", true);
            CheckHit();
        }
        // mechanic for saving Pattacks for combos down below

        noOfAttacks = Mathf.Clamp(noOfAttacks, 0, 3);

        PrimaryAttack2();
        PrimaryAttack3();

    }

    public void PrimaryAttack2()
    {
        if (noOfAttacks >= 2 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("PAttack1"))
        {
            anim.SetBool("PAttack1", false);
            anim.SetBool("PAttack2", true);
            CheckHit();
        }
    }

    public void PrimaryAttack3()
    {
        
        if (noOfAttacks >= 3 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("PAttack2"))
        {
            anim.SetBool("PAttack2", false);
            anim.SetBool("PAttack3", true);
            CheckHit();
        }
    }
    
    public void PComboDone()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
        {
            anim.SetBool("PAttack3", false);
            anim.SetBool("PAttack2", false);
            anim.SetBool("PAttack1", false);
            anim.SetBool("HAttackC", false);
            anim.SetBool("HAttack", false);
            noOfAttacks = 0;
        }
    }

    /// <summary>
    /// Sheath/Unsheath depending on last time attacked
    /// </summary>
    public void SheathWeapon()
    {
        if (Time.time - lastAttackTime > sheathTime)
        {
            if (Time.time - lastAttackTime >= sheathTime + 0.5)
            {
                weapon[0].SetActive(false);
            }
            weapon[2].SetActive(true);
        }
    }
    */
    #endregion

    public void OnLevelUp()
    {
        // Designers change the stats
        level++;
        skillPoints++;
        attackDamage += 2;
        maxEnergy += 2;
        maxhealth += 2;
    }

    public void RegenerateEnergy()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += Time.deltaTime;
            GameManager.instance.hudUpdate = true;
        }     
    }

    public bool Dash()
    {
        if (currentEnergy >= 20)
            return true;

        return false;
    }

    public void Dead()
    {
        deathTime = Time.time;
        isDead = true;
        anim.SetBool("isDead", isDead);
    }

    public void Respawn()
    {
        isDead = false;
        anim.SetBool("isDead", isDead);
        DataPersistenceManager.instance.LoadGame();
    }

    void TongueGrab(){

        // a little yucky but it works
        // adding Vector3.up adjusts for the player object's anchor being on the floor, and adding the forward vector of the camera ensures we don't accidentally detect the shield or weapon objects
            // camera forward offset could be replaced by a layermask later for a more robust implementation
        Vector3 tonguePosStart = transform.position + Vector3.up; 
        Vector3 tongueDirection = camera.transform.forward;
        tongueDirection.y = 0;
        RaycastHit raycast = new RaycastHit();

        bool tongueHasHit = false;
        while (!tongueHasHit && tongueDirection.y < 1)
        {

            Debug.DrawLine(tonguePosStart, tonguePosStart + tongueDirection * tongueLength, Color.red, 3.0f);

            Physics.Raycast(tonguePosStart, tongueDirection, out raycast);

            grapplePoint = raycast.collider.transform.position;
            if (raycast.collider && raycast.collider.gameObject.GetComponent<IGrabbable>() != null && raycast.distance <= maxSwingingDistance)
            {
                IGrabbable g = raycast.collider.gameObject.GetComponent<IGrabbable>();
                if (g != null)
                {
                    //tongueLine.positionCount = 2;
                    tongueHasHit = true;
                    if (g.GetSwingable() && canSwing)
                    {
                        canSwing = false;
                        GetComponent<ThirdPersonController>().Swing(raycast.point);

                        grapplePoint = raycast.collider.transform.position;
                    }
                    else
                    {
                        Vector3 playerToEnemy = transform.position - raycast.collider.gameObject.transform.position;
                        Debug.DrawLine(transform.position, raycast.collider.gameObject.transform.position, Color.green, 1.0f);
                        StartCoroutine(g.Grab(transform, pullSpeed));

                        //grapplePoint = raycast.collider.gameObject.transform.position;
                        Enemy tongueAttackEnemy = raycast.collider.gameObject.transform.GetComponent<Enemy>();
                        if (tongueAttackEnemy != null)
                        {
                            grapplePoint = raycast.collider.gameObject.transform.GetComponent<Enemy>().grabPoint.position;
                        }
                        else 
                        {
                            grapplePoint = raycast.collider.transform.position;
                        }
                        tongueAttack = true;
                    }

                }
            }
            else
            {
                //Debug.Log("Tongue direction" + tongueDirection);
                //tongueDirection.y = 0;
                grapplePoint = tongueTip.transform.position + transform.rotation.eulerAngles.normalized * tongueLength;
            }
            tongueDirection.y += 0.05f;

        }
        
    }

    private Vector3 currentGrapplePos;
    public void DrawTongue()
    {
        //if (!inputs.holdingTongue) return;
        ////currentGrapplePos = Vector3.Lerp(currentGrapplePos, grapplePoint, Time.deltaTime * 8f);
        //tongueLine.SetPosition(0, tongueTip.position);
        ////tongueLine.SetPosition(1, currentGrapplePos);
        //tongueLine.SetPosition(1, grapplePoint);

        if (!inputs.holdingTongue) {
            currentGrapplePos=tongueTip.position;
            spring.Reset();
            if (tongueLine.positionCount > 0)
            {
                tongueLine.positionCount = 0;
            }
            return;
        }

        if (tongueLine.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            tongueLine.positionCount = quality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 up = Quaternion.LookRotation((grapplePoint - tongueTip.position).normalized) * Vector3.up;
        currentGrapplePos = Vector3.Lerp(currentGrapplePos, grapplePoint, Time.deltaTime * 12f);

        for (int i = 0; i < quality + 1; i++)
        {
            float delta = (float)i /(float)quality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);
            tongueLine.SetPosition(i, Vector3.Lerp(tongueTip.position, currentGrapplePos, delta) + offset);
        }
    }
    private void TongueAttack()
    {
        hitEnemies.Clear();
        timeSinceLastAttack = 0;
        isAttacking = true;
        weaponTrail.active = true;
        croakTimer = 4;
        if (GameManager.instance.croakEnabled)
        {
            UnSheathWeapon();
            anim.Play("TongueAttack");
            tongueAttack = false;
        }
        tongueAttack = false;
    }

    public void AddFirefly(GameObject firefly)
    {
        Debug.Log("Adding firefly");
        fireflies++;
        firefly.SetActive(false);
        GameManager.instance.hudUpdate = true;
        collectFireflyParticles.Play();
    }

    public void TakeDamage(int damage)
    {
        if (invulnTimer <= 0) // only take damage when vunerable
        {
            invulnTimer = invulnDuration;
            currentHealth -= damage;
        }

        gameObject.GetComponent<ThirdPersonController>().knockbackDirection = new Vector3(this.transform.forward.normalized.x, 0, this.transform.forward.normalized.z);

        // knockback, will always trigger independent of damage taken
        StartCoroutine(gameObject.GetComponent<ThirdPersonController>().KnockbackCoroutine());
    }

}

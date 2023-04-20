using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;
using StarterAssets;


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
    private ThirdPersonController playerThirdPersonControllerScript;
    private FrogCharacter playerFrogCharacterScript;
    //protected UnityEngine.AI.NavMeshAgent agent;

    // Jump stuff
    public bool grounded = true;

    private float gravity = -50.0f;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    private float targetRotation = 0.0f;

    public bool isJumping;
    private StarterAssetsInputs _input;
    private float rotationVelocity;
    public float RotationSmoothTime = 0.12f;

    private CharacterController controller;

    private GameObject mainCamera;

    [Range(1f, 20f)]
    public float croakRadius;

    public StarterAssetsInputs Input
    {
        get
        {
            return _input;
        }
        set
        {
            _input = value;
        }
    }

    private void Awake()
    {
        // get a reference to our main camera
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.myFrog.gameObject;
        target = player.transform;
        controller = GetComponent<CharacterController>();
        playerThirdPersonControllerScript = player.GetComponent<ThirdPersonController>();
        playerFrogCharacterScript = player.GetComponent<FrogCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        //_input = player.GetComponent<ThirdPersonController>().Input;

        float distance = Vector3.Distance(target.position, transform.position);

        // Checks if Croak is too far or too close
        if (distance <= croakRadius && distance >= 2)
        {
            Move();
        }
        // Checks specifically if Croak is too far
        else if(distance > croakRadius)
        {
            playerFrogCharacterScript.UnSheathWeapon();
        }
        
        JumpAndGravity();

    }

    private void Move()
    {
        // Rotates Croak to face the same way as Dagger

        transform.LookAt(target.position);

        // Actual movement

        Vector3 direction = target.position - transform.position;
        Vector3 movement = direction.normalized * playerThirdPersonControllerScript.Speed * Time.deltaTime;

        controller.Move(movement);
    }

    private void JumpAndGravity()
    {

        if (grounded)
        {
            
            //Jump
            //if (isJumping)
            if(_input.jump && player)
            {
                verticalVelocity = 10f;
                controller.Move(new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            }
            
        }
        else
        {

            if (verticalVelocity < terminalVelocity)
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            controller.Move(new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
        }

    }

    void SwitchWeapons()
    {

    }

}

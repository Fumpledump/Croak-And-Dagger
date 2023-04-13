using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 7.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 10.0f;

        [Tooltip("Dash speed of the character in m/s")]
        public float DashSpeed = 12f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 10f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -25.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.24f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        //knockback
        public Vector3 knockbackDirection;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        [SerializeField] private CinemachineVirtualCamera followCam;

        // player
        private float _speed;
        private float _slideSpeed; //Flat speed of slope sliding movement
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        private bool _swinging = false;

        // jump stuff
        private float holdJumpTimer = 0;
        private bool jumpHeld;
        private int holdJumpCount = 0;
        const int HOLDJUMP_COUNT_MAX = 3;

        public GameObject croak;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;


        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        // Narrative
        public bool inDialog;

        // Swing
        public bool inSwing;
        Coroutine _swingCoroutine;

        public StarterAssetsInputs Input
        {
            get
            {
                return _input;
            }
        }

        public float Speed
        {
            get
            {
                return _speed;
            }
        }

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        // sliding variables
        private Vector3 hitPointNormal;
        //Determines if character is on a slope and if the slope is beyond the controller slope limit
        private bool IsSliding
        {
            get
            {
                if (!_controller.isGrounded) return false;
                return Vector3.Angle(hitPointNormal, Vector3.up) > _controller.slopeLimit + 10;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if(hit.gameObject.layer != LayerMask.NameToLayer("IgnoreSlide") && hit.gameObject.layer != LayerMask.NameToLayer("Enemy"))
                hitPointNormal = hit.normal;
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Set slide speed
            _slideSpeed = 4.5f;

            croak.GetComponent<FrogSon>().Input = Input;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            // Checks if the player is dead or swinging
            // if not then the player is able to control Dagger

            if (GameManager.instance.myFrog.isDead)
            {

            }
            if (!inSwing)
            {
                JumpAndGravity();
                GroundedCheck();

                // cannot move if attacking
                Move();
                

                //Check for character sliding, update movement if so
                if (IsSliding)
                {
                    //Convert normal of collision point to slope of collision point
                    Vector3 realSlopeDirection = Vector3.Cross(Vector3.Cross(hitPointNormal, Vector3.down), hitPointNormal);
                    Vector3 targetDirection = new Vector3(realSlopeDirection.x, realSlopeDirection.y, realSlopeDirection.z);
                    // move the player
                    
                    
                    _controller.Move(targetDirection.normalized * (_slideSpeed * 1.2f * Time.deltaTime));
                }

                hitPointNormal = Vector3.zero;
            }
            _animator.SetBool("InDialog",inDialog);
        }

        private void LateUpdate()
        {
            if (inDialog)
            {
                return;
            }

            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset

            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            // check if this frame switched switched groundedness
            if (Grounded != Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore))
            {
                gameObject.GetComponent<FrogCharacter>().EndAttackCombo();
            }
            
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            croak.GetComponent<FrogSon>().grounded = Grounded;
            //Debug.Log("grounded frog: " + Grounded);

            //Debug.Log("Grounded state: " + Grounded);
            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);

                // reset attack when landing
                //_animator.SetBool("isInAir", !Grounded);
            }
        }

        private void CameraRotation()
        {
            if (_input.lockedOn)
            {
                _cinemachineTargetYaw = followCam.transform.rotation.eulerAngles.y;
                _cinemachineTargetPitch = followCam.transform.rotation.eulerAngles.x;
            }
            // if there is an input and camera position is not fixed
            else if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.dash)
                targetSpeed = GameManager.instance.myFrog.Dash() ? DashSpeed : targetSpeed;

            // If Player is in Dialog Sequence disable movement controls until finished
            if (inDialog || GameManager.instance.myFrog.isDead)
            {
                targetSpeed = 0;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // If Player is in Dialog Sequence disable movement controls until finished
            if (inDialog)
            {
                inputMagnitude = 0;
            }

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            // FOR COMBAT: only create regular rotation and movement when not attacking

            if (_animator.GetInteger("MaceAttack") == 0)
            {
                if (_input.move != Vector2.zero && !GameManager.instance.myFrog.isDead)
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                      _mainCamera.transform.eulerAngles.y;

                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    if (!inDialog)
                    {
                        // rotate to face input direction relative to camera position
                        transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                    }
                }


                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            } else if (_animator.GetInteger("MaceAttack") > 0)
            {
                /*
                // Rotation to always attack forward during unlocked camera

                if (!_input.lockedOn)
                {
                    _targetRotation = Mathf.Atan2(0, 1) * Mathf.Rad2Deg +
                                          _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                            RotationSmoothTime);

                    // rotate to face forward relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
                else // locked on camera
                {
                    
                    Vector3 enemyPosition = GetComponent<TargetLock>().target.transform.position;


                    transform.LookAt(enemyPosition);
                    Vector3 eulerAngles = transform.rotation.eulerAngles;
                    eulerAngles.x = 0;
                    eulerAngles.z = 0;

                    transform.rotation = Quaternion.Euler(eulerAngles);
                    
                }
                */

                // get movement direction
                transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
                Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
                

                // jump attack motion if airborn
                if (!_animator.GetBool("Grounded")) {
                    _controller.Move(targetDirection.normalized * (MoveSpeed * 1.7f * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
                // move the player for the first 20% of the animations run time
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.2f && !inDialog)
                {
                    _controller.Move(targetDirection.normalized * (MoveSpeed * 1.5f * Time.deltaTime) +
                        new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
                }
            }






            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

            if (_input.dash)
                StartCoroutine(DashCoroutine(0.4f));

        }

        //called from FrogCharacter.cs
        public void Swing(Vector3 anchor)
        {
            if (!_swinging)
            {
                inSwing = true;
                _swingCoroutine = StartCoroutine(SwingCoroutine(anchor));
            }
        }

        //called from FrogCharacter.cs
        public void CancelSwing()
        {
            if(_swinging)
            {
                _swinging = false;
                StopCoroutine(_swingCoroutine);
                StartCoroutine(SwingBoostCoroutine());
            }
        }
        public void AirAttack()
        {
            _verticalVelocity = 2;
        }

        /// <summary>
        /// handles the player swinging
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        IEnumerator SwingCoroutine(Vector3 anchor)
        {
            /*
            The basic idea behind swinging is to imagine a sphere that emcompasses the volume in which the swing takes place and move the player along the surface of that sphere. 
            
            The sphere is centered on the anchor (the point on the swingable object which the tongue his hit) and has a radius equal to the magnitude of the the vector between the anchor and the player.
            Moving along the surface of the sphere takes the following basic shape:
                1. Calculate where the player would be if they stopped swinging this frame. This is called the ghost position, or ghostPos
                2. Find the point on the surface of the sphere closest to the ghost position. This is called the sphere point
                3. Recompute the players velocity as the vector from their current position to the sphere point
                4. Move the Character Controller according to the new velocity, and remember that velocity for the next frame to calcualte a new ghost position
                    a. Gravity will be added to this new velocity to give it a more physics-y feel
            */
            Vector3 ghostPos = Vector3.zero; //where the player would be if they weren't swinging
            Vector3 spherePoint = Vector3.zero; //the point on the sphere closest to ghostPos
            Vector3 swingDir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            
            float swingRadius = (anchor - transform.position).magnitude;
            float swingSpeed = 0.01f;
            _swinging = true;

            Vector3 velocity = ((swingDir * MoveSpeed) - (Vector3.down * Gravity)) * swingSpeed * Time.deltaTime;
            RaycastHit groundCheck;

            while (inSwing)
            {
                velocity -= (Vector3.down * Gravity * swingSpeed) * Time.deltaTime;
                ghostPos = transform.position + velocity;
                Vector3 anchorToGhost = ghostPos - anchor;
                if(anchorToGhost.sqrMagnitude > swingRadius * swingRadius)
                {
                    spherePoint = anchor + (anchorToGhost.normalized * swingRadius);
                    velocity = spherePoint - transform.position;
                }
                else
                {
                    velocity = (swingDir * MoveSpeed * swingSpeed) * Time.deltaTime;
                }

                Physics.Raycast(transform.position, velocity.normalized, out groundCheck, velocity.magnitude * 3);
                if (groundCheck.collider != null)
                {
                    CancelSwing();
                }

                _controller.Move(velocity);
                yield return null;
            }
            _swinging = false;
        }

        /// <summary>
        /// called once a swing is finished, gives the player a small boost forward
        /// </summary>
        /// <returns></returns>
        IEnumerator SwingBoostCoroutine()
        {
            //TODO: change this to be determined by the player's WASD/Joystick input instead of Camera.forward
            Vector3 boostDirection = Camera.main.transform.forward;
            float boostSpeed = 10.0f;
            float deltaBoost = -1.0f;
            for(float timer = 0; timer < 0.2f; timer += Time.deltaTime)
            {
                _controller.Move((boostDirection * boostSpeed) * Time.deltaTime);
                boostSpeed += deltaBoost * Time.deltaTime;
                if(boostSpeed < 0) { boostSpeed = 0; }
                yield return null;
            }
            inSwing = false;
        }

        /// <summary>
        /// Coroutine for dashing mechanism
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        IEnumerator DashCoroutine(float time)
        {
            _animator.SetBool("Dash", true);
            yield return new WaitForSeconds(time);
            // to update once
            if(_input.dash)
                GameManager.instance.myFrog.currentEnergy = GameManager.instance.myFrog.currentEnergy < 20 ? GameManager.instance.myFrog.currentEnergy : GameManager.instance.myFrog.currentEnergy - 20;

            GameManager.instance.hudUpdate = true;
            _animator.SetBool("Dash", false);
            _input.dash = false;
        }

        private void JumpAndGravity()
        {
            // check if player is holding down jump key
            jumpHeld = (_playerInput.currentActionMap.actions[2].ReadValue<float>() > 0.1f) ? true : false;

            if (inDialog)
            {
                jumpHeld = false;
            }

            croak.GetComponent<FrogSon>().isJumping = false;
            if (Grounded && _animator.GetInteger("MaceAttack") == 0 && !GameManager.instance.myFrog.isDead)
            {
                // reset hold jump timer
                holdJumpTimer = 0;
                holdJumpCount = 0;

                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                //Stop Input From being Buffered during Dialog
                if(_input.jump && inDialog)
                {
                    _input.jump = false;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    //_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    //Debug.Log(Mathf.Sqrt(JumpHeight * -2f * Gravity));
                    _verticalVelocity = JumpHeight;

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                    croak.GetComponent<FrogSon>().isJumping = true;

                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {

                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
                if (!jumpHeld) holdJumpCount = HOLDJUMP_COUNT_MAX;

                if (jumpHeld && holdJumpCount < HOLDJUMP_COUNT_MAX)
                {
                    //Debug.Log("holding and count less than 4");
                    if(holdJumpTimer > 0.05f)
                    {
                        holdJumpTimer = 0;
                        _verticalVelocity += 2;
                        holdJumpCount++;
                    }
                    else
                    {
                        holdJumpTimer += Time.deltaTime;
                    }
                    
                    
                    //Debug.Log("hey: "+ holdJumpTimer);
                }
            }


            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        public IEnumerator KnockbackCoroutine()
        {
            _verticalVelocity = JumpHeight / 2;
            //TODO: change this to be determined by the player's WASD/Joystick input instead of Camera.forward
            float boostSpeed = 5.0f;
            float deltaBoost = -1.0f;
            for (float timer = 0; timer < 0.2f; timer += Time.deltaTime)
            {
                _controller.Move((knockbackDirection * boostSpeed) * Time.deltaTime);
                boostSpeed += deltaBoost * Time.deltaTime;
                if (boostSpeed < 0) { boostSpeed = 0; }
                yield return null;
            }
            inSwing = false;
        }
        public void ComboDirectionReset()
        {
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
            }
        }
    }
}
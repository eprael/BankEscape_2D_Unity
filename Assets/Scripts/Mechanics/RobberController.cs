using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using Platformer.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customization.
    /// </summary>
    public class RobberController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        /// <summary>
        /// Controls how much the jump is cut when the jump button is released.
        /// 1 = no cut, 0 = instant stop, 0.5 = half height, etc.
        /// </summary>
        [Range(0f, 1f)]
        public float jumpDeceleration = 0.5f; // Exposed for tuning

        [Header("Jump Timing")]
        [Tooltip("Minimum time (in seconds) before we consider ourselves airborne/landed")]
        public float minJumpAirTime = 0.1f;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;


        private Vector2 move = Vector2.zero; // Initialize to avoid null issues
        private bool jumpInput;
        private float jumpStartTime;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        [Header("Energy System")]
        public RobberEnergy robberEnergy; // robberEnergy component 
        public float damageCooldown = 2.0f; // Seconds between damage hits
        private float lastDamageTime = -999f; // When we last took damage

        [Header("Score System")]
        public RobberScore robberScore; // robberScore component 

        [Header("Escape State")]
        [HideInInspector]
        public bool isEscaping = false; // Flag to prevent cop catches during escape


        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            spriteRenderer.color = Color.white; // Remove any tint
            // Ensure Animator has jumpState parameter
            if (animator != null && !animator.parameters.Any(p => p.name == "jumpState" && p.type == AnimatorControllerParameterType.Int))
            {
                Debug.LogWarning("Animator missing 'jumpState' integer parameter. Please add it to the Animator Controller.");
            }
            var rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody != null)
                rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                // detect jump input only when grounded
                if (Input.GetButtonDown("Jump") && jumpState == JumpState.Grounded)
                {
                    jumpState = JumpState.PrepareToJump;
                    jumpStartTime = Time.time;           // record jump start
                }

                // horizontal movement
                //if (SceneManager.GetActiveScene().name == "Level1")
                //{
                //    move.x = Input.GetAxis("Horizontal");
                //}
                //else
                //{
                    float slowDown = Input.GetKey(KeyCode.A) ? 0.7f : 0f;
                    float speedUp = Input.GetKey(KeyCode.D) ? 0.3f : 0f;
                    move.x = Mathf.Clamp(1f - slowDown + speedUp, 0f, 1.5f);
                //}

                // detect jump release
                if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                }
            }
            else
            {
                move.x = 0;
            }

            base.Update(); // calls FixedUpdate internally
        }


        void OnTriggerEnter2D(Collider2D other)
        {
            // Powerup collision
            if (other.gameObject.layer == LayerMask.NameToLayer("Powerups"))
            {
                PowerupController powerup = other.GetComponent<PowerupController>();

                if (powerup != null)
                {
                    int energyGain = powerup.GetEnergyValue();
                    Debug.Log($"Collected powerup! Gained {energyGain} energy");

                    powerup.PlayCollectSound();

                    if (robberEnergy!= null)
                    {
                        robberEnergy.IncreaseEnergy(energyGain);
                    }
                }

                Destroy(other.gameObject);
            }

            // Loot collision
            else if (other.gameObject.layer == LayerMask.NameToLayer("Loot"))
            {
                ScoreController score = other.GetComponent<ScoreController>();

                if (score != null)
                {
                    int lootValue = score.GetLootValue();
                    Debug.Log($"Collected loot! Gained {lootValue} dollars");

                    score.PlayCollectSound();

                    if (robberScore != null)
                    {
                        robberScore.IncreaseScore(lootValue);
                    }
                }
                Destroy(other.gameObject);
            }


            // Cop collision - ONLY if not already escaping
            else if (other.gameObject.layer == LayerMask.NameToLayer("AI") && !isEscaping)
            {
                Debug.Log("Cop caught the robber!");

                // Get MetaGameController to stop cop (shared method)
                MetaGameController metaController = FindObjectOfType<MetaGameController>();
                if (metaController != null)
                {
                    metaController.StopCopMovement(other.gameObject); // Call the shared method
                    metaController.GameLost("Caught by police!");
                }
            }

            // Get-away car collision
            else if (other.gameObject.layer == LayerMask.NameToLayer("GetawayCar") && !isEscaping)
            {
                Debug.Log("Robber reached the escape vehicle!");

                // Set escaping flag immediately to prevent cop catches
                isEscaping = true;

                EscapeVehicle vehicle = other.GetComponent<EscapeVehicle>();
                if (vehicle != null)
                {
                    vehicle.StartEscapeSequence();
                }
            }
        }

        

        // Helper method to check if animator has a parameter
        bool HasAnimatorParameter(Animator animator, string paramName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                    return true;
            }
            return false;
        }

        protected override void PerformMovement(Vector2 move, bool yMovement)
        {
            // Store original position to detect if we hit something
            Vector2 originalPosition = body.position;

            // Call the base movement (existing collision detection)
            base.PerformMovement(move, yMovement);

            // Check if we actually moved less than expected (hit something)
            Vector2 actualMove = body.position - originalPosition;
            float expectedDistance = move.magnitude;
            float actualDistance = actualMove.magnitude;

            // If we moved significantly less than expected, we hit something
            if (expectedDistance > 0.001f && actualDistance < expectedDistance * 0.8f)
            {
                // Check what we hit using the same casting system
                CheckForObstacleHit(move);
            }
        }


        void CheckForObstacleHit(Vector2 move)
        {
            // Check if enough time has passed since last damage
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return; // Still in cooldown period, ignore damage
            }

            // Use the same collision detection as the base class
            var count = body.Cast(move, contactFilter, hitBuffer, move.magnitude + 0.01f);

            for (var i = 0; i < count; i++)
            {
                var hit = hitBuffer[i];

                // Check if we hit an obstacle layer
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
                {
                    Debug.Log("Hit obstacle: " + hit.collider.gameObject.name);

                    // Damage the player if we have energy system
                    if (robberEnergy!= null)
                    {
                        robberEnergy.DecreaseEnergy(15f); // Or whatever damage amount
                        lastDamageTime = Time.time; // Record when damage was taken

                        // Optional: Play damage sound
                        if (audioSource != null && ouchAudio != null)
                        {
                            audioSource.PlayOneShot(ouchAudio);
                        }
                    }

                    break; // Only take damage once per collision
                }
            }
        }


        protected override void FixedUpdate()
        {
            // Debug.Log($"[Pre-FixedUpdate] position={transform.position}, velocity.y={velocity.y:F2}, jumpState={jumpState}, IsGrounded={IsGrounded}");

            UpdateJumpState();
            ComputeVelocity();

            // AFTER applying ComputeVelocity but before base.FixedUpdate
            // Debug.Log($"[Post-ComputeVelocity] targetVelocity.y={targetVelocity.y:F2}, velocity.y={velocity.y:F2}, jumpState={jumpState}, IsGrounded={IsGrounded}");

            base.FixedUpdate();
        }

        void UpdateJumpState()
        {
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jumpInput = true;
                    stopJump = false;
                    break;

                case JumpState.Jumping:
                    // wait at least minJumpAirTime before clearing grounded
                    if (Time.time - jumpStartTime >= minJumpAirTime && !IsGrounded)
                    {
                        jumpState = JumpState.InFlight;
                    }
                    break;

                case JumpState.InFlight:
                    if (Time.time - jumpStartTime >= minJumpAirTime && IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }
                    break;

                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            // apply jump impulse
            if (jumpInput && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jumpInput = false;
                audioSource.PlayOneShot(jumpAudio);

            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y *= jumpDeceleration;
                }
            }

            // apply horizontal and vertical to animator
            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
            if (animator != null && animator.parameters.Any(p => p.name == "jumpState" && p.type == AnimatorControllerParameterType.Int))
            {
                animator.SetInteger("jumpState", (int)jumpState);
                animator.SetBool("grounded", IsGrounded);
            }


            // set target velocity for KinematicObject
            targetVelocity = new Vector2(move.x * maxSpeed, velocity.y);
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}

using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customization.
    /// </summary>
    public class PlayerController : KinematicObject
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

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        // Reference to your static sprite
        //public Sprite staticSprite;
        //public Sprite anotherSprite;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            spriteRenderer.color = Color.white; // Remove any tint
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                // if is level 1
                if (SceneManager.GetActiveScene().name == "Level1")
                {
                    // Move left and right based on input
                    move.x = Input.GetAxis("Horizontal");
                    if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                        jumpState = JumpState.PrepareToJump;
                    else if (Input.GetButtonUp("Jump"))
                    {
                        stopJump = true;
                        Schedule<PlayerStopJump>().player = this;
                    }
                }
                else
                {
                    // Always move right (endless runner)
                    float slowDown = 0f;
                    float speedUp = 0f;

                    if (Input.GetKey(KeyCode.A))
                    {
                        slowDown = 0.7f; // How much to slow down
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        speedUp = 0.3f; // How much to speed up (tweak as needed)
                    }

                    // Base speed is 1, minus slowDown, plus speedUp
                    move.x = 1f - slowDown + speedUp;

                    // Clamp to a reasonable range (e.g., 0 to 1.5)
                    move.x = Mathf.Clamp(move.x, 0f, 1.5f);

                    if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                        jumpState = JumpState.PrepareToJump;
                    else if (Input.GetButtonUp("Jump"))
                    {
                        stopJump = true;
                        Schedule<PlayerStopJump>().player = this;
                    }
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }


        //// Call this to show the static sprite
        //public void ShowStaticSprite()
        //{
        //    animator.enabled = false;
        //    spriteRenderer.sprite = staticSprite;
        //}

        //// Call this to change to another sprite
        //public void ShowAnotherSprite()
        //{
        //    spriteRenderer.sprite = anotherSprite;
        //}

        //// Call this to resume animation
        //public void ResumeAnimation()
        //{
        //    animator.enabled = true;
        //}

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
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
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    // Use the exposed jumpDeceleration instead of model.jumpDeceleration
                    velocity.y = velocity.y * jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        //void SetStaticSprite()
        //{
        //    animator.enabled = false; // Disable animator to prevent it from overriding the sprite
        //    spriteRenderer.sprite = staticSprite;
        //}

        //void ChangeToAnotherSprite()
        //{
        //    spriteRenderer.sprite = anotherSprite;
        //}

        //void EnableAnimation()
        //{
        //    animator.enabled = true;
        //}

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
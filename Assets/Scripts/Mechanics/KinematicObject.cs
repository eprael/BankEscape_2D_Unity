using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Implements game physics for some in game entity.
    /// </summary>
    public class KinematicObject : MonoBehaviour
    {
        /// <summary>
        /// The minimum normal (dot product) considered suitable for the entity sit on.
        /// </summary>
        public float minGroundNormalY = .65f;

        /// <summary>
        /// A custom gravity coefficient applied to this entity.
        /// </summary>
        public float gravityModifier = 1f;

        /// <summary>
        /// The current velocity of the entity.
        /// </summary>
        public Vector2 velocity;

        /// <summary>
        /// Is the entity currently sitting on a surface?
        /// </summary>
        /// <value></value>
        public bool IsGrounded { get; private set; }

        protected Vector2 targetVelocity;
        protected Vector2 groundNormal;
        protected Rigidbody2D body;
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = 0.01f;


        /// <summary>
        /// Bounce the object's vertical velocity.
        /// </summary>
        /// <param name="value"></param>
        public void Bounce(float value)
        {
            velocity.y = value;
        }

        /// <summary>
        /// Bounce the objects velocity in a direction.
        /// </summary>
        /// <param name="dir"></param>
        public void Bounce(Vector2 dir)
        {
            velocity.y = dir.y;
            velocity.x = dir.x;
        }

        /// <summary>
        /// Teleport to some position.
        /// </summary>
        /// <param name="position"></param>
        public void Teleport(Vector3 position)
        {
            body.position = position;
            velocity *= 0;
            body.velocity *= 0;
        }

        protected virtual void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            body.isKinematic = true;
        }

        protected virtual void OnDisable()
        {
            body.isKinematic = false;
        }

        protected virtual void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            contactFilter.useLayerMask = true;
        }

        protected virtual void Update()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity();
        }

        protected virtual void ComputeVelocity()
        {

        }

        protected virtual void FixedUpdate()
        {
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            if (velocity.y < 0)
                velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            else
                velocity += Physics2D.gravity * Time.deltaTime;

            velocity.x = targetVelocity.x;

            IsGrounded = false;

            var deltaPosition = velocity * Time.deltaTime;

            var moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);

        }


        // In KinematicObject.cs
        protected virtual void PerformMovement(Vector2 move, bool yMovement)
        {
            // 1) CLEAR GROUNDED FOR THIS PASS
            if (yMovement)
                IsGrounded = false;

            var distance = move.magnitude;
            if (distance > minMoveDistance)
            {
                // Cast in movement direction + shellRadius
                var count = body.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
                for (var i = 0; i < count; i++)
                {
                    var hit = hitBuffer[i];
                    var normal = hit.normal;

                    // Debug.Log($"PerformMovement: move={move}, yMovement={yMovement}, hit#{i} normal={normal}, dist={hit.distance}");

                    // 2) GROUNDING: only when descending into a "flat" surface
                    if (yMovement && move.y < 0 && normal.y > minGroundNormalY)
                    {
                        IsGrounded = true;
                        groundNormal = normal;
                        normal.x = 0;
                    }

                    if (IsGrounded)
                    {
                        // Slide along slopes
                        var proj = Vector2.Dot(velocity, normal);
                        if (proj < 0) velocity -= proj * normal;
                    }
                    else
                    {
                        // AIRBORNE HIT: separate vertical vs. horizontal
                        if (yMovement)
                        {
                            // ceiling/floor on the vertical pass: kill upward y only
                            velocity.y = Mathf.Min(velocity.y, 0);
                        }
                        else
                        {
                            // wall on the horizontal pass: kill horizontal x only
                            velocity.x = 0;
                        }
                    }

                    // 3) back off by shellRadius so we don't stick
                    distance = Mathf.Min(distance, hit.distance - shellRadius);
                }
            }

            // Finally move the body
            body.position += move.normalized * distance;
        }

    }
}
using UnityEngine;
using Platformer.Mechanics;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;

public class CopController : KinematicObject
{
    [Header("Follow Settings")]
    public float followDistance = 2f; // Not used if not referencing player

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f; // Easy mode speed
    public float hardMoveSpeed = 7f; // Hard mode speed
    public float moveSpeed = 7f; // Set this to match the player's maxSpeed

    SpriteRenderer spriteRenderer;
    internal Animator animator;

    void Awake()
    {
        // get refrences to components
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Set Rigidbody2D interpolation to Interpolate for smooth movement
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }


    protected override void ComputeVelocity()
    {
        // Always move right at a constant speed
        targetVelocity = new Vector2(moveSpeed, velocity.y);

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player
        var robberController = collision.gameObject.GetComponent<RobberController>();
        if (robberController != null)
        {
            // Bot touched the player, schedule player death
            Schedule<PlayerDeath>();
        }
    }
}
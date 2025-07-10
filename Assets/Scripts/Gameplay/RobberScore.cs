using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player collides with a token.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>

    public class RobberScore : MonoBehaviour
    {
        [Header("Energy Settings")]
        public float currentScore = 0f;

        [Header("UI Reference")]
        public TextMeshProUGUI scoreText;


        public float cashGain = 15f; // Energy lost when hitting obstacles

        void Start()
        {
            // Initialize energy to full
            currentScore = 0f;

            UpdateScore();
        }

        void Update()
        {
        }



        // Detect collision with bling
        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision detected with: " + collision.gameObject.name + " on layer: " + collision.gameObject.layer);

            // Check if we hit an obstacle
            if (collision.gameObject.layer == LayerMask.NameToLayer("Loot"))
            {
                Debug.Log($"Player hit loot! Gained ${cashGain}");

                IncreaseScore(cashGain);
            }
        }


        // This gets called whenever inspector values change
        void OnValidate()
        {
            // Only update if we're in play mode and have references
            if (Application.isPlaying && scoreText != null)
            {
                UpdateScore();
            }
        }

        public void UpdateScore()
        {
            if (scoreText != null)
            {
                // Update the score text with the current score
                // format with no decimal places
                scoreText.text = $"${currentScore:0}"; // Format as currency with no decimal places
            }
        }


        // Test methods
        public void IncreaseScore(float amount)
        {
            currentScore += amount;
            UpdateScore();
        }
    }
}
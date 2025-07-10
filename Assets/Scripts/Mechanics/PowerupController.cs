using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class controls the behavior of powerups in the game. 
    /// It allows you to set different sprites for each powerup type, 
    /// manage energy values, and play different sounds when a powerup is collected.
    /// </summary>

    public class PowerupController : MonoBehaviour
    {
        [Header("Powerup Settings")]
        public Sprite[] powerupSprites; // Drag your powerup images here
        [Range(0, 10)]
        public int selectedSpriteIndex = 0; 
        public int energyValue = 25; // How much energy this gives


        [Header("Audio")]
        public AudioClip[] collectSounds; // Different sounds per powerup type
        public AudioClip defaultCollectSound; // Fallback sound
        [Range(0f, 1f)]
        public float volume = 0.7f;


        [Header("Optional: Different Values per Sprite")]
        public bool useVariableValues = false;
        public int[] energyValues; // If you want different sprites to give different amounts

        private SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSprite();
        }

        void UpdateSprite()
        {
            if (powerupSprites.Length > 0 && selectedSpriteIndex < powerupSprites.Length)
            {
                spriteRenderer.sprite = powerupSprites[selectedSpriteIndex];

                if (useVariableValues && energyValues.Length > selectedSpriteIndex)
                {
                    energyValue = energyValues[selectedSpriteIndex];
                }
            }
        }

        void OnValidate()
        {
            if (powerupSprites.Length > 0)
            {
                selectedSpriteIndex = Mathf.Clamp(selectedSpriteIndex, 0, powerupSprites.Length - 1);

                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    UpdateSprite();
                }
            }
        }

        public void PlayCollectSound()
        {
            AudioClip soundToPlay = null;

            // Try to get specific sound for this sprite
            if (collectSounds.Length > selectedSpriteIndex && collectSounds[selectedSpriteIndex] != null)
            {
                soundToPlay = collectSounds[selectedSpriteIndex];
            }
            // Fallback to default sound
            else if (defaultCollectSound != null)
            {
                soundToPlay = defaultCollectSound;
            }

            // Play the sound at this position (continues even after object is destroyed)
            if (soundToPlay != null)
            {
                AudioSource.PlayClipAtPoint(soundToPlay, transform.position, volume);
            }
        }

        public int GetEnergyValue()
        {
            return energyValue;
        }

    }
}

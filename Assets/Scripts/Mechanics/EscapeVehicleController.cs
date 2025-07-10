using Platformer.Gameplay;
using Platformer.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{

    public class EscapeVehicle : MonoBehaviour
    {
        [Header("Vehicle Configuration")]
        public EscapeVehicleData vehicleData; // Drag the ScriptableObject here

        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            // Set initial sprite
            if (vehicleData != null && vehicleData.emptyVehicleSprite != null)
            {
                spriteRenderer.sprite = vehicleData.emptyVehicleSprite;
            }
        }

        public void StartEscapeSequence()
        {
            if (vehicleData != null)
            {
                MetaGameController metaController = FindObjectOfType<MetaGameController>();
                if (metaController != null)
                {
                    metaController.StartEscapeCutscene(this);
                }
            }
        }

        public void ShowRobberInVehicle()
        {
            if (vehicleData?.vehicleWithRobberSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = vehicleData.vehicleWithRobberSprite;
            }
        }

        public void PlayStartSound()
        {
            if (vehicleData?.startSound != null && audioSource != null)
            {
                audioSource.volume = vehicleData.startSoundVolume;
                audioSource.PlayOneShot(vehicleData.startSound);
            }
        }
    }
}
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class plays background sounds in the game unrelated to any events or triggers. 
    /// eg. The police dispatch sounds, some guy yelling in the street, etc 
    /// </summary> 

    public class inGameAmbientSoundsController : MonoBehaviour
    {
        [Header("First Ambient Sounds")]
        public AudioClip[] firstAmbientSounds; // Collection of ambient sounds (played in sequence)

        [Header("Gameplay Ambient Sounds")]
        public AudioClip[] inGameAmbientSounds; // Collection of ambient sounds (played in sequence)

        [Header("Timing Settings")]
        public float firstSoundDelay = 3f; // Delay before first sound (seconds)
        public float minInterval = 15f; // Minimum time between sounds (seconds)
        public float maxInterval = 45f; // Maximum time between sounds (seconds)
        [Range(0f, 1f)]
        public float volume = 0.3f; // Volume for ambient sounds
        public bool autoStart = true; // Start playing automatically when game starts

        private AudioSource audioSource;
        private float nextSoundTime;
        private bool isActive = false;
        private int currentSoundIndex = 0; // Track which sound to play next
        private bool isFirstSound = true; // Flag to handle first sound timing

        void Start()
        {
            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Configure AudioSource for ambient sounds
            audioSource.volume = volume;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.loop = false;

            // Start the ambient sound system if autoStart is enabled
            if (autoStart && inGameAmbientSounds.Length > 0)
            {
                StartinGameAmbientSounds();
            }
        }

        void Update()
        {
            // Check if it's time to play the next ambient sound
            if (isActive && Time.time >= nextSoundTime && inGameAmbientSounds.Length > 0)
            {
                PlayNextAmbientSound();
                ScheduleNextSound();
            }
        }

        void PlayNextAmbientSound()
        {
            if (inGameAmbientSounds.Length == 0 || audioSource == null) return;

            AudioClip selectedSound;

            if (isFirstSound)
            {
                int randomIndex = Random.Range(0, firstAmbientSounds.Length);
                selectedSound = firstAmbientSounds[randomIndex];
                isFirstSound = false; // No longer the first sound
                currentSoundIndex = 0; // Reset index for subsequent sounds
            }
            else
            {
                selectedSound = inGameAmbientSounds[currentSoundIndex];
            }
 
            // Play the sound
            audioSource.PlayOneShot(selectedSound, volume);

            Debug.Log($"Playing ambient sound #{currentSoundIndex}: {selectedSound.name}");

            // Move to next sound (cycle back to 0 when reaching the end)
            currentSoundIndex = (currentSoundIndex + 1) % inGameAmbientSounds.Length;
        }


        void ScheduleNextSound()
        {
            float nextInterval;

            if (isFirstSound)
            {
                // Use the short delay for the first sound
                nextInterval = firstSoundDelay;
            }
            else
            {
                // Use random interval for subsequent sounds
                nextInterval = Random.Range(minInterval, maxInterval);
            }

            nextSoundTime = Time.time + nextInterval;

            Debug.Log($"Next ambient sound scheduled in {nextInterval:F1} seconds");
        }

        // Control methods
        public void StartinGameAmbientSounds()
        {
            if (inGameAmbientSounds.Length > 0)
            {
                isActive = true;
                currentSoundIndex = 0; // Start with first sound
                isFirstSound = true; // Reset first sound flag
                ScheduleNextSound();
                Debug.Log("Ambient sounds started - first sound will play soon");
            }
        }

        public void StopinGameAmbientSounds()
        {
            isActive = false;
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            Debug.Log("Ambient sounds stopped");
        }

        public void PlayImmediateSound()
        {
            PlayNextAmbientSound();
            ScheduleNextSound();
        }

        // Optional: Method to change volume at runtime
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        // Optional: Reset to first sound
        public void ResetToFirstSound()
        {
            currentSoundIndex = 0;
            isFirstSound = true;
        }
    }
}
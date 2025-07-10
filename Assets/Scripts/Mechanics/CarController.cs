using UnityEngine;
using Platformer.Mechanics;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using System.Collections;


namespace Platformer.Mechanics
{
    /// <summary>
    /// This class is attached to all the vehicles and manages car movement and sound
    /// For movement it manages speed & direction.  If the direction is set to right, it flips the car sprite to face right
    /// for Sound it plays a random sound from a collection of pre-assigned sounds for that car type.
    /// Trucks will have a diesel engine, ambulance a siren, the hot rod a rumbling engine, and so on.
    /// </summary> 

    public class CarController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 5f;
        public float yOffset = 0f; // Individual Y position offset for this car
        public float destroyOffset = 25f; // How far off-screen before destroying (increased for longer audio)
        [HideInInspector]
        public bool goingRight = false; // Set by spawner

        [Header("Audio")]
        public AudioClip[] specialSounds; // Multiple sounds - controller picks one randomly
        [Range(0f, 1f)]
        public float soundVolume = 0.7f; // Volume for this car's special sound
        public bool loopSound = false; // Whether to loop the special sound
        public float audioFadeTime = 2f; // Time to fade out audio before destruction

        private AudioSource audioSource;
        private AudioClip currentSound; // Stores the randomly selected sound
        private bool isFadingOut = false;
        private float originalVolume;

        void Start()
        {
            // Set up AudioSource for this car's special sound
            if (specialSounds.Length > 0)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                // Configure AudioSource
                audioSource.volume = soundVolume;
                originalVolume = soundVolume; // Store original volume for fade calculations
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.loop = loopSound; // Set loop based on inspector flag

                // Play the special sound immediately
                PlaySpecialSound();
            }
        }

        void Update()
        {
            // Calculate fade distance (when to start fading audio)
            float fadeDistance = destroyOffset - audioFadeTime * speed;

            // Move car based on direction
            if (goingRight)
            {
                transform.Translate(Vector2.right * speed * Time.deltaTime);

                // Start fading audio when approaching fade distance
                float distanceFromCamera = transform.position.x - Camera.main.transform.position.x;
                if (distanceFromCamera > fadeDistance && !isFadingOut && audioSource != null)
                {
                    StartCoroutine(FadeOutAudio());
                }

                // Destroy when too far off-screen (right side)
                if (transform.position.x > Camera.main.transform.position.x + destroyOffset)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                transform.Translate(Vector2.left * speed * Time.deltaTime);

                // Start fading audio when approaching fade distance  
                float distanceFromCamera = Camera.main.transform.position.x - transform.position.x;
                if (distanceFromCamera > fadeDistance && !isFadingOut && audioSource != null)
                {
                    StartCoroutine(FadeOutAudio());
                }

                // Destroy when too far off-screen (left side)
                if (transform.position.x < Camera.main.transform.position.x - destroyOffset)
                {
                    Destroy(gameObject);
                }
            }
        }

        void PlaySpecialSound()
        {
            if (specialSounds.Length == 0 || audioSource == null) return;

            // Randomly select a sound from the array
            int randomIndex = Random.Range(0, specialSounds.Length);
            currentSound = specialSounds[randomIndex];

            if (currentSound != null)
            {
                if (loopSound)
                {
                    // For looping sounds, use clip + Play()
                    audioSource.clip = currentSound;
                    audioSource.Play();
                }
                else
                {
                    // For one-time sounds, use PlayOneShot
                    audioSource.PlayOneShot(currentSound, soundVolume);
                }
            }
        }

        // Coroutine to fade out audio smoothly
        IEnumerator FadeOutAudio()
        {
            if (audioSource == null || !audioSource.isPlaying) yield break;

            isFadingOut = true;
            float fadeStartTime = Time.time;
            float startVolume = audioSource.volume;

            while (Time.time < fadeStartTime + audioFadeTime)
            {
                float elapsedTime = Time.time - fadeStartTime;
                float fadeProgress = elapsedTime / audioFadeTime;

                // Gradually reduce volume from start volume to 0
                audioSource.volume = Mathf.Lerp(startVolume, 0f, fadeProgress);

                yield return null; // Wait for next frame
            }

            // Ensure volume is exactly 0 and stop the audio
            audioSource.volume = 0f;
            audioSource.Stop();
        }

        // Optional: Play special sound at any time (for events, etc.)
        public void TriggerSpecialSound()
        {
            PlaySpecialSound();
        }

        // Stop the special sound (useful for looping sounds)
        public void StopSpecialSound()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            isFadingOut = false; // Reset fade state
        }

        // Control methods for the special sound
        public void PauseSpecialSound()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        public void ResumeSpecialSound()
        {
            if (audioSource != null && currentSound != null)
            {
                audioSource.volume = originalVolume; // Restore original volume
                audioSource.UnPause();
                isFadingOut = false; // Reset fade state
            }
        }
    }
}
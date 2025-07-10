using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player collides with a token.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>


    public class MusicManager : MonoBehaviour
    {
        [Header("Menu Background Music")]
        public AudioClip[] menuMusicTracks; // Background music during main menu
        [Range(0f, 1f)]
        public float menuVolume = 0.5f;

        [Header("Gameplay Background Music")]
        public AudioClip[] gameplayMusicTracks; // Background music during gameplay
        [Range(0f, 1f)]
        public float gameplayVolume = 0.5f;

        [Header("Game Over Background Music")]
        public AudioClip[] gameOverMusicTracks; // Background music for game over screen
        public AudioClip[] gameOverSounds; // One-shot sounds when getting caught
        [Range(0f, 1f)]
        public float gameOverVolume = 0.8f;

        [Header("Victory Background Music")]
        public AudioClip[] victoryMusicTracks; // Background music for victory screen
        public AudioClip[] victorySounds; // One-shot sounds when winning
        [Range(0f, 1f)]
        public float victoryVolume = 0.8f;

        [Header("Audio Sources")]
        public AudioSource backgroundAudioSource; // For looping background music
        public AudioSource effectsAudioSource; // For one-shot sounds

        public static MusicManager Instance;

        private int currentGameplayTrack = -1;
        private int currentGameOverTrack = -1;
        private int currentVictoryTrack = -1;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // Setup audio sources
            if (backgroundAudioSource == null)
                backgroundAudioSource = GetComponent<AudioSource>();

            if (effectsAudioSource == null)
            {
                effectsAudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (backgroundAudioSource != null)
            {
                backgroundAudioSource.loop = true;
            }

            if (effectsAudioSource != null)
            {
                effectsAudioSource.loop = false;
            }
        }

        public void PlayRandomMenuMusic()
        {
            PlayRandomBackgroundMusic(menuMusicTracks, ref currentGameplayTrack, menuVolume);
        }

        public void PlayRandomGameplayMusic()
        {
            PlayRandomBackgroundMusic(gameplayMusicTracks, ref currentGameplayTrack, gameplayVolume);
        }

        public void PlayRandomGameOverMusic()
        {
            PlayRandomBackgroundMusic(gameOverMusicTracks, ref currentGameOverTrack, gameOverVolume);
        }

        public void PlayRandomVictoryMusic()
        {
            PlayRandomBackgroundMusic(victoryMusicTracks, ref currentVictoryTrack, victoryVolume);
        }

        void PlayRandomBackgroundMusic(AudioClip[] tracks, ref int currentTrack, float volume)
        {
            if (tracks.Length == 0 || backgroundAudioSource == null) return;

            // Pick a random track (different from current one if possible)
            int newTrack;
            if (tracks.Length == 1)
            {
                newTrack = 0;
            }
            else
            {
                do
                {
                    newTrack = Random.Range(0, tracks.Length);
                }
                while (newTrack == currentTrack && tracks.Length > 1);
            }

            currentTrack = newTrack;

            // Play the selected track
            backgroundAudioSource.clip = tracks[newTrack];
            backgroundAudioSource.volume = volume;
            backgroundAudioSource.Play();

            Debug.Log($"Playing background music: {tracks[newTrack].name}");
        }

        public void PlayRandomGameOverSound()
        {
            PlayRandomOneShot(gameOverSounds, gameOverVolume);
        }

        public void PlayRandomVictorySound()
        {
            PlayRandomOneShot(victorySounds, victoryVolume);
        }

        void PlayRandomOneShot(AudioClip[] sounds, float volume)
        {
            if (sounds.Length == 0 || effectsAudioSource == null) return;

            int randomIndex = Random.Range(0, sounds.Length);
            AudioClip selectedSound = sounds[randomIndex];

            effectsAudioSource.volume = volume;
            effectsAudioSource.PlayOneShot(selectedSound);

            Debug.Log($"Playing one-shot sound: {selectedSound.name}");
        }

        public void StopBackgroundMusic()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Stop();
            }
        }

        public void PauseBackgroundMusic()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Pause();
            }
        }

        public void ResumeBackgroundMusic()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.clip != null)
            {
                backgroundAudioSource.UnPause();
            }
        }

        // Volume controls
        public void SetGameplayVolume(float volume)
        {
            gameplayVolume = Mathf.Clamp01(volume);
        }

        public void SetGameOverVolume(float volume)
        {
            gameOverVolume = Mathf.Clamp01(volume);
        }

        public void SetVictoryVolume(float volume)
        {
            victoryVolume = Mathf.Clamp01(volume);
        }
    }
}
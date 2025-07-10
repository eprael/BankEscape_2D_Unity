using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;
using UnityEngine.UI;

namespace Platformer.Mechanics
{
    /// <summary>
    /// this class manages the difficulty settings for the game. 
    /// </summary>

    public enum DifficultyLevel
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public class DifficultySettings : MonoBehaviour
    {
        [Header("UI References - Toggle Group Approach")]
        public Toggle easyToggle;
        public Toggle mediumToggle;
        public Toggle hardToggle;

        [Header("Current Difficulty")]
        public static DifficultyLevel currentDifficulty = DifficultyLevel.Easy;

        void Start()
        {
            LoadDifficultySetting();

            // Set up listeners based on UI type
            SetupToggles();
        }

        void SetupToggles()
        {
            if (easyToggle != null) easyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(DifficultyLevel.Easy); });
            if (mediumToggle != null) mediumToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(DifficultyLevel.Medium); });
            if (hardToggle != null) hardToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty(DifficultyLevel.Hard); });
        }

        void LoadDifficultySetting()
        {
            // Load from PlayerPrefs (0 = easy, 1 = medium, 2 = hard)
            int savedDifficulty = PlayerPrefs.GetInt("GameDifficulty", 0);
            currentDifficulty = (DifficultyLevel)savedDifficulty;

            // Update UI to match saved setting
            UpdateUIToMatchDifficulty();

            Debug.Log($"Loaded difficulty: {currentDifficulty}");
        }

        void UpdateUIToMatchDifficulty()
        {
            // Update toggles
            if (easyToggle != null) easyToggle.isOn = (currentDifficulty == DifficultyLevel.Easy);
            if (mediumToggle != null) mediumToggle.isOn = (currentDifficulty == DifficultyLevel.Medium);
            if (hardToggle != null) hardToggle.isOn = (currentDifficulty == DifficultyLevel.Hard);
        }


        void SetDifficulty(DifficultyLevel difficulty)
        {
            currentDifficulty = difficulty;

            // Save to PlayerPrefs
            PlayerPrefs.SetInt("GameDifficulty", (int)difficulty);
            PlayerPrefs.Save();

            Debug.Log($"Difficulty set to: {difficulty}");
        }

        // Public static methods for other scripts to use
        public static DifficultyLevel GetDifficulty()
        {
            return currentDifficulty;
        }

        public static bool IsEasy()
        {
            return currentDifficulty == DifficultyLevel.Easy;
        }

        public static bool IsMedium()
        {
            return currentDifficulty == DifficultyLevel.Medium;
        }

        public static bool IsHard()
        {
            return currentDifficulty == DifficultyLevel.Hard;
        }
    }
}
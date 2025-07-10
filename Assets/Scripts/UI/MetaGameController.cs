using Platformer.Gameplay;
using Platformer.Mechanics;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Platformer.UI
{
    public enum ScreenType
    {
        Title,
        MainMenu,
        Gameplay,
        GameOver,
        LevelComplete
    }

    [System.Serializable]
    public class UIScreen
    {
        public ScreenType screenType;
        public GameObject screenObject;
        public bool pauseGame = false; // Whether this screen should pause the game
    }

    [System.Serializable]
    public enum TitleMediaType
    {
        Video,
        Image
    }

    public class MetaGameController : MonoBehaviour
    {

        // Static flag to control startup behavior
        public static bool skipToGameplay = false;


        [Header("UI Screens")]
        public UIScreen[] screens; // Array of screen configurations

        [Header("Legacy References (keep for compatibility)")]

        public GameController gameController;


        [Header("Intro")]
        public VideoController videoController;


        [Header("Title Screen Media")]
        public TitleMediaType titleMediaType = TitleMediaType.Video;
        public VideoClip titleVideoClip; // Drag video file here
        public Sprite titleImageSprite; // Drag image file here
        public float titleImageDuration = 5f; // How long to show image before allowing proceed
        private float titleImageStartTime;
        private bool titleImageTimerStarted = false;
        private bool musicStartedAtTitle = false; // Whether we previously showed an image

        private float titleImageTimer; // Separate timer that counts down
        private GameObject titleImageObject;

        [Header("Gameplay UI")]
        public GameObject[] gamePlayCanvasii; // Any other gameplay UI elements

        [Header("Menu Navigation")]
        public MenuNavigation menuNavigation;

        [Header("Game End Conditions")]
        public RobberEnergy robberEnergy; // robberEnergy component here

        [Header("Game Over Messaging")]
        public string lastGameOverReason = "";
        public TMPro.TextMeshProUGUI gameOverReasonText; 
                                                         



        private GameState currentState = GameState.Title;
        private bool canProceedFromTitle = false;

        void Start()
        {
            // Check if we should skip directly to gameplay
            if (skipToGameplay)
            {
                Debug.Log("Skipping title screen - going directly to gameplay");
                skipToGameplay = false; // Reset flag
                StartGame();
            }
            else // if (SceneManager.GetActiveScene().name == "Level1")
            {
                ShowTitleScreen();
            }
        }

        // For game over screen - skip directly to gameplay
        public void RestartFromGameOver()
        {
            Debug.Log("Restarting from game over - skip to gameplay");
            skipToGameplay = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // For main menu - go through normal flow
        public void StartNewGame()
        {
            Debug.Log("Starting new game - normal flow");
            skipToGameplay = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // For testing - immediate gameplay
        public void QuickStartGameplay()
        {
            Debug.Log("Quick start to gameplay");
            skipToGameplay = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void Update()
        {
            switch (currentState)
            {
                case GameState.Title:
                    // Handle image timer for title screen
                    if (titleMediaType == TitleMediaType.Image && titleImageTimerStarted)
                    {
                        float currentTime = Time.realtimeSinceStartup;
                        float elapsedTime = currentTime - titleImageStartTime;
                        float remainingTime = titleImageDuration - elapsedTime;

                        // Log every half second
                        if (Mathf.FloorToInt(elapsedTime * 2) != Mathf.FloorToInt((elapsedTime - Time.unscaledDeltaTime) * 2))
                        {
                            Debug.Log($"Title timer: {remainingTime:F1} seconds remaining (elapsed: {elapsedTime:F1})");
                        }

                        if (elapsedTime >= titleImageDuration)
                        {
                            Debug.Log("Real time elapsed - calling OnTitleImageFinished");
                            OnTitleImageFinished();
                        }
                    }

                    if (canProceedFromTitle)
                    {
                        // Debug.Log("canProceedFromTitle is true - calling ShowMenu");
                        ShowMenu();
                    }
                    break;
                case GameState.Menu:
                    // Handle menu navigation here, e.g., start game button calls StartGame()
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        StartGame();
                    }
                    break;
                case GameState.Gameplay:
                    // DEBUG: Add logging to see what's happening
                    if (robberEnergy != null && robberEnergy.currentEnergy <= 0)
                    {
                        GameLost("Lost All Health!");
                    }

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ShowMenu();
                    }
                    break;
                case GameState.EndGame:
                    // Wait for input to return to menu or quit
                    break;
            }
        }

        void OnTitleImageFinished()
        {
            Debug.Log("=== OnTitleImageFinished called ===");
            canProceedFromTitle = true;
            titleImageTimerStarted = false;

            // Hide the image
            if (titleImageObject != null)
            {
                titleImageObject.SetActive(false);
            }

            Debug.Log("Title image timer finished - can proceed to menu");
        }


        // Smart screen management
        public void ShowScreen(ScreenType targetScreen)
        {
            Debug.Log("ShowScreen called for: " + targetScreen);

            // Turn off ALL screens first
            foreach (var screen in screens)
            {
                if (screen.screenObject != null)
                {
                    screen.screenObject.SetActive(false);
                }
            }

            UIScreen activeScreen = System.Array.Find(screens, s => s.screenType == targetScreen);

            if (activeScreen != null)
            {
                Time.timeScale = activeScreen.pauseGame ? 0f : 1f;
                UpdateGameState(targetScreen);

                // RE-ENABLED: Show/hide gameplay UI based on screen type
                SetGameplayUIActive(targetScreen == ScreenType.Gameplay);

                if (activeScreen.screenObject != null)
                {
                    activeScreen.screenObject.SetActive(true);
                }
            }
        }

        void UpdateGameState(ScreenType screenType)
        {
            switch (screenType)
            {
                case ScreenType.Title:
                    currentState = GameState.Title;
                    break;
                case ScreenType.MainMenu:
                    currentState = GameState.Menu;
                    break;
                case ScreenType.Gameplay:
                    currentState = GameState.Gameplay;
                    break;
                case ScreenType.GameOver:
                case ScreenType.LevelComplete:
                    currentState = GameState.EndGame;
                    break;
            }
        }

        public void ShowTitleScreen()
        {
            HideGameplayUI(); // Ensure gameplay UI is hidden
            ShowScreen(ScreenType.Title);
            canProceedFromTitle = false;
            titleImageTimerStarted = false;

            switch (titleMediaType)
            {
                case TitleMediaType.Video:
                    ShowTitleVideo();
                    break;
                case TitleMediaType.Image:
                    if (MusicManager.Instance != null)
                    {
                        musicStartedAtTitle = true;
                        MusicManager.Instance.PlayRandomMenuMusic(); // Play menu background music
                    }

                    ShowTitleImage();
                    break;
            }
        }

        void ShowTitleImage()
        {
            if (titleImageSprite != null)
            {
                CreateTitleImageDisplay();

                // Use real time instead of deltaTime accumulation
                titleImageStartTime = Time.realtimeSinceStartup;
                titleImageTimerStarted = true;

                Debug.Log($"Started title image at real time: {titleImageStartTime:F2}");
                Debug.Log($"Will finish at: {titleImageStartTime + titleImageDuration:F2}");
            }
            else
            {
                canProceedFromTitle = true;
            }
        }

        void CreateTitleImageDisplay()
        {
            // Find or create an Image component to display the sprite
            // This assumes you have an Image component on your title screen for this purpose
            GameObject titleScreen = System.Array.Find(screens, s => s.screenType == ScreenType.Title)?.screenObject;

            if (titleScreen != null)
            {
                // Look for existing image component
                UnityEngine.UI.Image titleImage = titleScreen.GetComponentInChildren<UnityEngine.UI.Image>();

                if (titleImage != null)
                {
                    titleImage.sprite = titleImageSprite;
                    titleImage.gameObject.SetActive(true);
                    titleImageObject = titleImage.gameObject;

                    // Start fade-in effect
                    // StartCoroutine(FadeInTitleImage(titleImage));
                }
                else
                {
                    Debug.Log("No Image component found in title screen for displaying sprite");
                }
            }
        }

        IEnumerator FadeInTitleImage(UnityEngine.UI.Image image, float duration = 1f)
        {
            // Start with transparent
            Color imageColor = image.color;
            imageColor.a = 0f;
            image.color = imageColor;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / duration);

                imageColor.a = alpha;
                image.color = imageColor;

                yield return null;
            }

            // Ensure final alpha is exactly 1
            imageColor.a = 1f;
            image.color = imageColor;
        }

        void ShowTitleVideo()
        {
            if (videoController != null && titleVideoClip != null)
            {
                videoController.PlayVideo(titleVideoClip.name);
                // Subscribe to video end event
                if (videoController.videoPlayer != null)
                {
                    videoController.videoPlayer.loopPointReached += OnTitleVideoFinished;
                }
                Debug.Log($"Playing title video: {titleVideoClip.name}");
            }
            else if (titleVideoClip == null)
            {
                Debug.Log("No title video clip assigned - allowing immediate proceed");
                canProceedFromTitle = true;
            }
        }



        void OnTitleVideoFinished(VideoPlayer vp)
        {
            canProceedFromTitle = true;
            // Optionally, show a "Press any key to continue" message here
        }

        public void ShowMenu()
        {
            HideGameplayUI(); // Hide any gameplay UI elements
            if (musicStartedAtTitle)
            {
                musicStartedAtTitle = false; // Reset flag
            }
            else
            {
                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.PlayRandomMenuMusic(); // Play menu background music
                }
            }
            ShowScreen(ScreenType.MainMenu);
            menuNavigation.ShowMainPanel(); // Show the main menu panel
        }

        public void StartGame()
        {
            ApplyDifficultySettings();

            ShowScreen(ScreenType.Gameplay);


            // Start gameplay background music
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayRandomGameplayMusic();
            }

            if (videoController != null)
                videoController.StopVideo();
        }

 
        void HideGameplayUI()
        {
            foreach (var uiElement in gamePlayCanvasii)
            {
                if (uiElement != null)
                {
                    //uiElement.gameObject.SetActive(false);
                    uiElement.SetActive(false);
                }
            }
        }


        private void SetGameplayUIActive(bool active)
        {
            Debug.Log($"SetGameplayUIActive called with: {active}");
            if (gamePlayCanvasii == null || gamePlayCanvasii.Length == 0)
            {
                Debug.LogWarning("gamePlayCanvasii array is null or empty!");
                return;
            }
            Debug.Log($"gamePlayCanvasii array length: {gamePlayCanvasii.Length}");

            foreach (var canvas in gamePlayCanvasii)
            {
                if (canvas != null)
                {
                    Debug.Log($"Setting {canvas.name} Type: {canvas.GetType().Name}, active = {active} (was {canvas.gameObject.activeInHierarchy})");
                    canvas.gameObject.SetActive(active);
                }
                else
                {
                    Debug.Log("Found null canvas in gamePlayCanvasii array");
                }
            }
        }


        void ResetGameState()
        {
            Debug.Log("=== Resetting game state ===");

            // Reset player energy
            if (robberEnergy != null)
            {
                robberEnergy.currentEnergy = robberEnergy.maxEnergy;
                robberEnergy.stopDraining = false;
                robberEnergy.UpdateEnergyBar();
                robberEnergy.ShowEnergyPanel(); // Make sure panel is visible
                Debug.Log("Player energy reset to full");
            }

            // Reset player state
            RobberController player = FindObjectOfType<RobberController>();
            if (player != null)
            {
                player.controlEnabled = true;
                player.isEscaping = false;

                // Show player sprite if it was hidden
                SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
                if (playerSprite != null)
                {
                    playerSprite.enabled = true;
                }

                Debug.Log("Player state reset");
            }

            // Reset cop state - re-enable movement and animation
            ResetAllCops();

            // Reset any other game state as needed
            Debug.Log("Game state reset complete");
        }

        void ResetAllCops()
        {
            // Find all cops and reset their state
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == LayerMask.NameToLayer("AI"))
                {
                    // Re-enable cop animator
                    Animator copAnimator = obj.GetComponent<Animator>();
                    if (copAnimator != null)
                    {
                        copAnimator.enabled = true;
                    }

                    // Re-enable cop collider if it was disabled
                    Collider2D copCollider = obj.GetComponent<Collider2D>();
                    if (copCollider != null)
                    {
                        copCollider.enabled = true;
                    }

                    Debug.Log($"Reset cop: {obj.name}");
                }
            }
        }

        void StopEnergyDepletion()
        {
            RobberEnergy robberEnergy = FindObjectOfType<RobberEnergy>();
            if (robberEnergy != null)
            {
                robberEnergy.stopDraining = true;
                Debug.Log("Energy depletion stopped");

                // robberEnergy.HideEnergyPanel(); // Hide the energy panel

                // Directly access the slider and change its fill color
                UnityEngine.UI.Slider energySlider = robberEnergy.energyBar;
                if (energySlider != null && energySlider.fillRect != null)
                {
                    UnityEngine.UI.Image fillImage = energySlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = Color.clear;
                        Debug.Log("Energy bar color changed to gray");
                    }
                    else
                    {
                        Debug.Log("Could not find fill Image component");
                    }
                }
                else
                {
                    Debug.Log("Energy slider or fillRect is null");
                }
            }
            else
            {
                Debug.Log("robberEnergy component not found");
            }
        }

        // Game end methods
        public void GameLost(string reason)
        {
            if (currentState != GameState.Gameplay) return;

            Debug.Log("GAME LOST: " + reason);

            // Store and display the reason
            SetGameOverReason(reason);

            // Stop player movement
            RobberController player = FindObjectOfType<RobberController>();
            if (player != null)
            {
                player.controlEnabled = false;
            }

            StopEnergyDepletion();

            StopAllCops();

            // Play game over sound
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayRandomGameOverSound();
                MusicManager.Instance.PlayRandomGameOverMusic();
            }

            ShowScreen(ScreenType.GameOver);
        }

        void StopAllCops()
        {
            // Find all GameObjects on AI layer
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == LayerMask.NameToLayer("AI"))
                {
                    StopCopMovement(obj);
                }
            }
        }

        //void StopCopMovement(GameObject cop)
        //{
        //    // Stop the animator (stops running animation)
        //    Animator copAnimator = cop.GetComponent<Animator>();
        //    if (copAnimator != null)
        //    {
        //        copAnimator.enabled = false;
        //        Debug.Log($"Stopped cop animation: {cop.name}");
        //    }

        //    // Stop rigidbody velocity to prevent sliding
        //    Rigidbody2D copRigidbody = cop.GetComponent<Rigidbody2D>();
        //    if (copRigidbody != null)
        //    {
        //        copRigidbody.velocity = Vector2.zero;
        //        Debug.Log($"Stopped cop movement: {cop.name}");
        //    }
        //}

        public void StopCopMovement(GameObject cop)
        {
            // Method 1: Try to find a public movement control field
            MonoBehaviour[] scripts = cop.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name.Contains("Cop"))
                {
                    // Look for common movement control fields using reflection
                    var fields = script.GetType().GetFields();
                    foreach (var field in fields)
                    {
                        // Try to find and disable movement-related fields
                        if (field.Name.ToLower().Contains("speed") && field.FieldType == typeof(float))
                        {
                            field.SetValue(script, 0f);
                            Debug.Log($"Set {field.Name} to 0 in {script.GetType().Name}");
                        }
                    }
                }
            }

            // Method 2: Set Rigidbody2D to kinematic if it isn't already
            Rigidbody2D copRigidbody = cop.GetComponent<Rigidbody2D>();
            if (copRigidbody != null && !copRigidbody.isKinematic)
            {
                copRigidbody.velocity = Vector2.zero;
                copRigidbody.isKinematic = true; // Prevent gravity from affecting it
            }

            // stop the animator if it exists
            Animator copAnimator = cop.GetComponent<Animator>();
            if (copAnimator != null)
            {
                copAnimator.enabled = false;
                Debug.Log("Cop animator disabled - stopped running animation");
            }
        }

        public void GameWon(string reason)
        {
            if (currentState != GameState.Gameplay) return; // Only win from gameplay

            Debug.Log("GAME WON: " + reason);

            // Simple victory - cutscene is handled by EscapeVehicle if needed
            // Stop player movement  
            RobberController player = FindObjectOfType<RobberController>();
            if (player != null)
            {
                player.controlEnabled = false;
            }

            // Play victory sound
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayRandomVictorySound();
            }

            ShowScreen(ScreenType.LevelComplete);
        }

        // NEW: Data-driven escape cutscene system
        public void StartEscapeCutscene(EscapeVehicle vehicle)
        {
            StartCoroutine(EscapeCutsceneSequence(vehicle));
        }

        System.Collections.IEnumerator EscapeCutsceneSequence(EscapeVehicle vehicle)
        {
            if (vehicle.vehicleData == null) yield break;

            var data = vehicle.vehicleData;
            Debug.Log($"Starting escape cutscene with {data.vehicleName}");

            // 1. Hide player and setup vehicle
            RobberController player = FindObjectOfType<RobberController>();
            if (player != null)
            {
                player.controlEnabled = false;

                // Hide the player sprite
                SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
                if (playerSprite != null)
                {
                    playerSprite.enabled = false;
                }
            }

            // 2. Stop all cops immediately when escape begins
            Debug.Log("Stopping energy depletion...");
            StopEnergyDepletion(); // Use our existing method

            // 2. Stop all cops immediately when escape begins
            Debug.Log("Stopping all cops for escape sequence...");
            StopAllCops(); // Use our existing method

            vehicle.ShowRobberInVehicle();
            vehicle.PlayStartSound();

            // 3. Wait for start delay
            yield return new WaitForSeconds(data.startDelay);

            Debug.Log("Vehicle starting to move...");

            // 4. Accelerating movement
            float elapsedTime = 0f;
            while (elapsedTime < data.accelerationTime)
            {
                float progress = elapsedTime / data.accelerationTime;
                float speedMultiplier = Mathf.SmoothStep(0f, 1f, progress);
                float currentSpeed = data.maxSpeed * speedMultiplier;

                vehicle.transform.Translate(data.escapeDirection * currentSpeed * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 4. Full speed until duration ends
            float remainingTime = data.totalDuration - data.startDelay - data.accelerationTime;
            if (remainingTime > 0)
            {
                elapsedTime = 0f;
                while (elapsedTime < remainingTime)
                {
                    vehicle.transform.Translate(data.escapeDirection * data.maxSpeed * Time.deltaTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            Debug.Log("Escape cutscene complete - showing victory screen");



            // 5. Show victory (no music here since vehicle might have played its own sounds)
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayRandomVictorySound(); // One-shot sound first
                MusicManager.Instance.PlayRandomVictoryMusic(); // Then background music
            }
            ShowScreen(ScreenType.LevelComplete);
        }

        (float, float) GetCarSpawnTimesForDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy: return (3f, 6f);
                case DifficultyLevel.Medium: return (3f, 5f);
                case DifficultyLevel.Hard: return (2f, 4f);
                default: return (3f, 6f); // Default to easy settings
            }
        }

        void ApplyDifficultySettings()
        {
            DifficultyLevel difficulty = DifficultySettings.GetDifficulty();

            Debug.Log($"Applying {difficulty} difficulty settings");

            // Get difficulty multipliers
            float copSpeed = GetCopSpeedForDifficulty(difficulty);
            float energyDrainRate = GetEnergyDrainForDifficulty(difficulty);
            float obstacleDamage = GetObstacleDamageForDifficulty(difficulty);
            (float minSpawnTime, float maxSpawnTime) = GetCarSpawnTimesForDifficulty(difficulty);



            // Update cop speed
            CopController cop = FindObjectOfType<CopController>();
            if (cop != null)
            {
                cop.moveSpeed = copSpeed;
                Debug.Log($"Cop speed set to: {copSpeed}");
            }

            // Update robber energy settings
            RobberEnergy robberEnergy = FindObjectOfType<RobberEnergy>();
            if (robberEnergy != null)
            {
                robberEnergy.energyDrainRate = energyDrainRate;
                robberEnergy.obstacleDamage = obstacleDamage;
                Debug.Log($"Energy drain: {energyDrainRate}, Obstacle damage: {obstacleDamage}");
            }

            // Update car spawn settings
            CarSpawner carSpawner = FindObjectOfType<CarSpawner>();
            if (carSpawner != null)
            {
                carSpawner.minSpawnTime = minSpawnTime;
                carSpawner.maxSpawnTime = maxSpawnTime;
            }

        }

        float GetCopSpeedForDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy: return 5f;
                case DifficultyLevel.Medium: return 6f;
                case DifficultyLevel.Hard: return 7f;
                default: return 5f;
            }
        }

        float GetEnergyDrainForDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy: return 1.5f;
                case DifficultyLevel.Medium: return 2.0f;
                case DifficultyLevel.Hard: return 3.0f;
                default: return 2.0f;
            }
        }

        float GetObstacleDamageForDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy: return 10f;
                case DifficultyLevel.Medium: return 15f;
                case DifficultyLevel.Hard: return 25f;
                default: return 15f;
            }
        }

        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        // UI Button methods (call these from your UI buttons)
        public void RestartLevel()
        {
            Debug.Log("Restarting Level");
            skipToGameplay = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMainMenu()
        {
            ShowMenu();
        }

        public void NextLevel()
        {
            // Load next scene or restart for now
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
            Debug.Log("Quit game called");

            HideGameplayUI();
            // ShowTitleScreen();

#if UNITY_EDITOR
            // Stop playing in editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit application in build
        Application.Quit();
#endif
        }

        public void SetGameOverReason(string reason)
        {
            lastGameOverReason = reason;

            // Update the UI text if it exists
            if (gameOverReasonText != null)
            {
                gameOverReasonText.text = reason;
                Debug.Log("Updated game over reason text: " + reason);
            }
        }
    }
}
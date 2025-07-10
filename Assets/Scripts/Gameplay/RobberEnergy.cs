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

    public class RobberEnergy : MonoBehaviour
    {
        [Header("Energy Settings")]
        public float maxEnergy = 100f;
        public float currentEnergy = 100f;
        public float energyDrainRate = 2f; // Energy lost per second over time
        public float obstacleDamage = 15f; // Energy lost when hitting obstacles

        [Header("UI Reference")]
        public Slider energyBar; // EnergyBar slider here
        public GameObject energyPanel; // EnergyPanel here

        [Header("Colors")]
        public Color healthyColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color criticalColor = Color.red;

        [Header("Game State")]
        [HideInInspector]
        public bool stopDraining = false; // Flag to stop energy depletion


        private Image fillImage; // Reference to the slider's fill image

        void Start()
        {
            // Initialize energy to full
            currentEnergy = maxEnergy;

            // Get reference to the slider's fill image
            if (energyBar != null)
            {
                fillImage = energyBar.fillRect.GetComponent<Image>();
            }

            UpdateEnergyBar();
        }

        void Update()
        {
            // Drain energy over time
            DrainEnergyOverTime();
        }

        void DrainEnergyOverTime()
        {
            if (currentEnergy > 0 && !stopDraining)
            {
                currentEnergy -= energyDrainRate * Time.deltaTime;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
                UpdateEnergyBar();
            }
        }

        public void HideEnergyPanel()
        {
            stopDraining = true;

            if (energyPanel != null)
            {
                energyPanel.SetActive(false);
                Debug.Log("Energy panel hidden via direct reference");
            }
            else
            {
                Debug.Log("Energy panel reference is null");
            }
        }

        //public void ShowEnergyPanel()
        //{
        //    stopDraining = false;
        //    if (energyPanel != null)
        //    {
        //        energyPanel.SetActive(true);
        //        Debug.Log("Energy panel shown via direct reference");
        //    }
        //    else
        //    {
        //        Debug.Log("Energy panel reference is null");
        //    }
        //}

        public void ShowEnergyPanel()
        {
            stopDraining = false; // Resume energy draining

            // Find the energy panel and show it
            Transform energyPanel = transform.parent;
            if (energyPanel == null)
            {
                GameObject panel = GameObject.Find("EnergyPanel");
                if (panel != null)
                {
                    energyPanel = panel.transform;
                }
            }

            if (energyPanel != null)
            {
                energyPanel.gameObject.SetActive(true);
                Debug.Log("Energy panel shown");
            }
        }

        // Detect collision with obstacles
        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision detected with: " + collision.gameObject.name + " on layer: " + collision.gameObject.layer);

            // Check if we hit an obstacle
            if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
            {
                Debug.Log("Player hit obstacle! Lost " + obstacleDamage + " energy");
                DecreaseEnergy(obstacleDamage);
            }
        }

        // Also add this to catch ANY collision
        void OnCollisionStay2D(Collision2D collision)
        {
            // Only log once per second to avoid spam
            if (Time.time % 1f < 0.1f)
            {
                Debug.Log("Still colliding with: " + collision.gameObject.name);
            }
        }

        // This gets called whenever inspector values change
        void OnValidate()
        {
            // Only update if we're in play mode and have references
            if (Application.isPlaying && energyBar != null)
            {
                UpdateEnergyBar();
            }
        }

        public void UpdateEnergyBar()
        {
            if (energyBar != null)
            {
                energyBar.value = currentEnergy;
                energyBar.maxValue = maxEnergy;

                // Update color based on energy percentage
                UpdateEnergyBarColor();
            }
        }

        void UpdateEnergyBarColor()
        {
            if (fillImage == null) return;

            float energyPercentage = currentEnergy / maxEnergy;

            if (energyPercentage <= 0.25f) // 25% or below - Red
            {
                fillImage.color = criticalColor;
            }
            else if (energyPercentage <= 0.40f) // 40% or below - Yellow
            {
                fillImage.color = warningColor;
            }
            else // Above 40% - Green
            {
                fillImage.color = healthyColor;
            }
        }

        // Test methods
        public void DecreaseEnergy(float amount)
        {
            currentEnergy -= amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            UpdateEnergyBar();
        }

        public void IncreaseEnergy(float amount)
        {
            currentEnergy += amount;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            UpdateEnergyBar();
        }
    }
}
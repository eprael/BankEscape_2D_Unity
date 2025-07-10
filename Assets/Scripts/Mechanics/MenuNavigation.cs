using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Platformer.Mechanics
{
    /// <summary>
    /// this class controls the navigation between different menu panels 
    /// </summary>

    public class MenuNavigation : MonoBehaviour
    {
        [Header("Navigation Buttons")]
        public Button[] navButtons; // All your nav buttons

        [Header("Button Graphics")]
        public Sprite activeButtonSprite; // Graphic for current panel
        public Sprite inactiveButtonSprite; // Graphic for other panels

        [Header("Button Colors")]
        public Color activeButtonColor = Color.white; // Color for current panel
        public Color inactiveButtonColor = Color.gray; // Color for other panels

        [Header("Text Colors")]
        public Color activeTextColor = new Color(1f, 1f, 1f, 1f); // #FFFFFF - White
        public Color inactiveTextColor = new Color(0.3f, 0.35f, 0.4f, 1f); // #4C5865 - Gray-blue

        [Header("Panel References")]
        public GameObject[] menuPanels; // All your menu panels

        private int currentPanelIndex = 0;

        void Start()
        {
            // Set up button listeners
            for (int i = 0; i < navButtons.Length; i++)
            {
                int panelIndex = i; // Capture for closure
                navButtons[i].onClick.AddListener(() => ShowPanel(panelIndex));
            }

            // Show first panel by default
            ShowPanel(0);
        }

        public void ShowPanel(int panelIndex)
        {
            // Hide all panels
            for (int i = 0; i < menuPanels.Length; i++)
            {
                menuPanels[i].SetActive(i == panelIndex);
            }

            // Update button graphics, colors, and text colors
            UpdateButtonGraphics(panelIndex);

            currentPanelIndex = panelIndex;
            Debug.Log($"Switched to panel: {panelIndex}");
        }

        void UpdateButtonGraphics(int activePanelIndex)
        {
            for (int i = 0; i < navButtons.Length; i++)
            {
                // Update button image
                Image buttonImage = navButtons[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = (i == activePanelIndex) ? activeButtonSprite : inactiveButtonSprite;
                    buttonImage.color = (i == activePanelIndex) ? activeButtonColor : inactiveButtonColor;
                }

                // Update button text color
                UpdateButtonTextColor(navButtons[i], i == activePanelIndex);
            }
        }

        void UpdateButtonTextColor(Button button, bool isActive)
        {
            Color targetColor = isActive ? activeTextColor : inactiveTextColor;

            // Try TextMeshPro first (more common in modern UI)
            TextMeshProUGUI tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.color = targetColor;
                Debug.Log($"Updated TextMeshPro color for {button.name} to {targetColor}");
                return;
            }

            // Fallback to legacy Text component
            Text legacyText = button.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.color = targetColor;
                Debug.Log($"Updated Text color for {button.name} to {targetColor}");
                return;
            }

            Debug.Log($"No text component found on button {button.name}");
        }

        // Public methods for specific panels (if needed)
        public void ShowMainPanel() { ShowPanel(0); }
        public void ShowSettingsPanel() { ShowPanel(1); }
        public void ShowControlsPanel() { ShowPanel(2); }
        public void ShowAboutPanel() { ShowPanel(3); }
    }

}

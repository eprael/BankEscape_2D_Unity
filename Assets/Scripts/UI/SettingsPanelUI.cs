using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this at the top

public class SettingsPanelUI : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text volumeValueText;

    void Start()
    {
        // Initialize slider
        volumeSlider.value = SettingsManager.Instance.settings.volume;
        UpdateVolumeText(volumeSlider.value);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        SettingsManager.Instance.settings.volume = value;
        UpdateVolumeText(value);
        AudioListener.volume = value;
    }

    void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
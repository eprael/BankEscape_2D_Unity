using UnityEngine;

[System.Serializable]
public class GameSettings
{
    public float volume = 1.0f;
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public int difficulty = 1; // 0 = Easy, 1 = Normal, 2 = Hard

    // Add more settings as needed
}
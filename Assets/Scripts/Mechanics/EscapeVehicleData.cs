using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "EscapeVehicleData", menuName = "Game/Escape Vehicle Data")]
    public class EscapeVehicleData : ScriptableObject
    {
        [Header("Vehicle Info")]
        public string vehicleName = "Getaway Car";
        public Sprite emptyVehicleSprite;
        public Sprite vehicleWithRobberSprite;

        [Header("Audio")]
        public AudioClip startSound;
        public float startSoundVolume = 0.8f;

        [Header("Cutscene Timing")]
        public float startDelay = 1f;
        public float accelerationTime = 2f;
        public float maxSpeed = 15f;
        public float totalDuration = 4f;

        [Header("Movement")]
        public Vector3 escapeDirection = Vector3.right; // Could be up for helicopter, etc.
    }
}
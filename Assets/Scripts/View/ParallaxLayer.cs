using UnityEngine;

namespace Platformer.View
{
    /// <summary>
    /// Used to move a transform relative to the main camera position with a scale factor applied.
    /// This is used to implement parallax scrolling effects on different branches of gameobjects.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        /// <summary>
        /// Movement of the layer is scaled by this value.
        /// </summary>
        public Vector3 movementScale = Vector3.one;

        private Transform _camera;
        private Vector3 _startCameraPos;
        private Vector3 _startLayerPos;

        void Awake()
        {
            _camera = Camera.main.transform;
            _startCameraPos = _camera.position;
            _startLayerPos = transform.position;
        }

        void LateUpdate()
        {
            Vector3 cameraDelta = _camera.position - _startCameraPos;
            transform.position = _startLayerPos + Vector3.Scale(cameraDelta, movementScale);
        }
    }
}
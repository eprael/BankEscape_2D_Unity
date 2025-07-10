using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Vector2 parallaxFactor = new Vector2(0.5f, 0f);
    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
    }

    void LateUpdate()
    {
        Vector3 cameraDelta = Camera.main.transform.position - lastCameraPosition;
        transform.position += new Vector3(cameraDelta.x * parallaxFactor.x, cameraDelta.y * parallaxFactor.y, 0);
        lastCameraPosition = Camera.main.transform.position;
    }
}
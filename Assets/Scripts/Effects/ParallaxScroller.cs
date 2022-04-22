using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] float distanceFromCamera;

    const float MAX_DISTANCE = 1000;

    public Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        CounteractCameraMovement();
    }

    private void CounteractCameraMovement()
    {
        Vector2 cameraPosition = mainCamera.transform.position;

        Vector2 offset = countParallaxOffset(cameraPosition);

        transform.position = offset;
    }

    private Vector2 countParallaxOffset(Vector2 cameraPosition)
    {
        float parallaxFactor = (MAX_DISTANCE - distanceFromCamera) / MAX_DISTANCE;

        return new Vector2(cameraPosition.x * parallaxFactor, cameraPosition.y * parallaxFactor);
    }
}

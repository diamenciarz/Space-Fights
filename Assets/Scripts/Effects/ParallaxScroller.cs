using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] float distanceFromCamera;

    const float MAX_DISTANCE = 1000;

    private Camera mainCamera;
    private Vector2 startingPosition;
    private bool isOnScreen;

    private void Start()
    {
        mainCamera = Camera.main;
        startingPosition = transform.position;
    }

    void Update()
    {
        OnScreenCheck();
        CounteractCameraMovement();
    }
    private void OnScreenCheck()
    {
        isOnScreen = CameraInformation.IsPositionOnScreen(transform.position);
    }
    private void CounteractCameraMovement()
    {
        if (!isOnScreen)
        {
            return;
        }
        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2 offset = countParallaxOffset(cameraPosition);

        transform.position = startingPosition + offset;
    }
    private Vector2 countParallaxOffset(Vector2 cameraPosition)
    {
        float parallaxFactor = (MAX_DISTANCE - distanceFromCamera) / MAX_DISTANCE;

        return new Vector2(cameraPosition.x * parallaxFactor, cameraPosition.y * parallaxFactor);
    }

}

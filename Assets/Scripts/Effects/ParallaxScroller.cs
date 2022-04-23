using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] [Range(0, 1000)] float minDistance;
    [SerializeField] [Range(0, 1000)] float maxDistance;

    [Tooltip("If on, the position of this doodad will be recalculated after the camera moves too far from it. Set to off if this is an important landmark")]
    [SerializeField] bool recalculateOffScreen = true;

    private float distanceFromCamera;
    const float MAX_DISTANCE = 1000;
    /// <summary>
    /// After the doodad has moved this many camera sizes away from the middle of the camera and "recalculateOffScreen" is set to true,
    /// the position of this doodad will be recalculated.
    /// If the camera had 10 units of length, a value of 2 would mean that the position will be recalculated after a distance of 2 * 10 from the middle of the camera.
    /// </summary>
    const float RECALCULATE_FACTOR = 1;

    private Camera mainCamera;
    private Vector2 startingPosition;
    private bool isOnScreen;

    private void Start()
    {
        distanceFromCamera = Random.Range(minDistance, maxDistance);
        mainCamera = Camera.main;
        startingPosition = transform.position;
    }

    void Update()
    {
        OnScreenCheck();
        CheckPosition();
        CounteractCameraMovement();
    }
    private void OnScreenCheck()
    {
        isOnScreen = CameraInformation.IsPositionOnScreen(transform.position);
    }
    private void CheckPosition()
    {
        if (!recalculateOffScreen)
        {
            return;
        }

        Vector2 cameraPositionInWorldSpace = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector2 deltaPositionToCameraOrigin = (Vector2)transform.position - cameraPositionInWorldSpace;

        startingPosition += CalculateDeltaPosition(deltaPositionToCameraOrigin);
    }
    /// <summary>
    /// Calculates a translation of the doodad's current position if it is out of bounds. Otherwise, returns a zero vector;
    /// </summary>
    /// <param name="posToCamera"></param>
    /// <returns></returns>
    private Vector2 CalculateDeltaPosition(Vector2 posToCamera)
    {
        Vector2 translatePosition = Vector2.zero;
        bool widthOutBounds = Mathf.Abs(posToCamera.x) / CameraInformation.cameraSize.x > RECALCULATE_FACTOR;
        if (widthOutBounds)
        {
            float randomNewHeight = Random.Range(-CameraInformation.cameraSize.y, CameraInformation.cameraSize.y);
            translatePosition = new Vector2(-2 * posToCamera.x, -posToCamera.y + randomNewHeight);
            Debug.Log("Delta pos: " + posToCamera);
        }

        bool heightOutOfBounds = Mathf.Abs(posToCamera.y) / CameraInformation.cameraSize.y > RECALCULATE_FACTOR;
        if (heightOutOfBounds)
        {
            float randomNewWidth = Random.Range(-CameraInformation.cameraSize.x, CameraInformation.cameraSize.x);
            translatePosition = new Vector2(-posToCamera.x + randomNewWidth, -2 * posToCamera.y);
            Debug.Log("Delta pos: " + posToCamera);
        }
        return translatePosition;
    }
    private void CounteractCameraMovement()
    {
        /*
        if (!isOnScreen)
        {
            return;
        }
        */
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
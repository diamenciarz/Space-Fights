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
    /// If the camera had 10 units of length, a value of 2 would mean that the position will be recalculated after moving away for a distance of 2 * 10 from the middle of the camera.
    /// </summary>
    const float CAMERA_DISTANCE_FACTOR = 1;

    private Camera mainCamera;
    private Vector2 startingPosition;

    /// <summary>
    /// If the doodad is further from the middle of the screen than that distance, then it will be put at a random position with a flipped coordinate
    /// </summary>
    public float horizontalBoundary;
    public float verticalBoundary;

    private void Start()
    {
        mainCamera = Camera.main;
        SetRandomDistance();
        SetupPositionBoundaries();
        GoToRandomPosition();
    }

    private void SetupPositionBoundaries()
    {
        horizontalBoundary = CameraInformation.GetCameraSize().x * CAMERA_DISTANCE_FACTOR;
        verticalBoundary = CameraInformation.GetCameraSize().y * CAMERA_DISTANCE_FACTOR;
    }

    void Update()
    {
        OnScreenCheck();
        CheckPosition();
        CounteractCameraMovement();
    }
    private void OnScreenCheck()
    {
        //isOnScreen = CameraInformation.IsPositionOnScreen(transform.position);
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
        bool widthOutBounds = Mathf.Abs(posToCamera.x) > horizontalBoundary;
        if (widthOutBounds)
        {
            float deltaX = -(posToCamera.x + Mathf.Sign(posToCamera.x) * horizontalBoundary);
            float deltaY = -posToCamera.y + getRandomHeight();
            translatePosition = new Vector2(deltaX, deltaY);
        }

        bool heightOutOfBounds = Mathf.Abs(posToCamera.y) > verticalBoundary;
        if (heightOutOfBounds)
        {
            float deltaY = -(posToCamera.y + Mathf.Sign(posToCamera.y) * verticalBoundary);
            float deltaX = -posToCamera.x + getRandomWidth();
            translatePosition = new Vector2(deltaX, deltaY);
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

    #region Mutator methods

    public void GoToRandomPosition()
    {
        startingPosition = new Vector2(getRandomWidth(), getRandomHeight());
    }
    public void GoToPosition(Vector2 position)
    {
        startingPosition = position;
    }
    public void SetRandomDistance()
    {
        distanceFromCamera = Random.Range(minDistance, maxDistance);
    }
    public void SetDistance(float distance)
    {
        distanceFromCamera = distance;
    }
    #endregion

    #region Calculation methods
    private float getRandomHeight()
    {
        return Random.Range(-verticalBoundary, verticalBoundary);

    }
    private float getRandomWidth()
    {
        return Random.Range(-horizontalBoundary, horizontalBoundary);
    }
    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxScroller : MonoBehaviour
{
    [SerializeField] [Range(0, MAX_DISTANCE)] float minDistance;
    [SerializeField] [Range(0, MAX_DISTANCE)] float maxDistance;

    [Tooltip("If on, the position of this doodad will be recalculated after the camera moves too far from it. Set to off if this is an important landmark")]
    [SerializeField] bool recalculateOffScreen = true;
    [SerializeField] bool changeSpriteOnRecalculation = true;
    [SerializeField] bool setRandomRotation = true;

    [Header("Custom Position")]
    [SerializeField] bool useCustomPosition = false;
    [SerializeField] Vector2 customPosition;

    private float distanceFromCamera;
    const float MAX_DISTANCE = 1000;
    /// <summary>
    /// After the doodad has moved this many camera sizes away from the middle of the camera and "recalculateOffScreen" is set to true,
    /// the position of this doodad will be recalculated.
    /// If the camera had 10 units of length, a value of 2 would mean that the position will be recalculated after moving away for a distance of 2 * 10 from the middle of the camera.
    /// </summary>
    [HideInInspector]
    public static float CAMERA_DISTANCE_FACTOR = 2;

    private Camera mainCamera;
    private Vector2 startingPosition;
    private SetRandomTexture textureUpdater;
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// If the doodad is further from the middle of the screen than that distance, then it will be put at a random position with a flipped coordinate
    /// </summary>
    private float horizontalBoundary;
    private float verticalBoundary;


    #region Startup
    private void Start()
    {
        mainCamera = Camera.main;
        textureUpdater = GetComponent<SetRandomTexture>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetRandomDistance();
        SetupPositionBoundaries();
        SetStartingPosition();
        SetRandomRotation();
    }
    private void SetupPositionBoundaries()
    {
        horizontalBoundary = CameraInformation.GetCameraSize().x * CAMERA_DISTANCE_FACTOR;
        verticalBoundary = CameraInformation.GetCameraSize().y * CAMERA_DISTANCE_FACTOR;
    }
    #endregion

    #region Update
    void Update()
    {
        OnScreenCheck();
        CheckBoundaries();
        CounteractCameraMovement();
    }
    private void OnScreenCheck()
    {
        //isOnScreen = CameraInformation.IsPositionOnScreen(transform.position);
    }
    private void CheckBoundaries()
    {
        if (!recalculateOffScreen)
        {
            return;
        }

        Vector2 cameraPositionInWorldSpace = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector2 deltaPositionToCameraOrigin = (Vector2)transform.position - cameraPositionInWorldSpace;

        Vector2 boundaryDeltaPosition = CalculateBoundaryDeltaPosition(deltaPositionToCameraOrigin);
        startingPosition += boundaryDeltaPosition;

        if (!changeSpriteOnRecalculation)
        {
            return;
        }
        bool positionOutOfBounds = boundaryDeltaPosition != Vector2.zero;
        if (positionOutOfBounds)
        {
            SetRandomSprite();
            SetRandomRotation();
        }
    }
    /// <summary>
    /// Calculates a translation of the doodad's current position if it is out of bounds. Otherwise, returns a zero vector;
    /// </summary>
    /// <param name="posToCamera"></param>
    /// <returns></returns>
    private Vector2 CalculateBoundaryDeltaPosition(Vector2 posToCamera)
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
        Vector2 cameraPosition = mainCamera.transform.position;
        Vector2 offset = CountParallaxOffset(cameraPosition);

        transform.position = new Vector3(startingPosition.x + offset.x, startingPosition.y + offset.y, 1);
    }
    private Vector2 CountParallaxOffset(Vector2 cameraPosition)
    {
        float parallaxFactor = (distanceFromCamera) / MAX_DISTANCE;

        return new Vector2(cameraPosition.x * parallaxFactor, cameraPosition.y * parallaxFactor);
    }
    private void SetRandomSprite()
    {
        if (textureUpdater)
        {
            textureUpdater.SetRandomSprite();
        }
    }
    private void SetRandomRotation()
    {
        if (setRandomRotation)
        {
            transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
        }
    }
    #endregion

    #region Movement
    private void SetStartingPosition()
    {
        if (useCustomPosition)
        {
            GoToPosition(customPosition);
        }
        else
        {
            GoToRandomPosition();
        }
    }
    #endregion

    #region Mutator methods
    public void GoToRandomPosition()
    {
        startingPosition = new Vector2(getRandomWidth(), getRandomHeight());
        useCustomPosition = false;
    }
    public void GoToPosition(Vector2 position)
    {
        customPosition = position;
        startingPosition = position;
        useCustomPosition = true;
    }
    public void SetRandomDistance()
    {
        SetDistance(Random.Range(minDistance, maxDistance));
    }
    public void SetDistance(float distance)
    {
        distanceFromCamera = distance;
        spriteRenderer.sortingOrder = (int)(-10 - distance);
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
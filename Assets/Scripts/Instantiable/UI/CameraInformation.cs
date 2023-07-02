using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInformation : MonoBehaviour
{

    private static float xMin;
    private static float xMax;
    private static float yMin;
    private static float yMax;

    /// <summary>
    /// This is given in world units. Y means height, X means width
    /// </summary>

    static Camera mainCamera;
    private static bool hasCalculatedCameraSize = false;
    private static Vector2 cameraSize;

    private void Awake()
    {
        CalculateCameraSize();
        RecountScreenEdges();
    }

    private static void CalculateCameraSize()
    {
        SetMainCamera(Camera.main);
        cameraSize.y = 2f * mainCamera.orthographicSize;
        cameraSize.x = cameraSize.y * mainCamera.aspect;
        hasCalculatedCameraSize = true;
    }

    void Update()
    {
        RecountScreenEdges();
    }
    private void RecountScreenEdges()
    {
        if (!hasCalculatedCameraSize)
        {
            CalculateCameraSize();
        }
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }

    #region Accessor methods
    /// <summary>
    /// Camera size in game units
    /// </summary>
    /// <returns></returns>
    public static Vector2 GetCameraSize()
    {
        if (!hasCalculatedCameraSize)
        {
            CalculateCameraSize();
        }
        return cameraSize;
    }
    public static bool IsPositionOnScreen(Vector2 position)
    {
        return IsPositionOnScreen(position, 0);
    }
    /// <summary>
    /// <param name="position"></param>
    /// <param name="offsetToCenter">The distance from the edge of the screen that is considered to be outside</param>
    /// </summary>
    public static bool IsPositionOnScreen(Vector2 position, float offsetToCenter)
    {
        return IsPositionOnScreen(position, offsetToCenter, offsetToCenter, offsetToCenter, offsetToCenter);
    }
    public static bool IsPositionOnScreen(Vector2 position, float leftOffset, float rightOffset, float topOffset, float bottomOffset)
    {
        return position.x > xMin + leftOffset && position.x < xMax - rightOffset
            && position.y > yMin + bottomOffset && position.y < yMax - topOffset;
    }
    public static Vector2 ClampPositionOnScreen(Vector2 position)
    {
        return ClampPositionOnScreen(position, 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="offsetToCenter">The distance from the edge of the screen that is considered to be outside</param>
    /// <returns></returns>
    public static Vector2 ClampPositionOnScreen(Vector2 position, float offsetToCenter)
    {
        return ClampPositionOnScreen(position, offsetToCenter, offsetToCenter, offsetToCenter, offsetToCenter);
    }
    public static Vector2 ClampPositionOnScreen(Vector2 position, float leftOffset, float rightOffset, float topOffset, float bottomOffset)
    {
        float newXPosition = Mathf.Clamp(position.x, xMin + leftOffset, xMax - rightOffset);
        float newYPosition = Mathf.Clamp(position.y, yMin + bottomOffset, yMax - topOffset);
        return new Vector2(newXPosition, newYPosition);
    }
    /// <returns>
    /// An array with the bottomLeft corner of the camera at index 0 and topRight corner at index 1 given in world units.
    /// </returns>
    public static Vector2[] GetDiagonalCameraPoints()
    {
        return new Vector2[]
        {
            new Vector2(xMin, yMin),
            new Vector2(xMax, yMax)
        };
    }
    #endregion

    #region Mutator methods
    public static void SetMainCamera(Camera cam)
    {
        mainCamera = cam;
    }
    #endregion
}

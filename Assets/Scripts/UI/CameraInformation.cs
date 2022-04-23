using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInformation : MonoBehaviour
{

    private static float xMin;
    private static float xMax;
    private static float yMin;
    private static float yMax;

    Camera mainCamera;

    private void Start()
    {
        SetMainCamera(Camera.main);
    }

    void Update()
    {
        RecountScreenEdges();
    }
    private void RecountScreenEdges()
    {
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }

    #region Accessor methods
    public static bool IsPositionOnScreen(Vector2 position)
    {
        return IsPositionOnScreen(position, 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="offsetToCenter">The distance from the edge of the screen that is considered to be outside</param>
    /// <returns></returns>
    public static bool IsPositionOnScreen(Vector2 position, float offsetToCenter)
    {
        return IsPositionOnScreen(position, offsetToCenter, offsetToCenter, offsetToCenter, offsetToCenter);
    }
    public static bool IsPositionOnScreen(Vector2 position, float leftOffset, float rightOffset, float topOffset, float bottomOffset)
    {
        return position.x > xMin + leftOffset && position.x < xMax - rightOffset
            && position.y > yMin - bottomOffset && position.y < yMax + topOffset;
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
    #endregion

    #region Mutator methods
    public void SetMainCamera(Camera cam)
    {
        mainCamera = cam;
    }
    #endregion
}
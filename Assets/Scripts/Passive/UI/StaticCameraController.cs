using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticCameraController
{
    public static List<GameObject> objectsToObserve = new List<GameObject>();
    private static List<GameObject> hiddenObservedObjects = new List<GameObject>();

    private static CameraControllerImplementation nonStaticCameraController;

    #region Mutator methods
    public static void SetCamera(CameraControllerImplementation cam)
    {
        nonStaticCameraController = cam;
    }
    public static void SetObserveMe(GameObject obj, bool set)
    {
        if (set)
        {
            ObserveMe(obj);
        }
        else
        {
            UnobserveMe(obj);
        }
    }
    public static void LookAtPosition(Vector2 position, float duration)
    {
        nonStaticCameraController.LookAtPosition(position, duration, hiddenObservedObjects);
    }
    #endregion

    #region Helper methods
    private static void ObserveMe(GameObject obj)
    {
        if (!hiddenObservedObjects.Contains(obj))
        {
            hiddenObservedObjects.Add(obj);
            objectsToObserve = hiddenObservedObjects;
        }
    }
    private static void UnobserveMe(GameObject obj)
    {
        if (hiddenObservedObjects.Contains(obj))
        {
            hiddenObservedObjects.Remove(obj);
            objectsToObserve = hiddenObservedObjects;
        }
    }
    #endregion
}

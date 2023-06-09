using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerImplementation : MonoBehaviour
{

    private static Coroutine currentLookingAnimation;

    void Start()
    {
        StaticCameraController.SetCamera(this);
    }
    public void LookAtPosition(Vector2 position, float waitSec, List<GameObject> hiddenObservedObjects)
    {
        StopCoroutine(currentLookingAnimation);
        currentLookingAnimation = StartCoroutine(LookTimer(position, waitSec, hiddenObservedObjects));
    }
    private IEnumerator LookTimer(Vector2 position, float waitSec, List<GameObject> hiddenObservedObjects)
    {
        GameObject posObj = Instantiate(new GameObject("Look position"), position, Quaternion.identity);
        List<GameObject> lookList = new List<GameObject> { posObj };
        StaticCameraController.objectsToObserve = lookList;

        yield return new WaitForSeconds(waitSec);
        StaticCameraController.objectsToObserve = hiddenObservedObjects;
    }
}

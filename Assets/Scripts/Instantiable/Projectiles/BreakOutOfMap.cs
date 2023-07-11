using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakOutOfMap : MonoBehaviour
{
    private float outOfMapOffset = 10;
    private bool isDestroyed = false;

    private void Start()
    {
        outOfMapOffset = CameraInformation.GetCameraSize().x;
    }

    void Update()
    {
        if (isDestroyed)
        {
            return;
        }
        if (IsOutOfMap())
        {
            isDestroyed = true;
            HelperMethods.CollisionUtils.DoDestroyActions(gameObject, TriggerOnDeath.DestroyCause.InstantBreak);
            Destroy(gameObject);
        }
    }
    private bool IsOutOfMap()
    {
        if(transform.position.x - outOfMapOffset > StaticMapInformation.topRightCorner.x)
        {
            return true;
        }
        if (transform.position.y - outOfMapOffset > StaticMapInformation.topRightCorner.y)
        {
            return true;
        }
        if (transform.position.x + outOfMapOffset < StaticMapInformation.bottomLeftCorner.x)
        {
            return true;
        }
        if (transform.position.y + outOfMapOffset < StaticMapInformation.bottomLeftCorner.y)
        {
            return true;
        }
        return false;
    }
}

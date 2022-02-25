using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserShootingController : MonoBehaviour
{
    [Tooltip("Turns user controls off")]
    [SerializeField] bool controlledByPlayer;
    [Tooltip("List of shooting controllers that should shoot when mouse is clicked")]
    [SerializeField] ShootingController[] onClick;
    [Tooltip("List of shooting controllers that should shoot when space is pressed")]
    [SerializeField] ShootingController[] onSpace;

    void Update()
    {
        if (controlledByPlayer)
        {
            CheckSpace();
            CheckMouse();
        }
    }

    #region Mouse
    private void CheckMouse()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            CallMouse(true);
        }
        else
        {
            CallMouse(false);
        }
    }
    private void CallMouse(bool isOn)
    {
        foreach (ShootingController controller in onClick)
        {
            controller.SetShoot(isOn);
        }
    }
    #endregion

    #region Space

    private void CheckSpace()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            CallSpace(true);
        }
        else
        {
            CallSpace(false);
        }
    }

    private void CallSpace(bool isOn)
    {
        foreach (ShootingController controller in onSpace)
        {
            controller.SetShoot(isOn);
        }
    }
    #endregion
}

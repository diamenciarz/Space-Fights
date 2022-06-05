using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEntityController : MonoBehaviour
{
    [SerializeField] bool controlledByPlayer = true;
    [SerializeField] Controls controls;

    public enum Controls
    {
        Keyboard,
        Mouse
    }

    #region Private variables
    IEntityMover myVehicle;
    #endregion

    #region Startup
    void Start()
    {
        StartupMethods();
    }
    private void StartupMethods()
    {
        myVehicle = GetComponent<IEntityMover>();
    }
    #endregion

    #region Update
    void Update()
    {
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (!controlledByPlayer)
        {
            return;
        }
        if (controls == Controls.Keyboard)
        {
            DoKeyboardInput();
        }
        if (controls == Controls.Mouse)
        {
            DoMouseInput();
        }
    }
    private void DoKeyboardInput()
    {
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        myVehicle.SetInputVector(inputVector);
    }
    private void DoMouseInput()
    {
        Vector2 mousePos = HelperMethods.VectorUtils.TranslatedMousePosition(transform.position);
        Vector2 deltaMousePos = HelperMethods.VectorUtils.DeltaPosition((Vector2) transform.position, mousePos);
        Vector2 forwardVector = transform.right;

        float deltaAngle = Vector2.SignedAngle(forwardVector, deltaMousePos);
        Vector2 directionVector = HelperMethods.VectorUtils.DirectionVectorNormalized(deltaAngle); ;
        myVehicle.SetInputVector(directionVector);
    }
    #endregion
}

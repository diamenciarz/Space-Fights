using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.EventSystems.EventTrigger;

public class UserEntityController : MonoBehaviour
{
    [SerializeField] bool isControlledByPlayer = true;
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
        SetIsPlayerControlled();
    }
    private void StartupMethods()
    {
        myVehicle = GetComponent<IEntityMover>();
    }
    private void SetIsPlayerControlled()
    {
        StaticCameraController.SetObserveMe(gameObject, isControlledByPlayer);
        IPlayerControllable[] components = GetComponentsInChildren<IPlayerControllable>();
        foreach (IPlayerControllable component in components)
        {
            component.SetIsControlledByMouse(isControlledByPlayer);
        }
    }
    #endregion

    #region Update
    void Update()
    {
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (!isControlledByPlayer)
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
        if (!HelperMethods.InputUtils.IsMouseRightClicked())
        {
            myVehicle.SetInputVector(Vector2.zero);
            return;
        }
        Vector2 mousePos = HelperMethods.VectorUtils.TranslatedMousePosition(transform.position);
        Vector2 deltaMousePos = HelperMethods.VectorUtils.DeltaPosition((Vector2)transform.position, mousePos);

        Debug.Log(deltaMousePos);
        myVehicle.SetInputVector(ScaleInput(deltaMousePos));
    }
    private Vector2 ScaleInput(Vector2 deltaMousePos)
    {
        /// As the entity is approaching the mouse cursor, the input comes closer to zero.
        /// This way, the entity won't overshoot behind the mouse cursor.
        const float SLOWDOWN_DISTANCE = 5f;
        float magnitude = deltaMousePos.magnitude;
        float scaler = Mathf.Min(magnitude / SLOWDOWN_DISTANCE, 1);
        if (scaler < 0.2)
        {
            scaler = 0;
        }
        deltaMousePos = deltaMousePos.normalized * scaler;

        return deltaMousePos;
    }
    #endregion

    #region Mutator methods
    public void SetIsControlledByPlayer(bool set)
    {
        isControlledByPlayer = set;
        SetIsPlayerControlled();
    }
    #endregion
}

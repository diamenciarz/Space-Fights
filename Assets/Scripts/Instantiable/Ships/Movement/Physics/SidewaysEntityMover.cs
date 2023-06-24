using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;
using static EntityInput;

[RequireComponent(typeof(Rigidbody2D))]

public class SidewaysEntityMover : MonoBehaviour, IEntityMover, IMoveable
{
    enum RotationMode
    {
        ROTATE_FORWARD,
        STABILIZE
    }

    #region Serialization
    [Header("Movement")]
    [Tooltip("The highest speed that the vehicle can move at")]
    [SerializeField] float maxSpeed;
    [Tooltip("The maximum movement force that can be applied")]
    [SerializeField] float M1 = 25000;
    [SerializeField] float M2 = 10;
    [SerializeField] float M3 = 1;

    [Header("Rotation")]
    [SerializeField] RotationMode rotationMode; 
    [SerializeField] float T1 = 5000;
    [SerializeField] float T2 = 10;
    [SerializeField] float T3 = 40;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;
    private Vector2 lastVelocity;
    private Vector2 lastInput;
    private float previousAngularVelocity;

    #endregion

    #region Startup
    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        lastVelocity = Vector2.up;
        previousAngularVelocity = myRigidbody2D.angularVelocity;
    }
    #endregion

    #region Mutator methods
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
    }
    #endregion

    #region Update
    void FixedUpdate()
    {
        UpdateVelocity();
        UpdateRotation();
        previousAngularVelocity = myRigidbody2D.angularVelocity;
        lastVelocity = myRigidbody2D.velocity;
    }

    #region Movement
    private void UpdateVelocity()
    {
        Vector2 forceToApply = GetForceToApply();
        ApplyForce(forceToApply);
    }
    private Vector2 GetForceToApply()
    {
        {
            if (inputVector.magnitude > 0)
            {
                lastInput = inputVector;
            }

            Vector2 m1 = inputVector * M1;
            Vector2 m2 = -M2 * myRigidbody2D.velocity;
            Vector2 m3 = -M3 * GetAcceleration();
            return m1 + m2 + m3;
        }
    }
    private void ApplyForce(Vector2 force)
    {
        myRigidbody2D.AddForce(force * Time.deltaTime, ForceMode2D.Force);
        // Send signal to visible afterburners
    }
    #endregion

    #region Rotation
    private void UpdateRotation()
    {
        if (rotationMode == RotationMode.ROTATE_FORWARD)
        {
            RoatateForward();
        }
        if (rotationMode == RotationMode.STABILIZE)
        {
            Stabilize();
        }
    }
    private void RoatateForward()
    {
        float deltaAngle = CalculateDeltaAngle(inputVector);
        float torqueToApply = GetTorqueToApply(deltaAngle);
        ApplyTorque(torqueToApply);
    }
    private void Stabilize()
    {
        float deltaAngle = CalculateDeltaAngle(Vector2.up);
        float torqueToApply = GetTorqueToApply(deltaAngle);
        ApplyTorque(torqueToApply);
    }
    private float CalculateDeltaAngle(Vector2 targetDirection)
    {
        if (targetDirection.magnitude == 0)
        {
            targetDirection = lastInput;
        }
        float directionVector = HelperMethods.VectorUtils.VectorDirection(targetDirection);
        float directionVectorClamped = HelperMethods.AngleUtils.ClampAngle180(directionVector);
        float rotation = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float deltaAngle = HelperMethods.AngleUtils.ClampAngle180(directionVectorClamped - rotation);

        return deltaAngle;
    }

    private float GetTorqueToApply(float deltaAngle)
    {
        float anglePercentage = deltaAngle / 180;
        float t1 = anglePercentage * T1;
        float t2 = -T2 * myRigidbody2D.angularVelocity;
        float t3 = -T3 * GetAngularAcceleration();
        return t1 + t2 + t3;
    }

    private void ApplyTorque(float torque)
    {
        myRigidbody2D.AddTorque(torque * Time.deltaTime);
    }

    #endregion
    #endregion

    #region Accessor methods
    public float GetSpeed()
    {
        return myRigidbody2D.velocity.magnitude;
    }
    private Vector2 GetSidewayVelocity()
    {
        return transform.right * Vector2.Dot(myRigidbody2D.velocity, transform.right);
    }
    private Vector2 GetForwardVelocity()
    {
        return transform.up * Vector2.Dot(myRigidbody2D.velocity, transform.up);
    }
    public float GetAngularAcceleration()
    {
        return myRigidbody2D.angularVelocity - previousAngularVelocity;
    }
    public Vector2 GetAcceleration()
    {
        return myRigidbody2D.velocity - lastVelocity;
    }
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
    #endregion
}

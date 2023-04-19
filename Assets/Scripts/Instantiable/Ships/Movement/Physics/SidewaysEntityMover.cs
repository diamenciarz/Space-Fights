using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;
using static EntityInput;

public class SidewaysEntityMover : MonoBehaviour, IEntityMover
{

    #region Serialization
    [Category("Movement")]
    [Tooltip("The highest speed that the vehicle can move at")]
    [SerializeField] float maxSpeed;
    [Tooltip("The maximum movement force that can be applied")]
    [SerializeField] float M1 = 25000;
    [SerializeField] float M2 = 10;
    [SerializeField] float M3 = 1;
    [Category("Rotation")]
    [SerializeField] float T1 = 5000;
    [SerializeField] float T2 = 10;
    [SerializeField] float T3 = 40;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;
    private Vector2 lastVelocity;
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

    #region Public methods
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
    }
    #endregion

    #region Update
    void FixedUpdate()
    {
        UpdateMovement();
        UpdateRotation();
        previousAngularVelocity = myRigidbody2D.angularVelocity;
    }

    #region Movement
    private void UpdateMovement()
    {
        Vector2 deltaVelocity = CalculateDeltaVelocity(inputVector);
        Vector2 forceToApply = GetForceToApply(deltaVelocity);
        ApplyMovementForce(forceToApply);
        //KeepVelocityInBounds(forceToApply);
    }
    private Vector2 CalculateDeltaVelocity(Vector2 inputVector)
    {
        return (inputVector * maxSpeed) - myRigidbody2D.velocity;
    }
    private Vector2 GetForceToApply(Vector2 deltaVelocity)
    {
        /*
        if (inputVector.magnitude == 0)
        {
            Vector2 forceInTime = maxForce * -myRigidbody2D.velocity.normalized;
            float forceMultiplier = GetSpeed() / maxSpeed;
            Vector2 clampedForce = forceInTime * forceMultiplier;

            return clampedForce / Time.fixedDeltaTime;
        }
        else
        */
        {
            if (HelperMethods.InputUtils.IsAnyInputKeyPressed())
                //TODO:Change to input
            {   
                lastVelocity = myRigidbody2D.velocity;
            }

            Vector2 speedPercentage = deltaVelocity / maxSpeed;
            Vector2 m1 = speedPercentage * M1;
            Vector2 m2 = -M2 * myRigidbody2D.velocity;
            Vector2 m3 = -M3 * GetAcceleration();
            return m1 + m2 + m3;

            //Vector2 forceInTime = maxForce * inputVector;
            //float forceMultiplier = CalculateForceMultiplier(forceInTime);
            //Vector2 clampedForce = forceInTime * forceMultiplier;

            //return clampedForce / Time.fixedDeltaTime;
        }
    }
    private float CalculateForceMultiplier(Vector2 appliedForce)
    {
        Vector2 forceInWorldspace = CalculateAppliedForce(appliedForce);
        Vector2 velocityInForceDirection = HelperMethods.VectorUtils.ProjectVector(myRigidbody2D.velocity, forceInWorldspace);
        float dot = Vector2.Dot(myRigidbody2D.velocity, forceInWorldspace);

        bool forceInFlightDirection = dot > 0;
        if (forceInFlightDirection)
        {
            float deltaSpeedToMax = maxSpeed - velocityInForceDirection.magnitude;
            return deltaSpeedToMax / maxSpeed;
        }
        return 1;
    }
    private void KeepVelocityInBounds(Vector2 appliedForce)
    {
        Vector2 velocity = myRigidbody2D.velocity;
        Vector2 forceInWorldspace = CalculateAppliedForce(appliedForce);
        Vector2 velocityInForceDirection = HelperMethods.VectorUtils.ProjectVector(velocity, forceInWorldspace);
        Vector2 perpendicularVelocity = velocity - velocityInForceDirection;

        float maxPerpendicularSpeed = Mathf.Sqrt(maxSpeed * maxSpeed - velocityInForceDirection.sqrMagnitude);
        Vector2 allowedPerpendicularVelocity = Vector2.ClampMagnitude(perpendicularVelocity, maxPerpendicularSpeed);
        myRigidbody2D.velocity = allowedPerpendicularVelocity + velocityInForceDirection;
    }
    private Vector2 CalculateAppliedForce(Vector2 appliedForce)
    {
        return appliedForce;
    }
    private void ApplyMovementForce(Vector2 force)
    {
        myRigidbody2D.AddForce(force * Time.deltaTime, ForceMode2D.Force);
        // Send signal to visible afterburners
    }
    #endregion

    #region Rotation
    private void UpdateRotation()
    {
        float deltaAngle = CalculateDeltaAngle(inputVector);
        float torqueToApply = GetTorqueToApply(deltaAngle);
        ApplyTorque(torqueToApply);
    }

    private float CalculateDeltaAngle(Vector2 targetDirection)
    {
        if (targetDirection.magnitude == 0)
        {
            targetDirection = lastVelocity;
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
    private float GetAngularAcceleration()
    {
        return myRigidbody2D.angularVelocity - previousAngularVelocity;
    }
    private Vector2 GetAcceleration()
    {
        return myRigidbody2D.velocity - lastVelocity;
    }
    #endregion
}

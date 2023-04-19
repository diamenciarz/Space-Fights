using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static EntityInput;

public class SidewaysEntityMover : MonoBehaviour, IEntityMover
{

    #region Serialization
    [Tooltip("The highest speed that the vehicle can accelerate towards")]
    [SerializeField] float maxSpeed;
    [Tooltip("The maximum force that can be applied")]
    [SerializeField] float force;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;

    #endregion

    #region Startup
    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
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
        Vector2 forceToApply = GetForceToApply(inputVector);
        ApplyForce(forceToApply);
        KeepVelocityInBounds(forceToApply);
    }

    private Vector2 GetForceToApply(Vector2 inputVector)
    {
        Vector2 forceInTime = force * inputVector * Time.fixedDeltaTime;
        float forceMultiplier = CalculateForceMultiplier(forceInTime);
        Vector2 clampedForce = forceInTime * forceMultiplier;

        return clampedForce / Time.fixedDeltaTime;
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
    private void ApplyForce(Vector2 force)
    {
        myRigidbody2D.AddForce(force * Time.deltaTime, ForceMode2D.Force);
        return;
    }
    #endregion

    #region Accessor methods
    private float GetSpeed()
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
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
    #endregion
}

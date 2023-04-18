using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityInput;

public class SidewaysEntityMover : MonoBehaviour
{

    #region Serialization
    [Tooltip("The highest speed that the vehicle can accelerate towards")]
    [SerializeField] float maxSpeed;
    [Tooltip("The lowest speed that the vehicle can travel backwards")]
    [SerializeField] float minSpeed;
    [Tooltip("Turn speed in degrees per second (at the highest speed)")]
    [SerializeField] float maxTurningSpeed;
    [Tooltip("How slippery the driving experience is. 1 for no drifting, 0 for driving on ice")]
    [SerializeField][Range(0, 1)] float driftFactor;
    [SerializeField] bool isForceGlobal = true;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;

    private float previousRotationAngle;
    private float directionAngle;
    #endregion

    #region Startup
    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        directionAngle = transform.rotation.eulerAngles.z;
        previousRotationAngle = directionAngle;
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region Public methods
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
    }
    public void RotateByAngle(float rotation, bool affectedByVelocity)
    {
        float deltaAngle = CalculateDeltaAngle(rotation, affectedByVelocity);
        ModifyDirection(deltaAngle);
    }
    public void RotateTowardsVector(Vector2 targetDirection, bool affectedByVelocity)
    {
        float deltaAngle = CalculateDeltaAngle(targetDirection, affectedByVelocity);
        ModifyDirection(deltaAngle);
    }
    #region Helper methods
    private float CalculateDeltaAngle(float rotation, bool affectedByVelocity)
    {
        if (affectedByVelocity)
        {
            float maxSpeedPercentage = myRigidbody2D.velocity.magnitude / maxSpeed;
            return rotation * Time.fixedDeltaTime * maxSpeedPercentage;
        }
        return rotation * Time.fixedDeltaTime;
    }
    private float CalculateDeltaAngle(Vector2 targetDirection, bool affectedByVelocity)
    {
        float directionVector = HelperMethods.VectorUtils.VectorDirection(targetDirection);
        float directionVectorClamped = HelperMethods.AngleUtils.ClampAngle180(directionVector);
        float rotation = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float deltaAngle = HelperMethods.AngleUtils.ClampAngle180(directionVectorClamped - rotation);

        float deltaStep = Mathf.Sign(deltaAngle) * maxTurningSpeed * Time.fixedDeltaTime;

        //0.1 is a good multiplier that avoids the counter torque from overshooting and creating wiggle
        const float WIGGLE_DAMPER = 0.1f;
        bool angleIsSmall = Mathf.Abs(deltaAngle) < maxTurningSpeed * WIGGLE_DAMPER;
        if (angleIsSmall)
        {
            return deltaAngle * Time.fixedDeltaTime;
        }

        if (affectedByVelocity)
        {
            float maxSpeedPercentage = myRigidbody2D.velocity.magnitude / maxSpeed;
            return deltaStep * maxSpeedPercentage;
        }
        return deltaStep;
    }
    private void ModifyDirection(float deltaDirection)
    {
        float minRotation = previousRotationAngle - maxTurningSpeed * Time.fixedDeltaTime;
        float maxRotation = previousRotationAngle + maxTurningSpeed * Time.fixedDeltaTime;
        directionAngle = Mathf.Clamp(directionAngle + deltaDirection, minRotation, maxRotation);
    }
    #endregion
    #endregion

    #region Update
    void FixedUpdate()
    {
        ApplyForce(inputVector);
    }
    private void ApplyForce(Vector2 force)
    {
        if (isForceGlobal)
        {
            myRigidbody2D.AddForce(force, ForceMode2D.Force);
            return;
        }
        myRigidbody2D.AddRelativeForce(force, ForceMode2D.Force);
    }
    #endregion

    #region Accessor methods
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
    public float GetMinSpeed()
    {
        return minSpeed;
    }
    public float GetMaxTurningSpeed()
    {
        return maxTurningSpeed;
    }
    public float GetDirectionAngle()
    {
        return directionAngle;
    }
    #endregion
}

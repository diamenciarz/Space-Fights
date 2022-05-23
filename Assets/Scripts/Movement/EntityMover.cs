using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : MonoBehaviour, IEntityMover
{
    #region Serialization
    [Tooltip("The highest speed that the vehicle can accelerate towards")]
    [SerializeField] float maxSpeed;
    [Tooltip("The lowest speed that the vehicle can travel backwards")]
    [SerializeField] float minSpeed;
    [Tooltip("Turn speed in degrees per second (at the highest speed)")]
    [SerializeField] float maxTurningSpeed;
    [Tooltip("How slippery the driving experience is. 1 for no drifting, 0 for driving on ice")]
    [SerializeField] [Range(0, 1)] float driftFactor;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;

    private float previousRotationAngle;
    private float rotationAngle;

    #endregion

    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
    }

    void FixedUpdate()
    {
        KillSidewayVelocity();
        HoldRotation();
    }
    private void KillSidewayVelocity()
    {
        myRigidbody2D.velocity = GetSidewayVelocity() * driftFactor + GetForwardVelocity();
    }
    private void HoldRotation()
    {
        myRigidbody2D.MoveRotation(rotationAngle);
        previousRotationAngle = rotationAngle;
    }
    public void RotateByAngle(float rotation)
    {
        float maxSpeedPercentage = myRigidbody2D.velocity.magnitude / maxSpeed;
        float deltaAngle = rotation * Time.fixedDeltaTime * maxSpeedPercentage;
        rotationAngle -= deltaAngle;

        float minRotation = previousRotationAngle - maxTurningSpeed * Time.fixedDeltaTime;
        float maxRotation = previousRotationAngle + maxTurningSpeed * Time.fixedDeltaTime;
        rotationAngle = Mathf.Clamp(rotationAngle, minRotation, maxRotation);
    }

    #region Accessor methods
    private Vector2 GetSidewayVelocity()
    {
        return transform.right * Vector2.Dot(myRigidbody2D.velocity, transform.right);
    }
    private Vector2 GetForwardVelocity()
    {
        return transform.up * Vector2.Dot(myRigidbody2D.velocity, transform.up);
    }
    #endregion
}

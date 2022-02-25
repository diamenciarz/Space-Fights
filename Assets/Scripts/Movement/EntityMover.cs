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
    [Tooltip("The speed change rate, when accelerating")]
    [SerializeField] float acceleratingForce;
    [Tooltip("The speed change rate, when braking")]
    [SerializeField] float brakingForce;
    [Tooltip("The speed change rate, when baking")]
    [SerializeField] float backingForce;
    [Tooltip("Turn speed in degrees per second (at the highest speed)")]
    [SerializeField] float turningSpeed;
    [Tooltip("How slippery the driving experience is. 1 for no drifting, 0 for driving on ice")]
    [SerializeField] [Range(0, 1)] float driftFactor;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;

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
        ChangeVelocity();

        KillSidewayVelocity();

        RotateByInput(inputVector);
    }
    private void ChangeVelocity()
    {
        myRigidbody2D.AddForce(GetEngineForce(), ForceMode2D.Force);
    }
    private Vector2 GetEngineForce()
    {
        if (inputVector.y > 0)
        {
            return GetForwardForce();
        }
        else
        {
            return GetBackwardsForce();
        }
    }
    private Vector2 GetForwardForce()
    {
        if (GetSpeed() < maxSpeed)
        {
            Vector2 basicForce = inputVector.y * transform.right * acceleratingForce * Time.fixedDeltaTime;
            float maxForce = (maxSpeed - GetSpeed()) / Time.fixedDeltaTime / myRigidbody2D.mass;
            return Vector2.ClampMagnitude(basicForce, maxForce);
        }
        else
        {
            return Vector2.zero;
        }
    }
    private Vector2 GetBackwardsForce()
    {
        if (IsGoingForward())
        {
            //Is braking
            Vector2 basicForce = inputVector.y * transform.right * brakingForce * Time.fixedDeltaTime;
            float maxForce = (GetSpeed() - minSpeed) / Time.fixedDeltaTime / myRigidbody2D.mass;
            return Vector2.ClampMagnitude(basicForce, maxForce);
        }

        if (GetSpeed() <= minSpeed)
        {
            return Vector2.zero;
        }
        else
        {
            //Is backing
            Vector2 basicForce = inputVector.y * transform.right * backingForce * Time.fixedDeltaTime;
            float maxForce = (GetSpeed() - minSpeed) / Time.fixedDeltaTime / myRigidbody2D.mass;
            return Vector2.ClampMagnitude(basicForce, maxForce);
        }
    }

    private void KillSidewayVelocity()
    {
        myRigidbody2D.velocity = GetSidewayVelocity() * driftFactor + GetForwardVelocity();
    }
    private void RotateByInput(Vector2 inputVector)
    {
        float maxSpeedPercentage = myRigidbody2D.velocity.magnitude / maxSpeed;
        float deltaAngle = inputVector.x * turningSpeed * Time.fixedDeltaTime * maxSpeedPercentage;
        rotationAngle -= deltaAngle;

        myRigidbody2D.MoveRotation(rotationAngle);
    }

    #region Accessor methods
    //Physics
    public Vector2 GetVelocity()
    {
        return myRigidbody2D.velocity;
    }
    public float GetSpeed()
    {
        Vector2 velocityVector = GetForwardVelocity();
        float speed = velocityVector.magnitude;
        if (Vector2.Dot(myRigidbody2D.velocity, transform.right) > 0)
        {
            return speed;
        }
        else
        {
            return -speed;
        }
    }
    private bool IsGoingForward()
    {
        float dot = Vector2.Dot(myRigidbody2D.velocity, transform.right);
        if (dot >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public Vector2 GetSidewayVelocity()
    {
        return transform.up * Vector2.Dot(myRigidbody2D.velocity, transform.up);
    }
    public Vector2 GetForwardVelocity()
    {
        return transform.right * Vector2.Dot(myRigidbody2D.velocity, transform.right);
    }
    //Movement
    public float GetBrakingSpeed()
    {
        return brakingForce;
    }
    public float GetAcceleratingSpeed()
    {
        return acceleratingForce;
    }
    public float GetTurningSpeed()
    {
        return turningSpeed;
    }
    #endregion
}

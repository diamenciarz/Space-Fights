using UnityEngine;

public class ForwardEntityMover : MonoBehaviour, IEntityMover
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
    [Header("Movement")]
    [Tooltip("The maximum movement force that can be applied")]
    [SerializeField] float M1 = 25000;
    [SerializeField] float M2 = 10;
    [SerializeField] float M3 = 1;
    [Header("Rotation")]
    [SerializeField] float T1 = 2500;
    [SerializeField] float T2 = 250;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;
    private Vector2 lastVelocity;

    private float previousAngularVelocity;
    private float previousRotationAngle;
    private float targetAngle;
    #endregion

    #region Startup
    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        targetAngle = transform.rotation.eulerAngles.z;
        previousRotationAngle = targetAngle;
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region Update
    void FixedUpdate()
    {
        // Rotation
        UpdateTargetAngle();
        RotateTowardsDirectionAngle();
        //Movement
        UpdateVelocity();
        KillSidewayVelocity();
    }

    #region Movement
    private void UpdateVelocity()
    {
        Vector2 forceToApply = GetForceToApply();
        ApplyForce(forceToApply);
    }
    private Vector2 GetForceToApply()
    {
        Vector2 m1 = GetForwardForce() * M1;
        Vector2 m2 = -M2 * myRigidbody2D.velocity;
        Vector2 m3 = M3 * GetAcceleration();
        //Debug.Log("P: " + m1 + " I: " + m2 + " D: " + m3);
        return m1 + m2 + m3;
    }
    private Vector2 GetForwardForce()
    {
        const float SLIGHT_SIDEWAYS_MOVEMENT = 0.1f;
        return transform.up * inputVector.y + transform.right * inputVector.x * SLIGHT_SIDEWAYS_MOVEMENT;
        //return transform.up * Vector2.Dot(inputVector, transform.up);
    }
    private void ApplyForce(Vector2 force)
    {
        myRigidbody2D.AddForce(force * Time.deltaTime, ForceMode2D.Force);
        // Send signal to visible afterburners
    }
    #endregion

    #region Rotation
    private void UpdateTargetAngle()
    {
        if (inputVector.x != 0)
        {
            //float deltaAngle = GetDeltaAngle();

            float deltaAngle = 5 * -Mathf.Sign(inputVector.x);
            ModifyDirection(deltaAngle);
        }
        Vector2 angleVector = HelperMethods.VectorUtils.DirectionVector(3, targetAngle);
        Debug.DrawRay(transform.position, angleVector, Color.red);
    }
    private float GetDeltaAngle()
    {
        float directionVector = HelperMethods.VectorUtils.VectorDirection(inputVector);
        float directionVectorClamped = HelperMethods.AngleUtils.ClampAngle180(directionVector);
        float rotation = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float deltaAngle = HelperMethods.AngleUtils.ClampAngle180(directionVectorClamped - rotation);
        //float maxSpeedPercentage = myRigidbody2D.velocity.magnitude / maxSpeed;
        return deltaAngle;
    }
    private void ModifyDirection(float deltaDirection)
    {
        float currentAngle = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float angleFromShipToTargetDirection = HelperMethods.AngleUtils.ClampAngle180(targetAngle - currentAngle);
        float moveFactor = 1;
        if (Mathf.Sign(deltaDirection) == Mathf.Sign(angleFromShipToTargetDirection))
        {
            moveFactor = 1 - Mathf.Abs(angleFromShipToTargetDirection / 45);
        }
        else
        {
            moveFactor = 1 + Mathf.Abs(angleFromShipToTargetDirection / 45);
        }

        previousRotationAngle = targetAngle;
        float minRotation = previousRotationAngle - maxTurningSpeed * Time.fixedDeltaTime * moveFactor;
        float maxRotation = previousRotationAngle + maxTurningSpeed * Time.fixedDeltaTime * moveFactor;
        targetAngle = Mathf.Clamp(targetAngle + deltaDirection, minRotation, maxRotation);
    }
    private void RotateTowardsDirectionAngle()
    {
        float currentAngle = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float t1 = T1 * HelperMethods.AngleUtils.ClampAngle180(targetAngle - currentAngle);
        float t2 = T2 * -myRigidbody2D.angularVelocity;

        float torque = t1 + t2;
        Debug.Log("P: " + t1 + " I: " + t2 + " D: " + 0 + " angVelocity: " + myRigidbody2D.angularVelocity);
        myRigidbody2D.AddTorque(torque);
    }
    #endregion

    #region Stability
    private void KillSidewayVelocity()
    {
        myRigidbody2D.velocity = GetSidewayVelocity() * driftFactor + GetForwardVelocity();
    }
    #endregion
    #endregion

    #region Public methods
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
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
    public float GetAngularAcceleration()
    {
        return (myRigidbody2D.angularVelocity - previousAngularVelocity) / Time.deltaTime;
    }
    public Vector2 GetAcceleration()
    {
        return (myRigidbody2D.velocity - lastVelocity) / Time.deltaTime;
    }
    public float GetDirectionAngle()
    {
        return targetAngle;
    }
    #endregion
}
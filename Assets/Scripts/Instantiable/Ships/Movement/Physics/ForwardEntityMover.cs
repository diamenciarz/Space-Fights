using UnityEngine;

public class ForwardEntityMover : MonoBehaviour, IEntityMover
{
    #region Serialization
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
    [SerializeField] float T3 = 250;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    private Vector2 inputVector;
    private Vector2 translatedInputVector;
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
        // Translate input vector
        TranslateInputVector();
        // Rotation
        UpdateTargetAngle();
        RotateTowardsDirectionAngle();
        //Movement
        UpdateVelocity();
        KillSidewayVelocity();
        //TODO: Unsolved
        FixAngularVelocityBug();
    }
    private void FixAngularVelocityBug()
    {
        if (Mathf.Abs(myRigidbody2D.angularVelocity) > 1000)
        {
            myRigidbody2D.angularVelocity = 0;
        }
    }
    #region Input translation
    private void TranslateInputVector()
    {
        if (inputVector.magnitude == 0)
        {
            translatedInputVector = Vector2.zero;
            return;
        }
        float currentAngle = HelperMethods.AngleUtils.ClampAngle180(myRigidbody2D.rotation);
        float inputAngle = HelperMethods.VectorUtils.VectorDirection(inputVector);
        float currentAngleToInput = inputAngle - currentAngle;
        translatedInputVector = HelperMethods.VectorUtils.DirectionVectorNormalized(currentAngleToInput);
    }
    #endregion

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
        return transform.up * translatedInputVector.y + transform.right * translatedInputVector.x * SLIGHT_SIDEWAYS_MOVEMENT;
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
        Vector2 angleVector = HelperMethods.VectorUtils.DirectionVector(3, targetAngle);
        Debug.DrawRay(transform.position, angleVector, Color.red);
        if (translatedInputVector.x != 0)
        {
            float deltaAngle = -Mathf.Sign(translatedInputVector.x);
            float inputVectorDirection = HelperMethods.VectorUtils.VectorDirection(inputVector);
            bool snapToInputVector = Mathf.Abs(HelperMethods.AngleUtils.ClampAngle360(targetAngle) - inputVectorDirection) < 5;
            if (snapToInputVector)
            {
                targetAngle = inputVectorDirection;
                return;
            }

            ModifyDirection(deltaAngle);
        }
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
        float t3 = T3 * -GetAngularAcceleration();

        previousAngularVelocity = myRigidbody2D.angularVelocity;
        float torque = t1 + t2 + t3;
        Debug.Log("P: " + t1 + " I: " + t2 + " D: " + t3);
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
        Vector2 mapEdgeVector = CalculateMapEdgeVector();
        if (mapEdgeVector.magnitude > 1)
        {
            mapEdgeVector = mapEdgeVector.normalized;
        }
        inputVector = mapEdgeVector + (1 - mapEdgeVector.magnitude) * newInputVector;
    }
    private Vector2 CalculateMapEdgeVector()
    {
        Vector2 returnVector = Vector2.zero;
        Vector2 topRightDelta = (Vector2)transform.position - StaticMapInformation.topRightCorner;
        if (topRightDelta.x > 0)
        {
            returnVector.x -= topRightDelta.x;
        }
        if (topRightDelta.y > 0)
        {
            returnVector.y -= topRightDelta.y;
        }
        Vector2 bottomLeftDelta = (Vector2)transform.position - StaticMapInformation.bottomLeftCorner;
        if (bottomLeftDelta.x < 0)
        {
            returnVector.x -= topRightDelta.x;
        }
        if (bottomLeftDelta.y < 0)
        {
            returnVector.y -= topRightDelta.y;
        }
        return returnVector;
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

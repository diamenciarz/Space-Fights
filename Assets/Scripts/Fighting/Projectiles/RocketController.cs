using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{

    [Header("Rocket target")]
    [SerializeField] GameObject objectMissingIconGameObject;

    [Header("Rocket Flight Settings")]
    [SerializeField] float maxRocketSpeed = 5f;
    [SerializeField] float speedChangeRatePerSecond = 3f;
    [Tooltip("In degrees per second")]
    [SerializeField] float rocketRotationSpeed = 120;
    [Header("Explosion Settings")]
    public float timeToExpire = 30;

    //Private variables
    public float currentRocketSpeed;
    public GameObject targetGameObject;

    #region Startup
    protected override void Awake()
    {
        base.Awake();
        SetupStartingSpeed();
    }
    protected override void Start()
    {
        base.Start();
        CreateMiaIcon();
    }
    private void CreateMiaIcon()
    {
        if (objectMissingIconGameObject != null)
        {
            GameObject miaGameObject = Instantiate(objectMissingIconGameObject, transform.position, Quaternion.identity);
            miaGameObject.GetComponent<ObjectMissingIcon>().TryFollowThisObject(gameObject);
        }
    }
    private void SetupStartingSpeed()
    {
        if (startingSpeed == -1)
        {
            currentRocketSpeed = maxRocketSpeed;
        }
        else
        {
            currentRocketSpeed = startingSpeed;
        }
    }
    #endregion

    #region Every Frame
    protected void Update()
    {
        CheckForTarget();
        TurnTowardsTarget();
        IncreaseSpeed();
        UpdateSpeed();
    }
    private void CheckForTarget()
    {
        if (team != 0)
        {
            if (targetGameObject == null)
            {
                targetGameObject = StaticDataHolder.GetClosestEnemy(transform.position, team);
            }
        }
    }
    private void IncreaseSpeed()
    {
        if (currentRocketSpeed != maxRocketSpeed)
        {
            currentRocketSpeed = Mathf.MoveTowards(currentRocketSpeed, maxRocketSpeed, speedChangeRatePerSecond * Time.deltaTime);
        }
    }
    private void UpdateSpeed()
    {
        Vector3 newVelocity = HelperMethods.VectorUtils.DirectionVectorNormalized(transform.eulerAngles.z) * currentRocketSpeed;
        SetVelocityVector(newVelocity);
    }
    public override void SetVelocityVector(Vector3 newVelocityVector)
    {
        velocityVector = newVelocityVector;
        myRigidbody2D.velocity = newVelocityVector;
        Quaternion deltaRotation = Quaternion.Euler(0, 0, -90); // Weird thing with rockets
        transform.rotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, transform.position + newVelocityVector) * deltaRotation;
    }
    private void TurnTowardsTarget()
    {
        if (targetGameObject)
        {
            Quaternion deltaRotation = Quaternion.Euler(0, 0, -90);// Weird thing with rockets
            Quaternion targetRotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, targetGameObject.transform.position) * deltaRotation;
            float deltaAngleThisFrame = rocketRotationSpeed * Time.deltaTime;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, deltaAngleThisFrame);
        }
    }
    #endregion

    #region Accessor/Mutator Methods
    public void SetTarget(GameObject target)
    {
        targetGameObject = target;
    }
    public float GetMaxRocketSpeed()
    {
        return maxRocketSpeed;
    }
    public float GetCurrentRocketSpeed()
    {
        return currentRocketSpeed;
    }
    public void SetCurrentRocketSpeed(float newSpeed)
    {
        currentRocketSpeed = newSpeed;
    }
    #endregion
}

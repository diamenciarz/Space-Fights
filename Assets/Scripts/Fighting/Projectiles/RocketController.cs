using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{

    [Header("Rocket Flight Settings")]
    [SerializeField] float maxRocketSpeed = 5f;
    [SerializeField] float speedChangeRatePerSecond = 3f;
    [Tooltip("In degrees per second")]
    [SerializeField] float rocketRotationSpeed = 120;
    [Header("Explosion Settings")]
    public float timeToExpire = 30;
    [Header("Targeting settings")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;
    [SerializeField] Transform rocketEyeTransform;
    //Private variables
    private float currentRocketSpeed;
    public GameObject target;

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetupStartingSpeed();
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
        if (team.teamInstance != TeamInstance.Neutral)
        {
            GameObject potentialGameObject = FindNewTarget();
            if (!potentialGameObject)
            {
                return;
            }
            if (target == null)
            {
                target = potentialGameObject;
                return;
            }
            if (StaticDataHolder.ListContents.Generic.IsFirstCloserToMiddleThanSecond(potentialGameObject, target, rocketEyeTransform.position, GetRocketDirection()))
            {
                target = potentialGameObject;
            }
        }
    }
    private GameObject FindNewTarget()
    {
        List<GameObject> potentialTargets = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);

        return StaticDataHolder.ListContents.Generic.GetClosestObjectInSightAngleWise(potentialTargets, rocketEyeTransform.position, GetRocketDirection());
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
    protected override void SetVelocityVector(Vector3 newVelocityVector)
    {
        velocityVector = newVelocityVector;
        myRigidbody2D.velocity = newVelocityVector;
        Quaternion deltaRotation = Quaternion.Euler(0, 0, -90); // Weird thing with rockets
        transform.rotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, transform.position + newVelocityVector) * deltaRotation;
    }
    private void TurnTowardsTarget()
    {
        if (target)
        {
            Quaternion deltaRotation = Quaternion.Euler(0, 0, -90);// Weird thing with rockets
            Quaternion targetRotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, target.transform.position) * deltaRotation;
            float deltaAngleThisFrame = rocketRotationSpeed * Time.deltaTime;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, deltaAngleThisFrame);
        }
    }
    #endregion

    #region Accessor/Mutator Methods
    public void SetTarget(GameObject target)
    {
        this.target = target;
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
    private float GetRocketDirection()
    {
        Quaternion deltaRotation = Quaternion.Euler(0, 0, 90); // Weird thing with rockets
        return (transform.rotation * deltaRotation).eulerAngles.z;
    }
    #endregion
}

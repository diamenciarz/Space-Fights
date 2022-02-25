using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGunRotator : TeamUpdater
{
    #region Serializable
    [Header("Gun stats")]
    [Tooltip("Delta angle from the middle of parent's rotation")]
    [SerializeField] float basicGunDirection;

    [Header("Turret stats")]
    [Tooltip("In degrees per second")]
    [SerializeField] float gunRotationSpeed;
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;

    [Header("Instances")]
    [SerializeField] [Tooltip("For forward orientation and team setup")] GameObject parentGameObject;
    [SerializeField] VisualDetector[] visualDetectors;

    [Header("Debug zone")]
    [SerializeField] bool debugZoneOn = true;
    [SerializeField] GameObject debugZonePrefab;
    [SerializeField] Transform DebugZoneTransform;
    #endregion

    public bool areTargetsInRange;
    public float invisibleTargetRotation;
    private Coroutine randomRotationCoroutine;
    private ProgressionBarController debugZoneScript;
    //Instances
    public GameObject closestTarget;

    private void Start()
    {
        CreateDebugZone();
    }
    protected void Update()
    {
        LookForTarget();
        Rotate();
    }
    private void Rotate()
    {
        if (areTargetsInRange)
        {
            StopRandomRotationCoroutine();
        }
        else
        {
            CreateRandomRotationCoroutine();
        }
        RotateOneStepTowardsTarget();
    }
    private void LookForTarget()
    {
        List<GameObject> targets = GetDetectedTargets();
        closestTarget = StaticDataHolder.GetClosestObjectAngleWise(targets, transform.position, GetGunAngle());
        if (closestTarget)
        {
            areTargetsInRange = true;
        }
        else
        {
            areTargetsInRange = false;
        }
    }

    #region Random Rotation
    private void CreateRandomRotationCoroutine()
    {
        if (randomRotationCoroutine == null)
        {
            float deltaAngleFromTheMiddle = GetGunToMiddleAngle();
            invisibleTargetRotation = deltaAngleFromTheMiddle;
            randomRotationCoroutine = StartCoroutine(RotateRandomly());
        }
    }
    private void StopRandomRotationCoroutine()
    {
        if (randomRotationCoroutine != null)
        {
            StopCoroutine(randomRotationCoroutine);
            randomRotationCoroutine = null;
        }
    }
    private IEnumerator RotateRandomly()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 8));
            if (!areTargetsInRange)
            {
                GenerateNewInvisibleTargetAngle();
            }
        }
    }
    private void GenerateNewInvisibleTargetAngle()
    {
        invisibleTargetRotation = Random.Range(-leftMaxRotationLimit, rightMaxRotationLimit);
    }
    /// <summary>
    /// Returns angle from gun to the generated invisible target
    /// </summary>
    /// <returns></returns>
    private float GetAngleToInvisibleTarget()
    {
        float deltaAngleFromTheMiddle = GetGunToMiddleAngle();
        float angleFromGunToItem = Mathf.DeltaAngle(deltaAngleFromTheMiddle, invisibleTargetRotation);
        return angleFromGunToItem;
    }
    #endregion

    #region Movement
    private void RotateOneStepTowardsTarget()
    {
        float degreesToRotateThisFrame = CountAngleToRotateThisFrameBy();
        RotateBy(degreesToRotateThisFrame);
    }

    private void RotateBy(float angle)
    {
        transform.rotation *= Quaternion.Euler(0, 0, angle);
    }
    #endregion

    #region Rotation towards target
    private float CountAngleToRotateThisFrameBy()
    {
        float zMoveAngle = GetTargetAngle();
        //Clamp by gun rotation speed and frame rate
        float degreesToRotateThisFrame = Mathf.Clamp(zMoveAngle, -gunRotationSpeed * Time.deltaTime, gunRotationSpeed * Time.deltaTime);
        return degreesToRotateThisFrame;
    }
    private float GetTargetAngle()
    {
        float angleToTarget;
        if (areTargetsInRange)
        {
            angleToTarget = CountDeltaAngleToTarget();
        }
        else
        {
            angleToTarget = GetAngleToInvisibleTarget();
        }
        return CountAngleToTarget(angleToTarget);
    }
    private float CountAngleToTarget(float angleToTarget)
    {
        if (hasRotationLimits)
        {
            angleToTarget = GoAroundBoundaries(angleToTarget);
        }
        if (debugZoneOn)
        {
            UpdateDebugZone(GetGunAngle(), GetGunAngle() + angleToTarget);
        }
        return angleToTarget;
    }
    #region Get Delta Angle
    private float CountDeltaAngleToTarget()
    {
        if (closestTarget)
        {
            Vector3 targetPosition = closestTarget.transform.position;
            return CountAngleFromGunToTargetPosition(targetPosition);
        }
        else
        {
            //There is no target to turn towards
            return 0;
        }
    }
    private float CountAngleFromGunToTargetPosition(Vector3 targetPosition)
    {
        Vector3 relativePositionFromGunToItem = HelperMethods.DeltaPosition(transform.position, targetPosition);
        float angleFromMiddleToItem = CountAngleFromMiddleToPosition(relativePositionFromGunToItem);
        if (hasRotationLimits)
        {
            angleFromMiddleToItem = ClampAngleToBoundaries(angleFromMiddleToItem);
        }

        float angleFromGunToItem = Mathf.DeltaAngle(GetGunToMiddleAngle(), angleFromMiddleToItem);

        return angleFromGunToItem;
    }
    private float ClampAngleToBoundaries(float angleFromMiddleToItem)
    {
        if (angleFromMiddleToItem < -rightMaxRotationLimit)
        {
            angleFromMiddleToItem = -rightMaxRotationLimit;
        }
        if (angleFromMiddleToItem > leftMaxRotationLimit)
        {
            angleFromMiddleToItem = leftMaxRotationLimit;
        }
        return angleFromMiddleToItem;
    }
    private float CountAngleFromMiddleToPosition(Vector3 relativePositionFromGunToItem)
    {
        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float middleZRotation = GetMiddleAngle();
        float angleFromMiddleToItem = angleFromZeroToItem - middleZRotation;

        if (angleFromMiddleToItem < -180)
        {
            angleFromMiddleToItem += 360;
        }
        return angleFromMiddleToItem;
    }
    #endregion

    #region Go Around Boundaries
    private float GoAroundBoundaries(float angleToMove)
    {
        if (angleToMove > 0)
        {
            return GoAroundLeftBoundary(angleToMove);
        }
        if (angleToMove < 0)
        {
            return GoAroundRightBoundary(angleToMove);
        }
        return angleToMove;
    }
    private float GoAroundLeftBoundary(float angleToMove)
    {
        float angleFromGunToLeftLimit = CountAngleFromGunToLeftLimit();
        if (angleFromGunToLeftLimit >= 0)
        {
            if (angleToMove > angleFromGunToLeftLimit)
            {
                return (angleToMove - 360);
            }
        }
        return angleToMove;
    }
    private float GoAroundRightBoundary(float angleToMove)
    {
        float angleFromGunToRightLimit = CountAngleFromGunToRightLimit();
        if (angleFromGunToRightLimit <= 0)
        {
            if (angleToMove < angleFromGunToRightLimit)
            {
                return (angleToMove + 360);
            }
        }
        return angleToMove;
    }
    private float CountAngleFromGunToLeftLimit()
    {
        float angleFromGunToLeftLimit = leftMaxRotationLimit - GetGunToMiddleAngle();
        if (angleFromGunToLeftLimit > 180)
        {
            angleFromGunToLeftLimit -= 360;
        }
        return angleFromGunToLeftLimit;
    }
    private float CountAngleFromGunToRightLimit()
    {
        float angleFromGunToRightLimit = -(rightMaxRotationLimit + GetGunToMiddleAngle());
        if (angleFromGunToRightLimit < -180)
        {
            angleFromGunToRightLimit += 360;
        }
        return angleFromGunToRightLimit;
    }
    #endregion

    #endregion

    #region GetValues
    /// <summary>
    /// Delta angle between the gun and the middle of the shooting zone
    /// </summary>
    /// <returns></returns>
    private float GetGunToMiddleAngle()
    {
        return GetGunAngle() - GetMiddleAngle();
    }
    /// <summary>
    /// Angle from zero (up) to middle of the gun shooting area
    /// </summary>
    /// <returns></returns>
    private float GetMiddleAngle()
    {
        float middleAngle = parentGameObject.transform.rotation.eulerAngles.z + basicGunDirection;
        if (middleAngle > 180)
        {
            middleAngle -= 360;
        }
        if (middleAngle < -180)
        {
            middleAngle += 360;
        }
        return middleAngle;
    }
    /// <summary>
    /// Angle from zero (up) to the gun
    /// </summary>
    /// <returns></returns>
    private float GetGunAngle()
    {
        Quaternion gunRotation = transform.rotation;
        float gunAngle = gunRotation.eulerAngles.z;

        if (gunAngle > 180)
        {
            gunAngle -= 360;
        }

        return gunAngle;
    }
    private List<GameObject> GetDetectedTargets()
    {
        List<GameObject> detectedTargets = new List<GameObject>();
        foreach (VisualDetector detector in visualDetectors)
        {
            GameObject target = detector.GetClosestTarget();
            if (target)
            {
                detectedTargets.Add(target);
            }
        }
        return detectedTargets;
    }
    #endregion

    #region UpdateUI
    private void UpdateDebugZone(float startAngle, float endAngle)
    {
        if (!debugZoneScript)
        {
            CreateDebugZone();
        }

        float parentAngle = parentGameObject.transform.rotation.eulerAngles.z;
        float angleSize = startAngle - endAngle;
        //Debug.Log(startAngle + ", " + endAngle);
        if (angleSize < 0)
        {
            debugZoneScript.UpdateProgressionBar(-angleSize, 360);
            float shootingZoneRotation = endAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        }
        else
        {
            debugZoneScript.UpdateProgressionBar(angleSize, 360);
            float shootingZoneRotation = startAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        }
    }
    private void CreateDebugZone()
    {
        if (debugZonePrefab != null)
        {
            GameObject newShootingZoneGo = Instantiate(debugZonePrefab, DebugZoneTransform);
            float debugZoneRange = 1.8f; // This is a constant
            newShootingZoneGo.transform.localScale = new Vector3(debugZoneRange, debugZoneRange, 1);

            SetupDebugZone(newShootingZoneGo);
        }
    }
    private void SetupDebugZone(GameObject newShootingZoneGo)
    {
        debugZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        debugZoneScript.SetObjectToFollow(DebugZoneTransform.gameObject);
    }
    #endregion
}
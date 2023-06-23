using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static StaticDataHolder.ListContents;

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
    [SerializeField] bool predictTargetVelocity = true;

    [Header("Instances")]
    [SerializeField] VisualDetector[] visualDetectors;

    [Header("Debug zone")]
    [SerializeField] bool debugZoneOn = true;
    [SerializeField] Transform DebugZoneTransform;
    #endregion

    private bool areTargetsInRange;
    private float invisibleTargetRotation;
    private Coroutine randomRotationCoroutine;
    private ProgressionBarController debugZoneScript;
    //Instances
    private IParent parent;
    private GameObject parentGameObject;
    private GameObject closestTarget;
    private GunController shootingController;

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetStartingVariables();
    }
    private void SetStartingVariables()
    {
        parent = GetComponentInParent<IParent>();
        if (parent != null)
        {
            parentGameObject = parent.GetGameObject();
        }
        shootingController = GetComponentInChildren<GunController>();
    }
    #endregion

    #region Update
    protected void Update()
    {
        if (!parentGameObject)
        {
            return;
        }
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
        closestTarget = Generic.GetClosestObjectAngleWise(targets, transform.position, GetGunAngle());
        if (closestTarget)
        {
            areTargetsInRange = true;
        }
        else
        {
            areTargetsInRange = false;
        }
    }
    #endregion

    #region Random Rotation
    private void CreateRandomRotationCoroutine()
    {
        if (randomRotationCoroutine == null)
        {
            float deltaAngleFromTheMiddle = GetFromGunToMiddleAngle();
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
        float deltaAngleFromTheMiddle = GetFromGunToMiddleAngle();
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
        float rotationThisFrame = gunRotationSpeed * Time.deltaTime;
        float degreesToRotateThisFrame = Mathf.Clamp(zMoveAngle, -rotationThisFrame, rotationThisFrame);
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
        float angleAroundBoundary = angleToTarget;
        if (hasRotationLimits)
        {
            angleAroundBoundary = GoAroundBoundaries(angleToTarget);

        }
        if (debugZoneOn)
        {
            UpdateDebugZone(GetGunAngle(), GetGunAngle() + angleAroundBoundary);
        }
        return angleAroundBoundary;
    }
    #region Get Delta Angle
    private float CountDeltaAngleToTarget()
    {
        if (closestTarget)
        {
            Vector3 targetPosition = GetTargetPosition();
            return CountAngleFromGunToTargetPosition(targetPosition);
        }
        else
        {
            //There is no target to turn towards
            return 0;
        }
    }
    private Vector2 GetTargetPosition()
    {
        if (predictTargetVelocity)
        {
            return PredictTargetPosition();
        }
        else
        {
            return closestTarget.transform.position;
        }
    }
    /// <summary>
    /// Iteratively predicts the future position of a target. Increase the constant for more calculation steps
    /// </summary>
    private Vector2 PredictTargetPosition()
    {
        if (shootingController == null)
        {
            return Vector2.zero;
        }

        SalvoScriptableObject.Shot shot = GetShot();
        EntityCreator.Projectiles projectileType = shot.shot.projectilesToCreateList[0];
        GameObject entityToSummon = EntityFactory.GetPrefab(projectileType);
        ProjectileController projectileController = entityToSummon.GetComponent<ProjectileController>();

        if (projectileController == null)
        {
            return Vector2.zero;
        }

        float myProjectileSpeed = projectileController.GetStartingSpeed();
        return HelperMethods.ObjectUtils.PredictTargetPositionUponHit(transform.position, closestTarget, myProjectileSpeed);
    }
    private SalvoScriptableObject.Shot GetShot()
    {
        int index = shootingController.shotIndex;
        if (index >= shootingController.salvo.shots.Length)
        {
            index -= 1;
        }
        return shootingController.salvo.shots[index];
    }
    private float CountAngleFromGunToTargetPosition(Vector3 targetPosition)
    {
        Vector3 relativePositionFromGunToItem = HelperMethods.VectorUtils.DeltaPosition(transform.position, targetPosition);
        float angleFromMiddleToItem = CountAngleFromMiddleToPosition(relativePositionFromGunToItem);
        if (hasRotationLimits)
        {
            angleFromMiddleToItem = ClampAngleToBoundaries(angleFromMiddleToItem);
        }

        float angleFromGunToItem = Mathf.DeltaAngle(GetFromGunToMiddleAngle(), angleFromMiddleToItem);

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
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float middleZRotation = GetMiddleAngle();
        float angleFromMiddleToItem = angleFromZeroToItem - middleZRotation;

        return HelperMethods.AngleUtils.ClampAngle180(angleFromMiddleToItem);
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
            // Due to two float angles being added, and then compared, an overflow occurred,
            // which would make equal values appear different by a very small fraction.
            // This did cause weird behaviour at the right boundary.
            // Subtracting 0.01 removes the error
            if (angleToMove < angleFromGunToRightLimit - 0.001f)
            {
                return (angleToMove + 360);
            }
        }
        return angleToMove;
    }
    private float CountAngleFromGunToLeftLimit()
    {
        float fromGunToMiddleAngle = GetFromGunToMiddleAngle();
        float angleFromGunToLeftLimit = leftMaxRotationLimit - fromGunToMiddleAngle;
        if (angleFromGunToLeftLimit > 180)
        {
            angleFromGunToLeftLimit -= 360;
        }
        return angleFromGunToLeftLimit;
    }
    private float CountAngleFromGunToRightLimit()
    {
        float fromGunToMiddleAngle = GetFromGunToMiddleAngle();
        float angleFromGunToRightLimit = -(rightMaxRotationLimit + fromGunToMiddleAngle);
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
    private float GetFromGunToMiddleAngle()
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
        return HelperMethods.AngleUtils.ClampAngle180(middleAngle);
    }
    /// <summary>
    /// Angle from zero (up) to the gun
    /// </summary>
    /// <returns></returns>
    private float GetGunAngle()
    {
        return HelperMethods.AngleUtils.ClampAngle180(transform.rotation.eulerAngles.z);
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

    #region DebugZone
    private void UpdateDebugZone(float startAngle, float endAngle)
    {
        if (!debugZoneScript)
        {
            CreateDebugZone();
        }

        float parentAngle = parentGameObject.transform.rotation.eulerAngles.z;
        float angleSize = startAngle - endAngle;
        if (angleSize < 0)
        {
            float ratio = -angleSize / 360f;
            debugZoneScript.UpdateProgressionBar(ratio);
            float shootingZoneRotation = endAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(shootingZoneRotation);
        }
        else
        {
            float ratio = angleSize / 360f;
            debugZoneScript.UpdateProgressionBar(ratio);
            float shootingZoneRotation = startAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(shootingZoneRotation);
        }
    }
    private void CreateDebugZone()
    {
        GameObject debugZonePrefab = EntityFactory.GetPrefab(EntityCreator.ProgressionCones.ShootingZone);
        GameObject newShootingZoneGo = Instantiate(debugZonePrefab, DebugZoneTransform);
        float debugZoneRange = 5f; // This is a constant
        newShootingZoneGo.transform.localScale = new Vector3(debugZoneRange, debugZoneRange, 1);

        SetupDebugZone(newShootingZoneGo);
    }
    private void SetupDebugZone(GameObject newShootingZoneGo)
    {
        debugZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        debugZoneScript.SetObjectToFollow(DebugZoneTransform.gameObject);
    }
    #endregion

    #endregion
}
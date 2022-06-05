using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDetector : TeamUpdater
{
    #region Serialization
    [Tooltip("Delta angle from the middle of parent's rotation")]
    [SerializeField] float basicGunDirection;

    [Header("Visual Zone")]
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [Tooltip("The visual range of the camera. Choose 0 for infinite range")]
    [SerializeField] float range = 10f;
    [Tooltip("The click range of the camera if overridden by mouse cursor. Choose 0 for infinite range")]
    [SerializeField] float mouseRange = 10f;
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField] bool targetObstacles = false;

    [Header("Mouse Steering")]
    [SerializeField] bool controlledByMouse;
    [SerializeField] bool isShootingZoneOn;
    [SerializeField] bool ignoreMouseCollisions;

    [Header("Instances")]
    [SerializeField] ShootingController[] shootingControllers;

    [Header("Visual Zone")]
    [SerializeField] GameObject visualZonePrefab;
    [SerializeField] Transform visualZoneTransform;
    #endregion

    private GameObject currentTarget;
    private List<GameObject> targetsInSightList = new List<GameObject>();
    private ProgressionBarController shootingZoneScript;

    private bool savedIsShootingZoneOn;
    private float savedLeftMaxRotationLimit;
    private float savedRightMaxRotationLimit;
    private float savedBasicGunDirection;
    private Coroutine checkCoroutine;
    private bool isTargetInSight;

    void Start()
    {
        checkCoroutine = StartCoroutine(VisualCheckCoroutine());
    }
    private void Update()
    {
        UpdateUI();
    }

    #region Check coroutine
    private IEnumerator VisualCheckCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshRate);
            DoChecks();
            SetShooting();
        }
    }
    #region Shooting behaviour
    private void SetShooting()
    {
        if (isTargetInSight)
        {
            UpdateShootingControllers(true);
        }
        else
        {
            UpdateShootingControllers(false);
        }
    }
    private void UpdateShootingControllers(bool shoot)
    {
        foreach (var item in shootingControllers)
        {
            item.SetShoot(shoot);
        }
    }
    #endregion
    private void DoChecks()
    {
        //Get all targets list
        targetsInSightList = FindAllEnemiesInSight();
        //The closest target
        currentTarget = StaticDataHolder.GetClosestObjectInSightAngleWise(targetsInSightList, transform.position, GetGunAngle());
        //Are there any targets in sight (edge case for mouse cursor)
        CheckForAnyTargetInSight();
    }
    #endregion

    #region Checks
    private List<GameObject> FindAllEnemiesInSight()
    {
        List<GameObject> targetList = StaticDataHolder.GetEnemyList(team);
        if (targetObstacles)
        {
            targetList.AddRange(StaticDataHolder.GetObstacleList());
        }
        if (targetList.Count == 0)
        {
            return null;
        }

        List<GameObject> targetsInSight = new List<GameObject>();
        foreach (GameObject target in targetList)
        {
            //I expect enemyList to never have a single null value
            if (CanSeeTarget(target))
            {
                targetsInSight.Add(target);
            }
        }
        return targetsInSight;
    }
    private void CheckForAnyTargetInSight()
    {
        if (controlledByMouse)
        {
            isTargetInSight = IsMouseInSight();
        }
        else
        {
            if (targetsInSightList.Count > 0)
            {
                isTargetInSight = true;
            }
            else
            {
                isTargetInSight = false;
            }
        }
    }
    private bool IsMouseInSight()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 mousePosition = HelperMethods.VectorUtils.TranslatedMousePosition(transform.position);
            return CanSeePosition(mousePosition, ignoreMouseCollisions);
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Helper methods
    private bool IsPositionInCone(Vector3 targetPosition, float range)
    {
        if (IsPositionInRange(targetPosition, range))
        {
            float angleFromZeroToItem = HelperMethods.AngleUtils.AngleFromUpToPosition(transform.position, targetPosition);
            float angleFromMiddleToItem = Mathf.DeltaAngle(GetMiddleAngle(), angleFromZeroToItem);

            bool isCursorInCone = angleFromMiddleToItem > -(rightMaxRotationLimit) && angleFromMiddleToItem < (leftMaxRotationLimit);
            if (isCursorInCone)
            {
                return true;
            }
        }
        return false;
    }
    private bool IsPositionInRange(Vector3 targetPosition, float range)
    {
        float distanceToTarget = HelperMethods.VectorUtils.Distance(transform.position, targetPosition);
        bool canShoot = range > distanceToTarget || range == 0;
        if (canShoot)
        {
            return true;
        }
        return false;
    }
    private float CountAngleToPosition(Vector3 targetPosition)
    {
        float angleFromZeroToItem = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, targetPosition).eulerAngles.z;
        float angleFromGunToItem = Mathf.DeltaAngle(GetGunAngle(), angleFromZeroToItem);

        return angleFromGunToItem;
    }
    #endregion

    #region Count values
    private float GetMiddleAngle()
    {
        float middleAngle = transform.rotation.eulerAngles.z + basicGunDirection;
        return middleAngle;
    }
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
    #endregion

    #region UI
    private void UpdateUI()
    {
        UpdateUIState();
        UpdateShootingZoneVisibility();
    }
    private void UpdateUIState()
    {
        if (isShootingZoneOn)
        {
            CreateGunShootingZone();
        }
        else
        {
            DeleteGunShootingZone();
        }
        bool shootingZoneWasModified = (savedIsShootingZoneOn != hasRotationLimits) || (savedLeftMaxRotationLimit != leftMaxRotationLimit) ||
        (savedRightMaxRotationLimit != rightMaxRotationLimit) || (savedBasicGunDirection != basicGunDirection);
        if (shootingZoneWasModified)
        {
            RefreshGunShootingZone();
        }
    }
    private void UpdateShootingZoneVisibility()
    {
        if (shootingZoneScript != null)
        {
            if (isTargetInSight)
            {
                //Make the light orange bar disappear
                shootingZoneScript.SetIsAlwaysVisible(false);
            }
            else
            {
                //Make the light orange bar show up
                shootingZoneScript.SetIsAlwaysVisible(true);
            }
        }
    }

    #region Create/Delete UI
    private void RefreshGunShootingZone()
    {
        savedIsShootingZoneOn = hasRotationLimits;
        savedLeftMaxRotationLimit = leftMaxRotationLimit;
        savedRightMaxRotationLimit = rightMaxRotationLimit;
        savedBasicGunDirection = basicGunDirection;
        DeleteGunShootingZone();
        CreateGunShootingZone();
    }
    private void DeleteGunShootingZone()
    {
        if (shootingZoneScript != null)
        {
            Destroy(shootingZoneScript.gameObject);
        }
    }
    private void CreateGunShootingZone()
    {
        if (visualZonePrefab != null && shootingZoneScript == null)
        {
            GameObject newShootingZoneGo = Instantiate(visualZonePrefab, visualZoneTransform);

            float xScale = GetCurrentRange() / newShootingZoneGo.transform.lossyScale.x;
            float yScale = GetCurrentRange() / newShootingZoneGo.transform.lossyScale.y;
            newShootingZoneGo.transform.localScale = new Vector3(xScale, yScale, 1);

            SetupShootingZoneShape(newShootingZoneGo);
        }
    }
    private void SetupShootingZoneShape(GameObject newShootingZoneGo)
    {
        shootingZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        if (hasRotationLimits)
        {
            float ratio = (leftMaxRotationLimit + rightMaxRotationLimit) / 360f;
            shootingZoneScript.UpdateProgressionBar(ratio);
        }
        else
        {
            shootingZoneScript.UpdateProgressionBar(1);
        }
        shootingZoneScript.SetObjectToFollow(visualZoneTransform.gameObject);
        float shootingZoneRotation = basicGunDirection + leftMaxRotationLimit;
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
    }
    #endregion

    #endregion

    #region Accessor methods
    /// <summary>
    /// Checks, whether this detector can directly see this position
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="ignoreCollisions"></param>
    /// <returns></returns>
    public bool CanSeePosition(Vector3 targetPosition, bool ignoreCollisions = false)
    {
        if (ignoreCollisions || HelperMethods.LineOfSightUtils.CanSeeDirectly(transform.position, targetPosition))
        {
            //Mouse can not hide behind a bush
            if (hasRotationLimits)
            {
                return IsPositionInCone(targetPosition, GetCurrentRange());
            }
            else
            {
                return IsPositionInRange(targetPosition, GetCurrentRange());
            }
        }
        return false;
    }
    public bool CanSeeTarget(GameObject target)
    {
        if (HelperMethods.LineOfSightUtils.CanSeeDirectly(transform.position, target))
        {
            if (hasRotationLimits)
            {
                return IsPositionInCone(target.transform.position, GetCurrentRange());
            }
            else
            {
                return IsPositionInRange(target.transform.position, GetCurrentRange());
            }
        }
        return false;
    }
    /// <summary>
    /// Returns the current closest target that this detector can see. (The closest target angle wise)
    /// </summary>
    /// <returns></returns>
    public GameObject GetClosestTarget()
    {
        return currentTarget;
    }
    /// <summary>
    /// Returns all targets that this detector can see
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetTargetsInSight()
    {
        return targetsInSightList;
    }
    /// <summary>
    /// Returns true if the detector can see at least one target
    /// </summary>
    /// <returns></returns>
    public bool CanSeeTargets()
    {
        return isTargetInSight;
    }
    public float GetCurrentRange()
    {
        if (controlledByMouse)
        {
            return mouseRange;
        }
        else
        {
            return range;
        }
    }
    #endregion
}

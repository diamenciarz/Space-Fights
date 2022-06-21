using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperMethods.LineOfSightUtils;
using static HelperMethods.VectorUtils;
using static StaticDataHolder.ListContents;

public class VisualDetector : TeamUpdater, IProgressionBarCompatible
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

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouse;
    [SerializeField] bool isShootingZoneOn;
    [SerializeField] bool ignoreMouseCollisions;

    [Header("Targeting settings")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;

    [Header("Instances")]
    [SerializeField] ShootingController[] shootingControllers;

    [Header("Visual Zone")]
    [SerializeField] Transform visualZoneTransform;
    #endregion

    public GameObject currentTarget;
    private List<GameObject> targetsInSightList = new List<GameObject>();
    private bool isTargetInSight;
    public bool wasShootingZoneModified;
    private Coroutine checkCoroutine;


    #region Startup
    void Start()
    {
        SetStartingVariables();
        checkCoroutine = StartCoroutine(VisualCheckCoroutine());
    }
    private void SetStartingVariables()
    {
        TryCreateUI();
    }
    #endregion

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
            FindCurrentTarget();
            CheckForAnyTargetInSight();
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
            item.SetTarget(currentTarget);
        }
    }
    #endregion
    private void FindCurrentTarget()
    {
        targetsInSightList = FindAllEnemiesInSight();
        TryRemoveObstacles();

        LayerNames[] layers = HelperMethods.ObjectUtils.GetLayers(targetTypes);
        currentTarget = Generic.GetClosestObjectInSightAngleWise(targetsInSightList, transform.position, GetGunAngle(), layers);
    }
    #endregion

    #region Checks
    private List<GameObject> FindAllEnemiesInSight()
    {
        List<GameObject> potentialTargets = Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);
        if (potentialTargets.Count == 0)
        {
            return new List<GameObject>();
        }

        List<GameObject> targetsInSight = new List<GameObject>();
        foreach (GameObject target in potentialTargets)
        {
            //I expect enemyList to never have a single null value
            if (CanSeeTarget(target))
            {
                targetsInSight.Add(target);
            }
        }
        return targetsInSight;
    }
    /// <summary>
    /// If you see a target that is not an obstacle, focus that target.
    /// </summary>
    private void TryRemoveObstacles()
    {
        if (Generic.ListContainsNonObstacle(targetsInSightList))
        {
            StaticDataHolder.ListModification.DeleteObstacles(targetsInSightList);
        }
    }
    /// <summary>
    ///Are there any targets in sight (edge case for mouse cursor)
    /// </summary>
    private void CheckForAnyTargetInSight()
    {
        if (isControlledByMouse)
        {
            isTargetInSight = IsMouseInSight();
            return;
        }
        isTargetInSight = targetsInSightList.Count > 0;
    }
    private bool IsMouseInSight()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 mousePosition = TranslatedMousePosition(transform.position);
            return CanSeePosition(mousePosition, ignoreMouseCollisions);
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Count values
    private float GetMiddleAngle()
    {
        return transform.rotation.eulerAngles.z + basicGunDirection;
    }
    private float GetGunAngle()
    {
        return HelperMethods.AngleUtils.ClampAngle180(transform.rotation.eulerAngles.z);
    }
    #endregion

    #region UI
    private void TryCreateUI()
    {
        bool createBar = hasRotationLimits && isShootingZoneOn;
        if (createBar)
        {
            float shootingZoneRotation = basicGunDirection + leftMaxRotationLimit;
            StaticProgressionBarUpdater.CreateProgressionCone(this, GetCurrentRange(), shootingZoneRotation);
        }
    }
    #region Update
    private void UpdateUI()
    {
        if (wasShootingZoneModified)
        {
            wasShootingZoneModified = false;
            RefreshUI();
        }
        UpdateShootingZoneVisibility();
    }
    private void RefreshUI()
    {
        StaticProgressionBarUpdater.DeleteProgressionCone(this);
        TryCreateUI();
    }
    #endregion
   
    private void UpdateShootingZoneVisibility()
    {
        bool barOn = hasRotationLimits && isShootingZoneOn && isControlledByMouse;
        if (!barOn)
        {
            return;
        }
        if (isTargetInSight)
        {
            //Make the light orange bar disappear
            StaticProgressionBarUpdater.SetIsProgressionConeAlwaysVisible(this, false);
        }
        else
        {
            //Make the light orange bar show up
            StaticProgressionBarUpdater.SetIsProgressionConeAlwaysVisible(this, true);
        }
    }
    #endregion

    #region Accessor methods
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public float GetBarRatio()
    {
        if (hasRotationLimits)
        {
            return (leftMaxRotationLimit + rightMaxRotationLimit) / 360f;
        }
        else
        {
            return 1;
        }
    }
    /// <summary>
    /// Checks, whether this detector can directly see this position
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="ignoreCollisions"></param>
    /// <returns></returns>
    public bool CanSeePosition(Vector3 targetPosition, bool ignoreCollisions = false)
    {
        if (ignoreCollisions || CanSeeDirectly(transform.position, targetPosition))
        {
            //Mouse can not hide behind a bush
            if (hasRotationLimits)
            {
                Cone cone = new Cone(transform.position, GetMiddleAngle(), leftMaxRotationLimit, rightMaxRotationLimit, GetCurrentRange());
                return IsPositionInCone(targetPosition, cone);
            }
            else
            {
                return IsPositionInRange(transform.position, targetPosition, GetCurrentRange());
            }
        }
        return false;
    }
    public bool CanSeeTarget(GameObject target)
    {
        if (CanSeeDirectly(transform.position, target))
        {
            if (hasRotationLimits)
            {
                Cone cone = new Cone(transform.position, GetMiddleAngle(), leftMaxRotationLimit, rightMaxRotationLimit, range);
                return IsPositionInCone(target.transform.position, cone);
            }
            else
            {
                return IsPositionInRange(transform.position, target.transform.position, GetCurrentRange());
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
        if (isControlledByMouse)
        {
            if (mouseRange == 0)
            {
                return 1000;
            }
            return mouseRange;
        }
        else
        {
            if (range == 0)
            {
                return 1000;
            }
            return range;
        }
    }
    #endregion

    #region Mutator methods
    public void SetIsControlledByMouse(bool set)
    {
        isControlledByMouse = set;
        wasShootingZoneModified = true;
    }
    public void SetHasRotationLimits(bool set)
    {
        hasRotationLimits = set;
        wasShootingZoneModified = true;
    }
    public void SetLeftRotationLimit(float limit)
    {
        leftMaxRotationLimit = limit;
        wasShootingZoneModified = true;
    }
    public void SetRightRotationLimit(float limit)
    {
        rightMaxRotationLimit = limit;
        wasShootingZoneModified = true;
    }
    public void SetMouseRange(float range)
    {
        mouseRange = range;
        wasShootingZoneModified = true;
    }
    public void SetRange(float range)
    {
        this.range = range;
        wasShootingZoneModified = true;
    }
    public void SetShootingZoneOn(bool isOn)
    {
        isShootingZoneOn = isOn;
        wasShootingZoneModified = true;
    }
    #endregion
}

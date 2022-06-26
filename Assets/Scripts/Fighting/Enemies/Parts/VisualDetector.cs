using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static HelperMethods.LineOfSightUtils;
using static HelperMethods.VectorUtils;
using static StaticDataHolder.ListContents;

public class VisualDetector : TeamUpdater, IProgressionBarCompatible
{
    #region Serialization
    [Tooltip("Delta angle from the middle of parent's rotation")]
    [SerializeField] float deltaParentRotation;

    [Header("Visual Zone")]
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [Tooltip("The visual range of the camera. Choose 0 for infinite range")]
    [SerializeField] float range = 10f;
    [Tooltip("The click range of the camera if overridden by mouse cursor. Choose 0 for infinite range")]
    [SerializeField] float mouseRange = 10f;
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField] Transform visualZoneOrigin;

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouse;
    [SerializeField] bool isShootingZoneOn;
    [SerializeField] bool ignoreMouseCollisions;

    [Header("Targeting settings")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;

    [Header("Instances")]
    [SerializeField] ActionController[] actionControllers;
    #endregion

    private GameObject currentTarget;
    private List<GameObject> targetsInSightList = new List<GameObject>();
    private bool isTargetInSight;
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
        UpdateShootingZoneVisibility();
    }

    #region Check coroutine
    private IEnumerator VisualCheckCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshRate);
            UpdateCurrentTarget();
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
        ActionControllerData data = GetControllerData(shoot);
        foreach (var item in actionControllers)
        {
            item.UpdateController(data);
        }
    }
    private ActionControllerData GetControllerData(bool shoot)
    {
        ActionControllerData data = new ActionControllerData(shoot);
        data.target = currentTarget;
        return data;
    }
    #endregion

    #region Target search
    private void UpdateCurrentTarget()
    {
        if (isControlledByMouse && IsMouseInSight())
        {
            currentTarget = Generic.GetClosestMouseCursor(transform.position);
        }
        else
        {
            currentTarget = FindClosestTarget();
        }
    }
    private GameObject FindClosestTarget()
    {

        targetsInSightList = FindAllEnemiesInSight();
        TryRemoveObstacles();

        LayerNames[] layers = HelperMethods.ObjectUtils.GetLayers(targetTypes);
        return Generic.GetClosestObjectInSightAngleWise(targetsInSightList, transform.position, GetGunAngle(), layers);
    }
    private List<GameObject> FindAllEnemiesInSight()
    {
        List<GameObject> potentialTargets = Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);

        List<GameObject> targetsInSight = new List<GameObject>();
        LayerNames[] layers = HelperMethods.ObjectUtils.GetLayers(targetTypes);
        foreach (GameObject target in potentialTargets)
        {
            //I expect enemyList to never have a single null value
            if (CanSeeTarget(target, layers))
            {
                targetsInSight.Add(target);
            }
        }
        return targetsInSight;
    }
    /// <summary>
    /// Only target obstacles if you only see obstacles.
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
    #endregion

    #region Shooting check
    private void CheckForAnyTargetInSight()
    {
        if (isControlledByMouse && IsMouseInSight())
        {
            isTargetInSight = IsMouseClickedInSight();
        }
        else
        {
            isTargetInSight = targetsInSightList.Count > 0;
        }
    }
    private bool IsMouseClickedInSight()
    {
        return IsMouseInSight() && IsMouseClicked();
    }
    private bool IsMouseInSight()
    {
        Vector3 mousePosition = TranslatedMousePosition(transform.position);
        return CanSeePosition(mousePosition, ignoreMouseCollisions);
    }
    private bool IsMouseClicked()
    {
        return Input.GetKey(KeyCode.Mouse0);
    }
    #endregion
    #endregion

    #region Count values
    private float GetMiddleAngle()
    {
        return transform.rotation.eulerAngles.z + deltaParentRotation;
    }
    private float GetGunAngle()
    {
        return HelperMethods.AngleUtils.ClampAngle180(transform.rotation.eulerAngles.z);
    }
    #endregion

    #region UI
    private void TryCreateUI()
    {
        bool createBar = hasRotationLimits;
        if (createBar)
        {
            float shootingZoneRotation = deltaParentRotation + leftMaxRotationLimit;
            StaticProgressionBarUpdater.CreateProgressionCone(this, GetCurrentRange(), GetBarRatio(), shootingZoneRotation);
        }
    }
    #region Update
    private void RefreshUI()
    {
        StaticProgressionBarUpdater.DeleteProgressionCone(this);
        TryCreateUI();
    }
    #endregion

    private void UpdateShootingZoneVisibility()
    {
        bool barOn = hasRotationLimits && isControlledByMouse;
        if (!barOn)
        {
            return;
        }
        if (isTargetInSight)
        {
            //Debug.Log("Show cone");
            //Make the light orange bar show up
            StaticProgressionBarUpdater.SetIsProgressionConeAlwaysVisible(this, true);
        }
        else
        {
            //Make the light orange bar disappear
            StaticProgressionBarUpdater.SetIsProgressionConeAlwaysVisible(this, false);
        }
    }
    #endregion

    #region Accessor methods
    public Transform GetFollowTransform()
    {
        return visualZoneOrigin;
    }
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
    public bool CanSeeTarget(GameObject target, LayerNames[] layerNames)
    {
        if (CanSeeDirectly(transform.position, target, layerNames))
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
        RefreshUI();
    }
    public void SetHasRotationLimits(bool set)
    {
        hasRotationLimits = set;
        RefreshUI();
    }
    public void SetLeftRotationLimit(float limit)
    {
        leftMaxRotationLimit = limit;
        RefreshUI();
    }
    public void SetRightRotationLimit(float limit)
    {
        rightMaxRotationLimit = limit;
        RefreshUI();
    }
    public void SetMouseRange(float range)
    {
        mouseRange = range;
        RefreshUI();
    }
    public void SetRange(float range)
    {
        this.range = range;
        RefreshUI();
    }
    public void SetShootingZoneOn(bool isOn)
    {
        isShootingZoneOn = isOn;
        RefreshUI();
    }
    #endregion
}

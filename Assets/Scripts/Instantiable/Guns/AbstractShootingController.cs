using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractShootingController : ActionController, IProgressionBarCompatible, IPlayerControllable
{
    [Header("Instances")]
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] protected Transform shootingPoint;

    //[Header("Settings")]

    [Header("Mouse Steering")]
    [SerializeField] protected bool reloadingBarAlwaysOn = true;


    //Private variables
    protected GameObject cameraTarget;
    /// <summary>
    ///The gun tries to shoot, if this is set to true
    /// </summary>
    protected bool shoot;
    protected bool isDetached = false;
    protected float shootingTimeBank;
    protected float lastShotTime;
    protected bool canShoot;
    protected bool isControlledByMouse;

    protected GameObject targetFollower;
    protected GameObject targetFollowerChild;

    #region Initialization
    protected override void Start()
    {
        base.Start();
        InitializeStartingVariables();
        CallStartingMethods();
    }
    protected abstract void InitializeStartingVariables();
    protected abstract void CallStartingMethods();
    #endregion

    #region Update
    protected virtual void Update()
    {
        TryReload();
        UpdateController();
        TryShoot();
        MoveTargetFollower();
    }
    protected abstract void TryReload();
    protected void UpdateController()
    {
        shoot = IsControllerOn();

        if (shoot)
        {
            cameraTarget = GetActionControllerData().target;
        }
    }
    protected abstract void TryShoot();
    #endregion


    private void MoveTargetFollower()
    {
        GameObject target = GetTarget();
        if (target != null && targetFollowerChild != null)
        {
            targetFollowerChild.transform.position = target.transform.position;
        }
    }
    protected abstract GameObject GetTarget();

    #region UI
    protected virtual void ResetUIState()
    {
        ResetProgressionBar();
        // There can come more UI updates here
    }
    private void ResetProgressionBar()
    {
        if (isDetached || !isControlledByMouse)
        {
            StaticProgressionBarUpdater.DeleteProgressionBar(this);
            return;
        }
        StaticProgressionBarUpdater.CreateProgressionBar(this);
        if (reloadingBarAlwaysOn)
        {
            StaticProgressionBarUpdater.SetIsProgressionBarAlwaysVisible(this, true);
        }
        else
        {
            StaticProgressionBarUpdater.SetIsProgressionBarAlwaysVisible(this, false);
        }
    }
    #endregion

    #region Mutator methods
    public void SetTarget(GameObject newTarget)
    {
        cameraTarget = newTarget;
    }
    public void SetIsControlledByMouse(bool isTrue)
    {
        isControlledByMouse = isTrue;
        ResetUIState();
    }
    public void Detach()
    {
        isDetached = true;
        ResetUIState();
    }
    #endregion

    #region Accessor methods
    public Transform GetTransform()
    {
        return transform;
    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public GameObject GetShootingPoint()
    {
        return shootingPoint.gameObject;
    }
    public abstract float GetBarRatio();
    #endregion

}

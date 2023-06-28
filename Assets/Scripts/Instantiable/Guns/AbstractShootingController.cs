using System;
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

    protected TargetFollowers targetFollowers = new TargetFollowers();
    protected class TargetFollowers
    {
        private Dictionary<int,TargetFollower> targetFollowers = new Dictionary<int, TargetFollower>();
        private KeyValuePair<int,TargetFollower> activeTargetFollower = new KeyValuePair<int, TargetFollower>(-1, null);

        public void Reset()
        {
            foreach (KeyValuePair<int, TargetFollower> pair in targetFollowers)
            {
                pair.Value.DestroyFollowers();
            }
            targetFollowers = new Dictionary<int, TargetFollower>();
            activeTargetFollower = new KeyValuePair<int, TargetFollower>(-1, null);
        }
        public void AddTargetFollower(int index, TargetFollower targetFollower)
        {
            targetFollower.SetEnabled(false);
            targetFollowers.Add(index, targetFollower);
        }
        public void SetCurrentProjectile(int index)
        {
            if(activeTargetFollower.Key == index)
            {
                return;
            }
            if(activeTargetFollower.Value != null)
            {
                activeTargetFollower.Value.SetEnabled(false);
            }
            if(targetFollowers.ContainsKey(index))
            {
                activeTargetFollower = new KeyValuePair<int, TargetFollower>(index, targetFollowers[index]);
                activeTargetFollower.Value.SetEnabled(true);
            }
            else
            {
                activeTargetFollower = new KeyValuePair<int, TargetFollower>(-1, null);
            }
        }
        public void MoveActiveFollower(GameObject target)
        {
            if (activeTargetFollower.Value != null)
            {
                activeTargetFollower.Value.Move(target);
            }
        }
    }
    protected class TargetFollower
    {
        public TargetFollower(EntityCreator.ObjectFollowers objectFollower)
        {
            targetFollower = Instantiate(EntityFactory.GetPrefab(objectFollower), Vector3.zero, Quaternion.identity);
            targetFollowerChild = FindTargetFollowerChild(targetFollower);
            targetFollowerChild.transform.parent = null;

            followerRenderer = targetFollower.GetComponent<SpriteRenderer>();
            childRenderer = targetFollowerChild.GetComponent<SpriteRenderer>();

        }

        private GameObject targetFollower;
        private GameObject targetFollowerChild;

        private SpriteRenderer followerRenderer;
        private SpriteRenderer childRenderer;

        private GameObject FindTargetFollowerChild(GameObject targetFollower)
        {
            for (int i = 0; i < targetFollower.transform.childCount; i++)
            {
                GameObject child = targetFollower.transform.GetChild(i).gameObject;
                if (child.tag == "TargetFollower")
                {
                    return child;
                }
            }
            Debug.LogError("Target follower child not found. The target follower prefab is defined wrongly! The child should have tag \"TargetFollower\"");
            return null;
        }
        public void SetEnabled(bool set)
        {
            followerRenderer.enabled = set;
            childRenderer.enabled = set;
        }
        public void Move(GameObject target)
        {
            if (target != null && targetFollowerChild != null)
            {
                Vector3 newPosition = target.transform.position;
                newPosition.z = 1;
                targetFollowerChild.transform.position = newPosition;
            }
        }
        public void DestroyFollowers()
        {
            Destroy(targetFollower);
            Destroy(targetFollowerChild);
        }
    }

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
        cameraTarget = GetActionControllerData().target;
    }
    protected abstract void TryShoot();
    #endregion


    private void MoveTargetFollower()
    {
        targetFollowers.MoveActiveFollower(GetTarget());
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
        //Debug.Log("Detached " + gameObject.name);
        isDetached = true;
        shoot = false;
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

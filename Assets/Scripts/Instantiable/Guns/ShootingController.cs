using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static HelperMethods;
using static HelperMethods.LineOfSightUtils;

public class ShootingController : ActionController, IProgressionBarCompatible, IPlayerControllable
{
    [Header("Instances")]
    public SalvoScriptableObject salvo;
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] Transform shootingPoint;

    [Header("Settings")]
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float forwardGunRotation;
    [Tooltip("Describes how the gun will choose the direction of the projectiles coming out of its pipe")]
    [SerializeField] ShootingMode shootingMode;
    [Tooltip("Describes how the gun will give targets to its projectiles")]
    [SerializeField] ShootingMode targetMode;
    [Tooltip("Used if FindTarget mode is chosen")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;
    [Header("Mouse Steering")]
    bool isControlledByMouse; // Just in case
    [SerializeField] bool reloadingBarAlwaysOn = true;
    [Header("Progression bar compatibility")]
    [Tooltip("The progression bars and users should be a one-to-one match. If true, this script is not using GetGomponent<>() to find a ProgressionBarProperty.")]
    [SerializeField] bool dontUseProgressionBar;

    public enum ShootingMode
    {
        Forward,
        FindTarget,
        CameraTargeting
    }

    //Private variables
    private GameObject cameraTarget;
    /// <summary>
    ///The gun tries to shoot, if this is set to true
    /// </summary>
    protected bool shoot;
    private bool isDetached = false;
    private float shootingTimeBank;
    private float currentTimeBetweenEachShot;
    private float lastShotTime;
    [HideInInspector]
    public int shotIndex;
    private bool canShoot;
    private int shotAmount;
    private float salvoTimeSum;
    private GameObject targetFollower;
    private EntityCreator.ObjectFollowers currentFollowerType;

    #region Initialization
    protected override void Start()
    {
        base.Start();
        InitializeStartingVariables();
        CallStartingMethods();
    }
    private void InitializeStartingVariables()
    {
        lastShotTime = Time.time;
        salvoTimeSum = salvo.GetSalvoTimeSum();
        shootingTimeBank = salvoTimeSum;
        shotAmount = salvo.shots.Length;
        canShoot = true;
        shotIndex = 0;
        UpdateTimeBetweenEachShot();
    }
    private void CallStartingMethods()
    {
        UpdateUIState();
        UpdateTargetFollower();
    }
    #endregion

    #region Update
    protected virtual void Update()
    {
        CheckTimeBank();
        UpdateController();
        TryShoot();
    }
    private void UpdateController()
    {
        bool isOn = IsControllerOn();
        SetShoot(isOn);

        if (isOn)
        {
            SetTarget(GetActionControllerData().target);
        }
    }
    private void TryShoot()
    {
        if (!shoot)
        {
            return;
        }
        if (!canShoot)
        {
            return;
        }
        if (isDetached)
        {
            return;
        }
        bool hasAmmo = shotIndex <= shotAmount - 1;
        if (hasAmmo)
        {
            // This order is important! Cannot play sound before increasing the shot index!
            // Otherwise, the shot index is already shifted by +1 too early!
            canShoot = false;
            DoOneShot(shotIndex);

            StartCoroutine(WaitForNextShotCooldown(shotIndex));
            UpdateTimeBetweenEachShot();
            UpdateAmmoBar();

            shotIndex++;
            UpdateTargetFollower();
        }
    }
    #endregion

    #region Reloading
    private void CheckTimeBank()
    {
        if (salvo.reloadAllAtOnce)
        {
            TryReloadAllAmmo();
        }
        else
        {
            TryReloadOneBullet();
        }
    }
    private void TryReloadAllAmmo()
    {
        float reloadCooldown = salvo.additionalReloadTime + salvo.GetSalvoTimeSum(shotIndex - 1);
        float timeSinceLastShot = Time.time - lastShotTime;

        bool canReloadOneBullet = (timeSinceLastShot >= reloadCooldown) && (shotIndex > 0);
        if (canReloadOneBullet)
        {
            shootingTimeBank = salvoTimeSum;
            shotIndex = 0;
            UpdateTimeBetweenEachShot();
            UpdateAmmoBar();
            UpdateTargetFollower();
        }
    }
    private void TryReloadOneBullet()
    {
        if (shotIndex > 0)
        {
            float previousShotDelay = salvo.shots[shotIndex - 1].reloadDelay;
            float reloadCooldown = salvo.additionalReloadTime + previousShotDelay;
            float timeSinceLastShot = Time.time - lastShotTime;

            bool canReloadOneBullet = (timeSinceLastShot >= reloadCooldown) && (shotIndex > 0);
            if (canReloadOneBullet)
            {
                shootingTimeBank += previousShotDelay;
                shotIndex--;
                lastShotTime += previousShotDelay;
                UpdateTimeBetweenEachShot();
                UpdateAmmoBar();
                UpdateTargetFollower();
            }
        }
    }
    IEnumerator WaitForNextShotCooldown(int index)
    {
        float delay = salvo.shots[index].delayAfterShot;
        yield return new WaitForSeconds(delay);
        canShoot = true;
    }
    #endregion

    #region Shot Methods
    private void DoOneShot(int shotIndex)
    {
        PlayShotSound();
        DecreaseShootingTime();
        CreateShot(shotIndex);
        //Update time bank
    }
    private void CreateShot(int shotIndex)
    {
        SummonedShotData data = new SummonedShotData();
        data.summonRotation = GetRotation();
        data.summonPosition = shootingPoint.position;
        data.SetTeam(team);
        data.createdBy = createdBy;
        data.shot = salvo.shots[shotIndex].shot;
        data.target = GetTarget();
        EntityCreator.SummonShot(data);
    }
    private Quaternion GetRotation()
    {
        if (shootingMode == ShootingMode.Forward)
        {
            return transform.rotation * GetForwardGunRotation();
        }
        if (shootingMode == ShootingMode.CameraTargeting)
        {
            return GetRotationToTarget(cameraTarget);
        }
        {
            //shootingMode == ShootingMode.FindTarget
            GameObject foundTarget = FindTarget();
            return GetRotationToTarget(foundTarget);
        }
    }
    private GameObject GetTarget()
    {

        if (targetMode == ShootingMode.Forward)
        {
            return null;
        }
        if (targetMode == ShootingMode.CameraTargeting)
        {
            if (StaticDataHolder.ListContents.Generic.IsObjectMouseCursor(cameraTarget))
            {
                return FindClosestTargetToMouseCursor(cameraTarget.transform.position);
            }
            return cameraTarget;
        }
        // targetMode == ShootingMode.FindTarget
        return FindTarget();
    }
    private GameObject FindClosestTargetToMouseCursor(Vector2 mousePosition)
    {
        List<GameObject> potentialTargets = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);
        return StaticDataHolder.ListContents.Generic.GetClosestObject(potentialTargets, mousePosition);
    }
    private GameObject FindTarget()
    {
        List<GameObject> potentialTargets = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);
        List<LayerNames> layers = ObjectUtils.GetLayers(targetTypes);
        RemoveMyLayer(layers);
        return StaticDataHolder.ListContents.Generic.GetClosestObjectInSightAngleWise(potentialTargets, shootingPoint.position, GetForwardGunRotation().eulerAngles.z, layers.ToArray());
    }
    private void RemoveMyLayer(List<LayerNames> layers)
    {
        LayerNames myLayerName = NumberToLayerName(gameObject.layer);
        layers.Remove(myLayerName);
    }
    private Quaternion GetForwardGunRotation()
    {
        return Quaternion.Euler(0, 0, forwardGunRotation);
    }
    #endregion

    #region Sound
    //Sounds
    private void PlayShotSound()
    {
        SingleShotScriptableObject currentShotSO = salvo.shots[shotIndex].shot;
        if (currentShotSO.shotSounds.Length != 0)
        {
            AudioClip sound = currentShotSO.shotSounds[Random.Range(0, currentShotSO.shotSounds.Length)];
            StaticDataHolder.Sounds.PlaySound(sound, transform.position, currentShotSO.shotSoundVolume);
        }
    }
    #endregion

    #region Helper Functions
    private Quaternion GetRotationToTarget(GameObject target)
    {
        if (!target)
        {
            return transform.rotation * GetForwardGunRotation();
        }
        Quaternion weirdAngle = Quaternion.Euler(0, 0, -90);
        return RotationUtils.DeltaPositionRotation(shootingPoint.position, target.transform.position) * weirdAngle;
    }
    private void DecreaseShootingTime()
    {
        lastShotTime = Time.time;
        shootingTimeBank -= currentTimeBetweenEachShot;
    }
    private void UpdateTimeBetweenEachShot()
    {
        if (shotIndex < salvo.shots.Length)
        {
            currentTimeBetweenEachShot = salvo.shots[shotIndex].reloadDelay;
        }
        else
        {
            currentTimeBetweenEachShot = 100000;
        }
    }
    #endregion

    #region UI
    private void UpdateUIState()
    {
        UpdateProgressionBar();
        // There can come more UI updates here
    }
    private void UpdateProgressionBar()
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
    private void UpdateTargetFollower()
    {
        List<EntityCreator.Projectiles> projectileTypes = salvo.shots[shotIndex].shot.projectilesToCreateList;
        TargetFollowerProperty newTargetFollowerProperty = EntityCreator.GetFirstTargetFollower(projectileTypes);

        // If new is null, destroy old if not null
        if (newTargetFollowerProperty == null)
        {
            if (targetFollower != null)
            {
                Destroy(targetFollower);
            }
            return;
        }
        // If old is null and new is not ==> overwrite
        if (targetFollower == null && newTargetFollowerProperty != null)
        {
            ChangeTargetFollower(newTargetFollowerProperty.targetIcon);
            return;
        }
        // If icons not the same ==> destroy old ==> overwrite with new
        if (newTargetFollowerProperty.targetIcon != currentFollowerType)
        {
            Destroy(targetFollower);
            ChangeTargetFollower(newTargetFollowerProperty.targetIcon);
            return;
        }
    }
    private void ChangeTargetFollower(EntityCreator.ObjectFollowers objectFollower)
    {
        targetFollower = Instantiate(EntityFactory.GetPrefab(objectFollower), transform.position, Quaternion.identity);
        ClosestTargetFollower closestTargetFollower = targetFollower.GetComponentInChildren<ClosestTargetFollower>();
        closestTargetFollower.SetFollowMouse(true);
        closestTargetFollower.SetTeam(team);

        currentFollowerType = objectFollower;
    }
    private void UpdateAmmoBar()
    {
        if (dontUseProgressionBar)
        {
            return;
        }
        StaticProgressionBarUpdater.UpdateProgressionBar(this);
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
        UpdateUIState();
    }
    public void Detach()
    {
        isDetached = true;
        UpdateUIState();
    }
    public void SetShoot(bool set)
    {
        shoot = set;
    }
    #endregion

    #region Accessor methods
    public Transform GetFollowTransform()
    {
        return transform;
    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public float GetBarRatio()
    {
        return shootingTimeBank / salvoTimeSum;
    }
    public GameObject GetShootingPoint()
    {
        return shootingPoint.gameObject;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperMethods;
using static HelperMethods.LineOfSightUtils;

public class ShootingController : TeamUpdater, IProgressionBarCompatible
{
    [Header("Instances")]
    [SerializeField] SalvoScriptableObject salvo;
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] Transform shootingPoint;

    [Header("Settings")]
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float forwardGunRotation;
    [Header("Targeting settings")]
    [SerializeField] ShootingMode shootingMode;
    [Tooltip("Used if FindTarget mode is chosen")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;
    [Header("Mouse Steering")]
    bool isControlledByMouse; // Just in case
    [SerializeField] bool reloadingBarAlwaysOn = true;
    [Header("Progression bar usage")]
    [SerializeField] bool dontUseProgressionBar;

    public enum ShootingMode
    {
        Forward,
        FindTarget,
        CameraTargeting
    }

    //Private variables
    private SingleShotScriptableObject currentShotSO;
    private GameObject cameraTarget;
    /// <summary>
    ///The gun tries to shoot, if this is set to true
    /// </summary>
    protected bool shoot;
    private bool isDetached = false;
    private float shootingTimeBank;
    private float currentTimeBetweenEachShot;
    private float lastShotTime;
    private int shotIndex;
    private bool canShoot;
    private int shotAmount;
    private float salvoTimeSum;

    #region Initialization
    protected void Start()
    {
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
    }
    #endregion

    #region Update
    protected virtual void Update()
    {
        CheckTimeBank();
        TryShoot();
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
            DoOneShot(shotIndex);
            canShoot = false;
            StartCoroutine(WaitForNextShotCooldown(shotIndex));
            shotIndex++;
            UpdateTimeBetweenEachShot();
            UpdateAmmoBar();
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
        currentShotSO = salvo.shots[shotIndex].shot;
        PlayShotSound();
        CreateShot(shotIndex);
        //Update time bank
        DecreaseShootingTime();
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
        if (shootingMode == ShootingMode.Forward)
        {
            return null;
        }
        if (shootingMode == ShootingMode.CameraTargeting)
        {
            return cameraTarget;
        }
        //shootingMode == ShootingMode.FindTarget
        return FindTarget();
    }
    private GameObject FindTarget()
    {
        List<GameObject> potentialTargets = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);
        LayerNames[] layers = ObjectUtils.GetLayers(targetTypes);
        return StaticDataHolder.ListContents.Generic.GetClosestObjectInSight(potentialTargets, shootingPoint.position, layers);
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
        bool barOn = !isDetached && (isControlledByMouse || reloadingBarAlwaysOn);
        if (barOn)
        {
            StaticProgressionBarUpdater.CreateProgressionBar(this);
        }
        else
        {
            StaticProgressionBarUpdater.DeleteProgressionBar(this);
        }
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
    #endregion
}

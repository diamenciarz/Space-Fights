using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static HelperMethods;
using static HelperMethods.LineOfSightUtils;

public class GunController : AbstractShootingController, IProgressionBarCompatible, IPlayerControllable
{
    [Header("Instances")]
    public SalvoScriptableObject salvo;

    [Header("Settings")]
    [Tooltip("Describes how the gun will choose the direction of the projectiles coming out of its pipe")]
    [SerializeField] ShootingMode shootingDirection;
    [Tooltip("Describes how the gun will give targets to its projectiles")]
    [SerializeField] TargetMode targetMode;
    [Tooltip("Used if FindTarget mode is chosen")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;
    [Header("Progression bar compatibility")]
    [Tooltip("The progression bars and users should be a one-to-one match. If true, this script is not using GetGomponent<>() to find a ProgressionBarProperty.")]
    [SerializeField] bool dontUseProgressionBar;

    public enum ShootingMode
    {
        Forward,
        ClosestAnglewiseDirection,
        CameraDirection
    }
    public enum TargetMode
    {
        None,
        ClosestTargetAnglewise,
        CameraTarget
    }

    //Private variables
    [HideInInspector]
    public int shotIndex;
    private int shotAmount;
    private float currentTimeBetweenEachShot;
    private float salvoTimeSum;
    private List<INotifyOnDestroy> notifyOnDestroy = new List<INotifyOnDestroy>();
    private Rigidbody2D rb2D;

    #region Initialization
    protected override void InitializeStartingVariables()
    {
        lastShotTime = Time.time;
        salvoTimeSum = salvo.GetSalvoTimeSum();
        shootingTimeBank = salvoTimeSum;    
        shotAmount = salvo.shots.Length;
        canShoot = true;
        shotIndex = 0;
        rb2D = GetComponentInParent<Rigidbody2D>();
    }
    protected override void CallStartingMethods()
    {
        UpdateTimeBetweenEachShot();
        ResetUIState();
    }
    #endregion

    #region Update
    protected override void Update()
    {
        base.Update();
    }
    protected override void TryShoot()
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
            UpdateAmmoBar();

            shotIndex++;
            UpdateTimeBetweenEachShot();
            UpdateTargetFollower();
        }
    }
    #endregion

    #region Reloading
    protected override void TryReload()
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
        data.startingVelocity = GetVelocity();
        data.createdBy = createdBy;
        data.shot = salvo.shots[shotIndex].shot;
        data.target = GetTarget();
        EntityCreator.SummonShot(data);
    }
    private Quaternion GetRotation()
    {
        if (shootingDirection == ShootingMode.Forward)
        {
            return GetForwardGunRotation();
        }
        if (shootingDirection == ShootingMode.CameraDirection)
        {
            return GetRotationToTarget(cameraTarget);
        }
        {
            //shootingMode == ShootingMode.FindTarget
            GameObject foundTarget = FindTarget();
            return GetRotationToTarget(foundTarget);
        }
    }
    private Vector2 GetVelocity()
    {
        if (rb2D)
        {
            return rb2D.velocity;
        }
        else
        {
            return Vector2.zero;
        }
    }
    protected override GameObject GetTarget()
    {
        if (targetMode == TargetMode.None)
        {
            return null;
        }
        if (targetMode == TargetMode.CameraTarget)
        {
            if (StaticDataHolder.ListContents.Generic.IsObjectMouseCursor(cameraTarget))
            {
                return FindClosestTargetToMouseCursor(cameraTarget.transform.position);
            }
            return cameraTarget;
        }
        // targetMode == TargetMode.ClosestTargetAnglewise
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
        Vector2 vec = shootingPoint.transform.up;
        Quaternion rightToUpTranslation = Quaternion.Euler(0, 0, -90);
        // By default, Arctan2 starts counting 0 from the right. But in Unity we want angle 0 to be facing up
        return RotationUtils.DeltaPositionRotation(Vector2.zero, vec) * rightToUpTranslation;
    }
    #endregion

    #region Sound
    //Sounds
    private void PlayShotSound()
    {
        SingleShotScriptableObject currentShotSO = salvo.shots[shotIndex].shot;
        if (currentShotSO.shotSounds.Length != 0)
        {
            AudioClip sound = currentShotSO.shotSounds[UnityEngine.Random.Range(0, currentShotSO.shotSounds.Length)];
            StaticDataHolder.Sounds.PlaySound(sound, transform.position, currentShotSO.shotSoundVolume);
        }
    }
    #endregion

    #region Helper Functions
    private Quaternion GetRotationToTarget(GameObject target)
    {
        if (!target)
        {
            return GetForwardGunRotation();
        }
        Quaternion rightToUpTranslation = Quaternion.Euler(0, 0, -90);
        // By default, Arctan2 starts counting 0 from the right. But in Unity we want angle 0 to be facing up
        return RotationUtils.DeltaPositionRotation(shootingPoint.position, target.transform.position) * rightToUpTranslation;
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
    protected override void ResetUIState()
    {
        base.ResetUIState();
        ResetTargetFollowers();
    }
    private void ResetTargetFollowers()
    {
        targetFollowers.Reset();
        int index = 0;
        foreach (SalvoScriptableObject.Shot shot in salvo.shots)
        {
            List<EntityCreator.Projectiles> projectileTypes = shot.shot.projectilesToCreateList;
            TargetFollowerProperty newTargetFollowerProperty = EntityCreator.GetFirstTargetFollower(projectileTypes);

            if (newTargetFollowerProperty != null)
            {
                targetFollowers.AddTargetFollower(index, new TargetFollower(newTargetFollowerProperty.targetIcon));
            }
            index ++;
        }
        targetFollowers.SetCurrentProjectile(shotIndex);
    }
    private void UpdateTargetFollower()
    {
        targetFollowers.SetCurrentProjectile(shotIndex);
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

    #region Accessor methods
    public override float GetBarRatio()
    {
        return shootingTimeBank / salvoTimeSum;
    }
    public void AddOnDestroyAction(INotifyOnDestroy toCall)
    {
        notifyOnDestroy.Add(toCall);
    }
    #endregion
    private void OnDestroy()
    {
        targetFollowers.Reset();
        foreach (INotifyOnDestroy toCall in notifyOnDestroy)
        {
            toCall.NofityOnDestroy(gameObject);
        }
    }
}

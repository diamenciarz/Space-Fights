using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityCreator;
using static HelperMethods;
using static HelperMethods.LineOfSightUtils;

public class PushingController : AbstractShootingController, IProgressionBarCompatible, IPlayerControllable
{
    [Header("Force")]
    [SerializeField] AnimationCurve forceOverTime;
    [SerializeField] float reloadDelay;
    [SerializeField] bool reloadAllAtOnce;

    [Header("Settings")]
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float forwardGunRotation;
    [Tooltip("Describes how the gun will give targets to its projectiles")]
    [SerializeField] ShootingMode targetMode;
    [Tooltip("Used if FindTarget mode is chosen")]
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;
    [Header("Mouse Steering")]
    [SerializeField] ObjectFollowers targetIcon;
    [SerializeField] float pushingRange = 15;
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
    public float totalRayDuration = 3;
    private GameObject closestTarget;

    #region Initialization
    protected override void InitializeStartingVariables()
    {
        lastShotTime = Time.time;
        //totalRayDuration = forceOverTime.keys[forceOverTime.keys.Length - 1].time;
        shootingTimeBank = totalRayDuration;
        canShoot = true;
    }
    protected override void CallStartingMethods()
    {
        ResetUIState();
    }
    #endregion

    #region Update
    protected override void TryShoot()
    {
        if (!shoot)
        {
            closestTarget = GetTarget();
            return;
        }
        if (!closestTarget)
        {
            return;
        }
        if (VectorUtils.Distance(gameObject, closestTarget) > pushingRange)
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
        bool hasAmmo = shootingTimeBank > 0;
        if (hasAmmo)
        {
            PushTarget();
        }
    }
    #endregion

    #region Reloading
    protected override void TryReload()
    {
        if (reloadAllAtOnce)
        {
            TryReloadAllAmmo();
        }
        else
        {
            TryReloadAmmo();
        }
    }
    private void TryReloadAllAmmo()
    {
        bool anyAmmoMissing = shootingTimeBank < totalRayDuration;
        if (anyAmmoMissing)
        {
            float reloadCooldown = reloadDelay + totalRayDuration;
            float timeSinceLastShot = Time.time - lastShotTime;

            bool canReloadAll = timeSinceLastShot > reloadCooldown;
            if (canReloadAll)
            {
                shootingTimeBank = totalRayDuration;
                canShoot = true;
                UpdateAmmoBar();
            }
        }
    }
    private void TryReloadAmmo()
    {
        bool anyAmmoMissing = shootingTimeBank < totalRayDuration;
        if (anyAmmoMissing)
        {
            //TODO: Later make a separate reloading curve, but for now, reload cooldown should be equal to reloadDelay
            float timeSinceLastShot = Time.time - lastShotTime;

            bool canReloadOneFrame = timeSinceLastShot > reloadDelay;
            if (canReloadOneFrame)
            {
                float timeIncrease = Mathf.Clamp(timeSinceLastShot - reloadDelay, 0, totalRayDuration - shootingTimeBank);
                shootingTimeBank += timeIncrease;
                lastShotTime += timeIncrease;
                canShoot = true;
                UpdateAmmoBar();
            }
        }
    }
    #endregion

    #region Shot Methods
    private void PushTarget()
    {
        PlayPushSound();
        ApplyForceToTarget();

        DecreasePushingTime();
    }
    private void ApplyForceToTarget()
    {
        closestTarget.GetComponent<Rigidbody2D>().AddForce(GetPushingForce(), ForceMode2D.Force);
    }
    private Vector2 GetPushingForce()
    {
        //return forceOverTime.Evaluate(shotIndex);
        float force = 25f;
        Vector2 direction = VectorUtils.TranslatedMousePosition() - (Vector2)closestTarget.transform.position;
        return Mathf.Min(direction.magnitude * force, 100) * direction.normalized;
    }
    protected override GameObject GetTarget()
    {
        if (StaticDataHolder.ListContents.Generic.IsObjectMouseCursor(cameraTarget))
        {
            return FindClosestTargetToMouseCursor(cameraTarget.transform.position);
        }
        return cameraTarget;
    }
    private GameObject FindClosestTargetToMouseCursor(Vector2 mousePosition)
    {
        List<GameObject> potentialTargets = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypes);
        StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargets, team);
        return StaticDataHolder.ListContents.Generic.GetClosestObject(potentialTargets, mousePosition);
    }
    private void DecreasePushingTime()
    {
        shootingTimeBank -= Time.deltaTime;
        lastShotTime = Time.time;
        if (shootingTimeBank < 0)
        {
            shootingTimeBank = 0;
        }
        if (shootingTimeBank == 0)
        {
            canShoot = false;
        }
        UpdateAmmoBar();
    }
    #endregion

    #region Sound
    //Sounds
    private void PlayPushSound()
    {
        /*
        SingleShotScriptableObject currentShotSO = salvo.shots[shotIndex].shot;
        if (currentShotSO.shotSounds.Length != 0)
        {
            AudioClip sound = currentShotSO.shotSounds[Random.Range(0, currentShotSO.shotSounds.Length)];
            StaticDataHolder.Sounds.PlaySound(sound, transform.position, currentShotSO.shotSoundVolume);
        }*/
    }
    #endregion

    #region UI
    protected override void ResetUIState()
    {
        base.ResetUIState();
        ResetTargetFollower();
    }
    private void UpdateAmmoBar()
    {
        if (dontUseProgressionBar)
        {
            return;
        }
        StaticProgressionBarUpdater.UpdateProgressionBar(this);
    }
    private void ResetTargetFollower()
    {
        targetFollowers.Reset();
        if (isDetached || !isControlledByMouse)
        {
            return;
        }

        targetFollowers.AddTargetFollower(0, new TargetFollower(targetIcon));
        targetFollowers.SetCurrentProjectile(0);
    }
    #endregion

    #region Accessor methods
    public override float GetBarRatio()
    {
        return shootingTimeBank / totalRayDuration;
    }
    #endregion
}

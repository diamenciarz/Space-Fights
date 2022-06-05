using System.Collections;
using UnityEngine;

public class ShootingController : TeamUpdater, IProgressionBarCompatible
{
    [Header("Instances")]
    [SerializeField] SalvoScriptableObject salvo;
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] Transform shootingPoint;

    [Header("Settings")]
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float basicGunRotation;
    [Tooltip("If true, the shot will target the closest enemy. If false, will shoot forward")]
    [SerializeField] protected bool targetEnemies;
    [Header("Mouse Steering")]
    bool isControlledByMouse;
    [SerializeField] bool reloadingBarAlwaysOn = true;
    [Header("Progression bar usage")]
    [SerializeField] bool dontUseProgressionBar;

    private bool isDetached = false;
    //The gun tries to shoot, if this is set to true
    protected bool shoot;
    //Private variables
    private ProgressionBarController gunReloadingBarScript;
    private SingleShotScriptableObject currentShotSO;
    private GameObject parent;

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
        parent = transform.parent.gameObject;
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
        if (timeSinceLastShot >= reloadCooldown)
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
        data.summonRotation = transform.rotation * Quaternion.Euler(0, 0, basicGunRotation);
        data.summonPosition = shootingPoint.position;
        data.SetTeam(team);
        data.createdBy = createdBy;
        data.shot = salvo.shots[shotIndex].shot;
        data.target = GetShotTarget();

        EntityCreator.SummonShot(data);
    }
    private GameObject GetShotTarget()
    {
        if (!targetEnemies)
        {
            return null;
        }
        else
        {
            return StaticDataHolder.ListContents.Enemies.GetClosestEnemyInSight(transform.position, team);
        }
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

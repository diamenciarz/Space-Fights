using System.Collections;
using UnityEngine;

public class ShootingController : TeamUpdater
{
    [Header("Instances")]
    [SerializeField] SalvoScriptableObject salvo;
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] Transform shootingPoint;
    [SerializeField] GameObject gunReloadingBarPrefab;
    [SerializeField] GameObject reloadingBarPosition;

    [Header("Settings")]
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float basicGunRotation;
    [Tooltip("If true, the shot will target the closest enemy. If false, will shoot forward")]
    [SerializeField] protected bool targetEnemies;
    [Header("Mouse Steering")]
    bool isControlledByMouse;
    [SerializeField] bool reloadingBarOn = true;

    public bool isDetached = false;

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
        shootingTimeBank = GetSalvoTimeSum();
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

    protected virtual void Update()
    {
        CheckTimeBank();
        TryShoot();
        UpdateAmmoBar();
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
        }
    }

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
        float reloadCooldown = salvo.additionalReloadTime + GetSalvoTimeSum(shotIndex - 1);
        float timeSinceLastShot = Time.time - lastShotTime;
        if (timeSinceLastShot >= reloadCooldown)
        {
            shootingTimeBank = GetSalvoTimeSum();
            shotIndex = 0;
            UpdateTimeBetweenEachShot();
        }
    }
    private void TryReloadOneBullet()
    {
        if (shotIndex > 0)
        {
            float previousShotDelay = salvo.reloadDelays[shotIndex - 1];
            float reloadCooldown = salvo.additionalReloadTime + previousShotDelay;
            float timeSinceLastShot = Time.time - lastShotTime;

            if ((timeSinceLastShot >= reloadCooldown) && (shotIndex > 0))
            {
                shootingTimeBank += previousShotDelay;
                shotIndex--;
                lastShotTime += previousShotDelay;
                UpdateTimeBetweenEachShot();
            }
        }
    }
    IEnumerator WaitForNextShotCooldown(int index)
    {
        float delay = salvo.delayAfterEachShot[index];
        yield return new WaitForSeconds(delay);
        canShoot = true;
    }
    #endregion

    #region Shot Methods
    private void DoOneShot(int shotIndex)
    {
        currentShotSO = salvo.shots[shotIndex];
        PlayShotSound();
        CreateShot(shotIndex);
        //Update time bank
        DecreaseShootingTime();
    }
    private void CreateShot(int shotIndex)
    {
        SummonedShotData data = new SummonedShotData();
        data.summonRotation = transform.rotation * Quaternion.Euler(0,0,basicGunRotation);
        data.summonPosition = shootingPoint.position;
        data.team = team;
        data.createdBy = createdBy;
        data.shot = salvo.shots[shotIndex];
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
            return StaticDataHolder.GetClosestEnemyInSight(transform.position, team);
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
            StaticDataHolder.PlaySound(sound, transform.position, currentShotSO.shotSoundVolume);
        }
    }
    #endregion

    #region Helper Functions
    private float GetSalvoTimeSum()
    {
        float timeSum = 0;
        foreach (var item in salvo.reloadDelays)
        {
            timeSum += item;
        }
        return timeSum;

    }
    /// <summary>
    /// Summes the time for the amount of shots. Starts counting from the last index. Amount starts from 0.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    private float GetSalvoTimeSum(int amount)
    {
        amount = ClampInputIndex(amount);
        float timeSum = 0;

        for (int i = 0; i < amount; i++)
        {
            timeSum += salvo.reloadDelays[i];
        }
        return timeSum;
    }
    private void DecreaseShootingTime()
    {
        lastShotTime = Time.time;
        shootingTimeBank -= currentTimeBetweenEachShot;
    }
    private int ClampInputIndex(int index)
    {
        int shotAmount = salvo.shots.Length;
        if (index < 0)
        {
            index = 0;
        }
        else
        if (index >= shotAmount)
        {
            index = shotAmount - 1;
        }
        return index;
    }
    private void UpdateTimeBetweenEachShot()
    {
        if (shotIndex < salvo.reloadDelays.Count)
        {
            currentTimeBetweenEachShot = salvo.reloadDelays[shotIndex];
        }
        else
        {
            currentTimeBetweenEachShot = 1000;
        }
    }
    #endregion

    #region UI
    private void UpdateUIState()
    {
        if (isControlledByMouse || reloadingBarOn)
        {
            CreateUI();
        }
        else
        {
            DeleteUI();
        }
    }
    private void CreateUI()
    {
        if (gunReloadingBarScript == null)
        {
            CreateGunReloadingBar();
        }
    }
    private void DeleteUI()
    {
        if (gunReloadingBarScript != null)
        {
            Destroy(gunReloadingBarScript.gameObject);
        }
    }
    private void CreateGunReloadingBar()
    {
        if (isDetached || gunReloadingBarPrefab != null)
        {
            GameObject newReloadingBarGO = Instantiate(gunReloadingBarPrefab, transform.position, transform.rotation);
            gunReloadingBarScript = newReloadingBarGO.GetComponent<ProgressionBarController>();
            gunReloadingBarScript.SetObjectToFollow(reloadingBarPosition);
            lastShotTime = Time.time;
        }
    }
    private void UpdateAmmoBar()
    {
        if (gunReloadingBarScript != null)
        {
            gunReloadingBarScript.UpdateProgressionBar(shootingTimeBank, GetSalvoTimeSum());
        }

    }
    public void SetIsControlledByMouse(bool isTrue)
    {
        isControlledByMouse = isTrue;
        UpdateUIState();
    }
    #endregion

    #region Mutator methods
    public void Detach()
    {
        isDetached = true;
        DeleteUI();
    }
    public void SetShoot(bool set)
    {
        shoot = set;
    }
    #endregion

}

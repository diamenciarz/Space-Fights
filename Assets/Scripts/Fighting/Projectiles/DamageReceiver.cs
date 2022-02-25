using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : ListUpdater
{
    [Header("Basic Stats")]
    [SerializeField] int team;
    [SerializeField] int health;
    [SerializeField] GameObject healthBarPrefab;
    [SerializeField] bool turnHealthBarOn;
    [SerializeField] bool canBePushed;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    //Private variables
    private GameObject healthBarInstance;
    private int maxHP;
    private bool isDestroyed = false;
    private ICollidingEntityData myEntityData; //basic projectile controller script for velocity editing
    protected ProgressionBarController healthBarScript;
    private IOnDamageDealt[] onHitCalls;

    protected void Awake()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        myEntityData = GetComponent<ICollidingEntityData>();
        maxHP = health;
        onHitCalls = GetComponentsInChildren<IOnDamageDealt>();
    }
    protected void Update()
    {
        UpdateHealthBarVisibility();
    }
    private void UpdateHealthBarVisibility()
    {
        if (healthBarScript)
        {
            if (turnHealthBarOn)
            {
                healthBarScript.SetIsVisible(true);
            }
            else
            {
                healthBarScript.SetIsVisible(false);
            }
        }
    }

    #region Receive Damage
    /// <summary>
    /// Deal damage and try to push object
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="gameObject"></param>
    public void DealDamage(IDamageReceived iDamage)
    {
        health -= iDamage.GetDamage();
        UpdateHealthBar();
        CheckHealth();

        //ModifyVelocity(iDamage);
        HandleDamage(iDamage);
    }
    private void ModifyVelocity(IDamageReceived iDamage)
    {
        if (myEntityData != null)
        {
            if (canBePushed && iDamage.GetIsPushing())
            {
                myEntityData.ModifyVelocityVector3(iDamage.GetPushVector(transform.position));
            }
        }
    }
    private void CheckHealth()
    {
        if (health <= 0)
        {
            HandleBreak();
        }
        else
        {
            HandleHit();
        }
    }
    private void HandleDamage(IDamageReceived iDamage)
    {
        foreach (IOnDamageDealt call in onHitCalls)
        {
            //If an enemy is hit by a bullet, then he receives information about the position of the entity shooting
            call.HitBy(iDamage.CreatedBy());
        }
    }
    #endregion

    #region Collision Handling
    //Break methods
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            StaticDataHolder.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);
            DestroyObject();
        }
    }
    protected void HandleHit()
    {
        StaticDataHolder.PlaySound(GetHitSound(), transform.position, hitSoundVolume);
    }
    
    #endregion

    #region Sounds
    protected AudioClip GetHitSound()
    {
        int soundIndex = Random.Range(0, hitSounds.Count);
        if (hitSounds.Count > soundIndex)
        {
            return hitSounds[soundIndex];
        }
        return null;
    }
    protected AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }
    #endregion

    #region UI
    //Other stuff
    public void CreateHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position, transform.rotation);
        healthBarScript = healthBarInstance.GetComponent<ProgressionBarController>();
        healthBarScript.SetObjectToFollow(gameObject);
    }
    private void UpdateHealthBar()
    {
        if (healthBarScript == null)
        {
            CreateHealthBar();
        }
        //Debug.Log("Updated HP");
        healthBarScript.UpdateProgressionBar(health, maxHP);
    }
    #endregion

    #region Team
    public virtual void SetTeam(int newTeam)
    {
        team = newTeam;
        UpdateTeam(newTeam);
    }
    private void UpdateTeam(int newTeam)
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.ChangeTeamTo(newTeam);
        }
        DamageReceiver[] damageReceivers = GetComponentsInChildren<DamageReceiver>();
        foreach (DamageReceiver item in damageReceivers)
        {
            item.ChangeTeamTo(newTeam);
        }
    }
    /// <summary>
    /// Change team of this script. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public void ChangeTeamTo(int newTeam)
    {
        team = newTeam;
    }
    #endregion

    #region Accessor methods
    public int GetTeam()
    {
        return team;
    }
    public int GetCurrentHealth()
    {
        return health;
    }
    #endregion
}

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

    //Private variables
    private GameObject healthBarInstance;
    private int maxHP;
    protected ProgressionBarController healthBarScript;
    private IOnDamageDealt[] onHitCalls;

    protected void Awake()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
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

        HandleDamage(iDamage);
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

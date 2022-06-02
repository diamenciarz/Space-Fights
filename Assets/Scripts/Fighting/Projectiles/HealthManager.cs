using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TeamUpdater;

public class HealthManager : ListUpdater, IParent, ITeamable
{
    [Header("Basic Stats")]
    [Tooltip("If this is set to true, summoned copies of this object will have the same team as is defined in the editor")]
    [SerializeField] bool unchangeableTeam;
    [SerializeField] Team team;
    [SerializeField] GameObject healthBarPrefab;
    [SerializeField] bool turnHealthBarOn;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;

    //Private variables
    private GameObject healthBarInstance;
    private float health;
    private float maxHealth;
    private List<DamageReceiver> parts = new List<DamageReceiver>();
    private GameObject createdBy;
    /// <summary>
    /// This is the additional damage received from parts being destroyed
    /// </summary>
    private float additionalDamage;
    protected ProgressionBarController healthBarScript;

    #region Startup
    protected void Start()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        SetupCreatedBy();
        FindParts();
        UpdateMaxHP();
        UpdateHealth();
    }
    private void SetupCreatedBy()
    {
        if (!createdBy)
        {
            createdBy = gameObject;
        }
    }
    private void FindParts()
    {
        parts = new List<DamageReceiver>();
        DamageReceiver[] partsFound = GetComponentsInChildren<DamageReceiver>();
        if (partsFound.Length > 0)
        {
            parts.AddRange(partsFound);
        }
    }
    private void UpdateMaxHP()
    {
        maxHealth = 0;
        foreach (DamageReceiver part in parts)
        {
            maxHealth += part.GetMaxBarHealth();
        }
    }
    #endregion

    #region Update
    protected void Update()
    {
        UpdateHealthBarVisibility();
    }
    private void UpdateHealthBarVisibility()
    {
        if (!healthBarScript)
        {
            return;
        }
        if (turnHealthBarOn)
        {
            healthBarScript.SetIsVisible(true);
        }
        else
        {
            healthBarScript.SetIsVisible(false);
        }
    }
    #endregion

    #region HP
    private float CountHP()
    {
        float hp = 0;
        foreach (DamageReceiver part in parts)
        {
            hp += part.GetBarHealth();
        }
        return hp;
    }
    private void CheckHP()
    {
        if (health <= 0)
        {
            HandleBreak();
        }
    }
    protected void HandleBreak()
    {
        StaticDataHolder.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);
        DestroyObject();
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
    public void CreateHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position, transform.rotation);
        healthBarScript = healthBarInstance.GetComponent<ProgressionBarController>();
        healthBarScript.SetObjectToFollow(gameObject);
    }
    private void UpdateHealthBar()
    {
        if (!healthBarPrefab)
        {
            return;
        }
        if (healthBarScript == null)
        {
            CreateHealthBar();
        }
        healthBarScript.UpdateProgressionBar(health, maxHealth);
    }
    #endregion

    #region Team
    /// <summary>
    /// Change team of this object and all its children. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public void SetTeam(Team newTeam)
    {
        if (unchangeableTeam)
        {
            return;
        }
        team = newTeam;
        UpdateTeam();
    }
    private void UpdateTeam()
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.UpdateTeam(this);
        }
    }
    #endregion

    #region Accessor methods
    public Team GetTeam()
    {
        return team;
    }
    public float GetCurrentHealth()
    {
        return health;
    }
    public GameObject GetCreatedBy()
    {
        return createdBy;
    }
    #endregion

    #region Mutator methods
    public void UpdateHealth()
    {
        float counted = CountHP();
        health = counted - additionalDamage;
        UpdateHealthBar();
        CheckHP();
    }
    public void RemovePart(DamageReceiver part)
    {
        parts.Remove(part);
        additionalDamage += part.GetDestroyDamage();
        UpdateHealth();
    }
    /// <summary>
    /// Set HP of each non-destroyed part to maximum and update HP bar
    /// </summary>
    public void DoFullHeal()
    {
        foreach (DamageReceiver part in parts)
        {
            part.DoFullHeal();
        }
        UpdateHealth();
    }
    public void SetCreatedBy(GameObject parent)
    {
        if (parent)
        {
            createdBy = parent;
        }
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : ListUpdater
{
    [Header("Basic Stats")]
    [SerializeField] int team;
    [SerializeField] GameObject healthBarPrefab;
    [SerializeField] bool turnHealthBarOn;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;

    //Private variables
    private GameObject healthBarInstance;
    private float health;
    private float maxHealth;
    private List<UnitPart> parts;
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
        FindParts();
        UpdateMaxHP();
        UpdateHealth();
    }
    private void FindParts()
    {
        UnitPart[] partsFound = GetComponentsInChildren<UnitPart>();
        parts.AddRange(partsFound);
    }
    private void UpdateMaxHP()
    {
        maxHealth = 0;
        foreach (UnitPart part in parts)
        {
            maxHealth += part.GetMaxBarHealth();
        }
    }
    #endregion

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

    #region HP
    private float CountHP()
    {
        float hp = 0;
        foreach (UnitPart part in parts)
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
        if (healthBarScript == null)
        {
            CreateHealthBar();
        }
        healthBarScript.UpdateProgressionBar(health, maxHealth);
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
    public float GetCurrentHealth()
    {
        return health;
    }
    #endregion

    #region Mutator methods
    public void UpdateHealth()
    {
        health = CountHP() - additionalDamage;
        UpdateHealthBar();
        CheckHP();
    }
    public void RemovePart(UnitPart part)
    {
        parts.Remove(part);
        additionalDamage += part.GetDestroyDamage();
        UpdateHealth();
    }
    /// <summary>
    /// Set HP of each non-destroyed part to maximum and update HP bar
    /// </summary>
    private void DoFullHeal()
    {
        foreach (UnitPart part in parts)
        {
            part.DoFullHeal();
        }
        UpdateHealth();
    }
    #endregion
}

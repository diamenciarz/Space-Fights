using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TeamUpdater;

public class HealthManager : ListUpdater, IParent, ITeamable, IProgressionBarCompatible
{
    [Header("Basic Stats")]
    [Tooltip("If this is set to true, summoned copies of this object will have the same team as is defined in the editor")]
    [SerializeField] bool unchangeableTeam;
    [SerializeField] Team team;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [Header("Progression bar usage")]
    [SerializeField] bool dontUseProgressionBar = true;

    //Private variables
    private float health;
    private float maxHealth;
    private List<DamageReceiver> parts = new List<DamageReceiver>();
    private GameObject createdBy;
    /// <summary>
    /// This is the additional damage received from parts being destroyed
    /// </summary>
    private float additionalDamage;
    private bool isDestroyed = false;

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
        HandleHealthBar();
    }
    private void SetupCreatedBy()
    {
        if (!createdBy)
        {
            createdBy = gameObject;
        }
        UpdateCreatorInChildren();
    }
    private void UpdateCreatorInChildren()
    {
        TeamUpdater[] teamUpdaters = GetComponentsInChildren<TeamUpdater>();
        foreach (var teamUpdater in teamUpdaters)
        {
            teamUpdater.SetCreatedBy(createdBy);
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
    private void HandleHealthBar()
    {
        if (dontUseProgressionBar)
        {
            return;
        }
        Debug.Log("Created progression bar");
        StaticProgressionBarUpdater.CreateProgressionBar(this);
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
        if (!isDestroyed)
        {
            isDestroyed = true;
            StaticDataHolder.Sounds.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);
            DisconnectAllParts();
            DestroyObject();
        }
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
    private void DisconnectAllParts()
    {
        if (parts.Count == 0)
        {
            return;
        }
        for (int i = parts.Count - 1; i >= 0; i--)
        {
            parts[i].ParentBrokeOff();
        }
    }
    #endregion

    #region UI
    private void UpdateHealthBar()
    {
        if (dontUseProgressionBar)
        {
            return;
        }
        StaticProgressionBarUpdater.UpdateProgressionBar(this);
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
    public GameObject GetGameObject()
    {
        return gameObject;
    }
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
    public float GetBarRatio()
    {
        return health / maxHealth;
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

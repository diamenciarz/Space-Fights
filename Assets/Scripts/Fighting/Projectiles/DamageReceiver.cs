using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : ListUpdater, IParent
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
    public float health;
    public float maxHealth;
    public List<UnitPart> parts = new List<UnitPart>();
    private GameObject createdBy;
    /// <summary>
    /// This is the additional damage received from parts being destroyed
    /// </summary>
    public float additionalDamage;
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
        parts = new List<UnitPart>();
        UnitPart[] partsFound = GetComponentsInChildren<UnitPart>();
        if (partsFound.Length > 0)
        {
            parts.AddRange(partsFound);
        }
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
    /// <summary>
    /// Change team of this object and all its children. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public void SetTeam(int newTeam)
    {
        team = newTeam;
        UpdateTeam();
    }
    private void UpdateTeam()
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.UpdateTeam();
        }
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
    public GameObject GetCreatedBy()
    {
        return createdBy;
    }
    #endregion

    #region Mutator methods
    public void UpdateHealth()
    {
        float counted = CountHP();
        Debug.Log("Counted HP:" + counted);
        health = counted - additionalDamage;
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
    public void DoFullHeal()
    {
        foreach (UnitPart part in parts)
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

    #region JoinBreak
    public void OnJointBreak2D(Joint2D joint)
    {
        UnitPart jointPart = joint.gameObject.GetComponent<UnitPart>();
        if (!jointPart)
        {
            return;
        }


        jointPart.GetBarHealth();
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPart : SpriteUpdater, IDamageReceiver
{
    [Header("Startup")]
    
    [Tooltip("Actions to call on death")]
    [SerializeField] IOnDestroyed iOnDestroyed;

    [Header("Health")]
    [Tooltip("The amount of damage that this part can receive, before it is destroyed")]
    [SerializeField] float partHealth;
    [Tooltip("The share of health that this part contributes to the HP bar")]
    [SerializeField] float barHealth;
    [Tooltip("The additional damage that is dealt to the unit, when this part is destroyed")]
    [SerializeField] float destroyDamage;

    [Header("Collisions")]
    [Tooltip("The collision velocity, above which this ship part will start taking damage")]
    [SerializeField] float minKineticEnergy = 10;
    [SerializeField] float collisionDamageModifier = 0.5f;
    [SerializeField] OnCollisionDamage.TypeOfDamage[] immuneTo;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    //Private variables
    DamageReceiver damageReceiver;
    private Rigidbody2D myRigidbody2D;
    private IOnDamageDealt[] onHitCalls;

    private float maxPartHealth;
    private float maxBarHealth;
    private float barToPartRatio;
    private bool isDestroyed;

    void Start()
    {
        SetupStartingVariables();
        UpdateTeam();
    }
    private void SetupStartingVariables()
    {
        onHitCalls = GetComponentsInParent<IOnDamageDealt>(); // Informs AI script about getting hit
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        damageReceiver = GetComponentInParent<DamageReceiver>();

        iOnDestroyed = GetComponent<IOnDestroyed>(); // On death trigger

        maxPartHealth = partHealth;
        maxBarHealth = barHealth;
        barToPartRatio = barHealth / partHealth;
    }

    #region Collisions
    //Collision damage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
        HandleDamage(collision.gameObject.GetComponent<IDamageDealer>());
    }
    private void HandleCollision(Collision2D collision)
    {
        Rigidbody2D colliderRB2D = collision.gameObject.GetComponent<Rigidbody2D>();
        if (colliderRB2D == null)
        {
            return;
        }

        int damage = CountCollisionDamage(colliderRB2D);
        DealDamage(damage);
    }
    private int CountCollisionDamage(Rigidbody2D colliderRB2D)
    {
        Vector2 deltaVelocity = colliderRB2D.velocity - myRigidbody2D.velocity;
        float speed = deltaVelocity.magnitude;
        float mass = colliderRB2D.mass;
        float damage = speed * speed * mass * collisionDamageModifier;

        if (damage > minKineticEnergy)
        {
            return (int)damage;
        }
        return 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleDamage(collision.gameObject.GetComponent<IDamageDealer>());
    }
    public bool HandleDamage(IDamageDealer iDamageReceived)
    {
        if (iDamageReceived == null || iDamageReceived.GetTeam() != team || IsImmune(iDamageReceived))
        {
            return false;
        }
        if (DealDamage(iDamageReceived.GetDamage(gameObject)))
        {
            NotifyAboutDamage(iDamageReceived);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Notify the AI controller type, who hit this unit
    /// </summary>
    /// <param name="iDamage"></param>
    private void NotifyAboutDamage(IDamageDealer iDamage)
    {
        foreach (IOnDamageDealt call in onHitCalls)
        {
            //If an enemy is hit by a bullet, then he receives information about the position of the entity shooting
            call.HitBy(iDamage.CreatedBy());
        }
    }
    private bool IsImmune(IDamageDealer iDamageReceived)
    {
        foreach (var damageType in immuneTo)
        {
            if (iDamageReceived.DamageTypeContains(damageType))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region HP
    private void LowerHealthBy(int damage)
    {
        partHealth -= damage;
        barHealth -= (float)damage * barToPartRatio;
    }
    private void CheckHP()
    {
        damageReceiver.UpdateHealth();
        if (partHealth <= 0)
        {
            HandleBreak();
        }
        else
        {
            HandleHit();
        }
    }
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
    private void DestroyObject()
    {
        DoDestroyActions();
        damageReceiver.RemovePart(this);
    }
    private void DoDestroyActions()
    {
        iOnDestroyed.DestroyObject();
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

    #region Accessor methods
    public float GetBarHealth()
    {
        return barHealth;
    }
    public float GetMaxBarHealth()
    {
        return maxBarHealth;
    }
    /// <summary>
    /// Returns the health of the part, not the part on the HP bar
    /// </summary>
    /// <returns></returns>
    public int GetHealth()
    {
        return (int)partHealth;
    }
    public float GetDestroyDamage()
    {
        return destroyDamage;
    }
    #endregion

    #region Mutator methods
    public void DoFullHeal()
    {
        partHealth = maxPartHealth;
        barHealth = maxBarHealth;
    }
    /// <summary>
    /// Returns true, if the damage was successfully dealt
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public bool DealDamage(int damage)
    {
        if (damage > 0 && !isDestroyed)
        {
            LowerHealthBy(damage);
            CheckHP();
            return true;
        }
        return false;
    }
    #endregion
}
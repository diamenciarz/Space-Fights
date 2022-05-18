using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPart : SpriteUpdater, IDamageable
{
    [Header("Health")]
    [Tooltip("The amount of damage that this part can receive, before it is destroyed")]
    [SerializeField] float partHealth;
    [Tooltip("The share of health that this part contributes to the HP bar")]
    [SerializeField] float barHealth;
    [Tooltip("The additional damage that is dealt to the unit, when this part is destroyed")]
    [SerializeField] float destroyDamage;
    [SerializeField] List<DamageCalculator.Immunity> immunities = new List<DamageCalculator.Immunity>();

    [Header("Collisions")]
    [Tooltip("The collision velocity, above which this ship part will start taking damage")]
    [SerializeField] float minKineticEnergy = 10;
    [SerializeField] float collisionDamageModifier = 0.5f;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    [Header("When destroyed")]
    [Tooltip("Actions to call on death")]
    public TriggerOnDeath[] onDeathTriggers;

    //Private variables
    DamageReceiver damageReceiver;
    private Rigidbody2D myRigidbody2D;
    private IOnDamageDealt[] onHitCalls;

    private float maxPartHealth;
    private float maxBarHealth;
    private float barToPartRatio;
    private bool isDestroyed;

    #region Startup
    protected override void Awake()
    {
        base.Awake();
        UpdateTeam(damageReceiver);
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        onHitCalls = GetComponentsInParent<IOnDamageDealt>(); // Informs AI script about getting hit
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        damageReceiver = GetComponentInParent<DamageReceiver>();

        onDeathTriggers = GetComponentsInChildren<TriggerOnDeath>(); // On death trigger

        maxPartHealth = partHealth;
        maxBarHealth = barHealth;
        barToPartRatio = barHealth / partHealth;
    }
    #endregion

    #region Physical Collisions
    //Collision damage
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //This class only calculates physical damage on its own
        HandleCollision(collision);
    }
    private void HandleCollision(Collision2D collision)
    {
        Rigidbody2D colliderRB2D = collision.gameObject.GetComponent<Rigidbody2D>();
        if (colliderRB2D == null || isDestroyed)
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
        float collisionKineticEnergy = speed * speed * mass / 2;

        if (collisionKineticEnergy > minKineticEnergy)
        {
            collisionKineticEnergy *= collisionDamageModifier;
            return (int)collisionKineticEnergy;
        }
        return 0;
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
    public GameObject GetGameObject()
    {
        return gameObject;
    }
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
    public List<DamageCalculator.Immunity> GetImmunities()
    {
        return immunities;
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
        bool damageDealt = damage > 0 && !isDestroyed;
        if (damageDealt)
        {
            LowerHealthBy(damage);
            CheckHP();
            return true;
        }
        return false;
    }
    #endregion

    #region OnJointBreak2D
    public void OnJointBreak2D(Joint2D joint)
    {
        damageReceiver.RemovePart(this);
        transform.SetParent(null);

        TurnOffGuns();
    }
    private void TurnOffGuns()
    {
        ShootingController shootingController = GetComponent<ShootingController>();
        if (shootingController)
        {
            shootingController.setIsDetached(true);
        }
    }
    
    #endregion

    #region HP Helper methods
    /// <summary>
    /// Notify the AI controller type, who hit this unit
    /// </summary>
    /// <param name="iDamage"></param>
    public void NotifyAboutDamage(GameObject damagedBy)
    {
        foreach (IOnDamageDealt call in onHitCalls)
        {
            //If an enemy is hit by a bullet, then he receives information about the position of the entity shooting
            call.HitBy(damagedBy);
        }
    }
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
            barHealth = 0;
            partHealth = 0;
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
        HelperMethods.DoDestroyActions(gameObject);
        damageReceiver.RemovePart(this);
        StartCoroutine(DestroyAtTheEndOfFrame());
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion
}
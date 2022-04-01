using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : OnCollisionBreak, IDamageDealer
{
    [Header("Basic Stats")]
    [SerializeField] int damage;
    [SerializeField] protected bool hurtsAllies;
    [SerializeField] bool isPiercing;

    [Header("Damage type")]
    public List<TypeOfDamage> damageTypes = new List<TypeOfDamage>();

    public enum TypeOfDamage
    {
        Projectile,
        Explosion,
        Rocket
    }

    private int currentDamageLeft;


    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        currentDamageLeft = damage;
    }

    #region Collision
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    private void HandleCollision(GameObject collisionObject)
    {
        if (CanDealDamage(collisionObject))
        {
            IDamageReceiver damageReceiver = collisionObject.GetComponent<IDamageReceiver>();
            DealDamageToObject(damageReceiver);
        }
    }
    private bool CanDealDamage(GameObject collisionObject)
    {
        if (isDestroyed)
        {
            return false;
        }
        bool isInvulnerable = IsInvulnerableTo(collisionObject);
        if (isInvulnerable)
        {
            return false;
        }
        IDamageReceiver iDamageReceiver = collisionObject.GetComponent<IDamageReceiver>();
        bool objectCanReceiveDamage = iDamageReceiver != null;
        if (!objectCanReceiveDamage)
        {
            return false;
        }
        bool shouldDealDamage = hurtsAllies || iDamageReceiver.GetTeam() != team;
        if (!shouldDealDamage)
        {
            return false;
        }
        return true;
    }

    #region Deal damage
    private void DealDamageToObject(IDamageReceiver damageReceiver)
    {
        if (damageReceiver.HandleDamage(this))
        {
            HandlePiercing(damageReceiver);
        }
    }
    private void HandlePiercing(IDamageReceiver damageReceiver)
    {
        if (isPiercing)
        {
            LowerMyDamage(damageReceiver.GetHealth());
        }
    }
    private void LowerMyDamage(int change)
    {
        currentDamageLeft -= change;
        CheckDamageLeft();
    }
    private void CheckDamageLeft()
    {
        if (currentDamageLeft < 0)
        {
            currentDamageLeft = 0;
            DestroyObject();
        }
    }
    protected void DestroyObject()
    {
        isDestroyed = true;
        HelperMethods.CallAllTriggers(gameObject);
        StartCoroutine(DestroyAtTheEndOfFrame());
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion
    #endregion

    #region Accessor methods
    public int GetDamage(GameObject damageReceiver)
    {
        if (CanDealDamage(damageReceiver))
        {
            return currentDamageLeft;
        }
        return 0;
    }
    public List<TypeOfDamage> GetDamageTypes()
    {
        return damageTypes;
    }
    public bool DamageTypeContains(TypeOfDamage damageType)
    {
        if (damageTypes.Contains(damageType))
        {
            return true;
        }
        return false;
    }
    public bool IsAProjectile()
    {
        return damageTypes.Count != 0;
    }
    public GameObject CreatedBy()
    {
        return createdBy;
    }
    #endregion
}

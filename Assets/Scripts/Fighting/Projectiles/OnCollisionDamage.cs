using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : OnCollisionBreak, IDamageReceived
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

    private ICollidingEntityData entityData;
    private int currentDamageLeft;

    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        entityData = GetComponent<ICollidingEntityData>();
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
            DamageReceiver damageReceiver = collisionObject.GetComponent<DamageReceiver>();
            DealDamageToObject(damageReceiver);
        }
    }
    private bool CanDealDamage(GameObject collisionObject)
    {
        DamageReceiver damageReceiver = collisionObject.GetComponent<DamageReceiver>();
        bool objectCanReceiveDamage = damageReceiver != null;
        if (objectCanReceiveDamage)
        {
            bool shouldDealDamage = hurtsAllies || damageReceiver.GetTeam() != team;
            if (shouldDealDamage)
            {
                bool isInvulnerable = IsInvulnerableTo(collisionObject);
                if (!isInvulnerable)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #region Deal damage
    private void DealDamageToObject(DamageReceiver damageReceiver)
    {
        damageReceiver.DealDamage(this);
        HandlePiercing(damageReceiver);
    }
    private void HandlePiercing(DamageReceiver damageReceiver)
    {
        if (isPiercing)
        {
            int collisionHP = damageReceiver.GetCurrentHealth();
            LowerMyDamage(collisionHP);
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
    public virtual Vector3 GetVelocityVector3()
    {
        return entityData.GetVelocityVector3();
    }
    public int GetDamage()
    {
        return currentDamageLeft;
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

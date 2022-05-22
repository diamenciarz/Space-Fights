using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : BreakOnCollision, IDamageDealer
{
    [Header("Basic Stats")]
    [SerializeField] protected bool hurtsAllies;

    [Header("Damage type")]
    [SerializeField] protected List<DamageInstance.DamageCategory> damageCategories = new List<DamageInstance.DamageCategory>();

    private List<GameObject> dealtDamageTo = new List<GameObject>();

    public enum TypeOfDamage
    {
        Explosion,
        Physical,
        Fire
    }

    protected override void Awake()
    {
        base.Awake();
    }

    #region Collisions
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
            DealDamageToObject(collisionObject);
        }
    }
    private bool CanDealDamage(GameObject collisionObject)
    {
        IDamageable iDamageReceiver = collisionObject.GetComponent<IDamageable>();
        bool objectCanReceiveDamage = iDamageReceiver != null;
        if (!objectCanReceiveDamage)
        {
            return false;
        }
        if (dealtDamageTo.Contains(collisionObject))
        {
            return false;
        }
        return true;
    }
    private void DealDamageToObject(GameObject collisionObject)
    {
        dealtDamageTo.Add(collisionObject);
        IDamageable iDamageable = collisionObject.GetComponent<IDamageable>();
        DamageCalculator.ApplyDamage(iDamageable, GetDamageInstance());
    }
    #endregion

    #region Accessor methods
    public virtual DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = new DamageInstance();
        damageInstance.createdBy = createdBy;
        damageInstance.damageCategories = damageCategories;
        damageInstance.dealtBy = gameObject;
        damageInstance.team = team;
        damageInstance.hurtsAllies = hurtsAllies;
        damageInstance.lifetime = Time.time - creationTime;

        return damageInstance;
    }

    public bool DamageCategoryContains(TypeOfDamage typeOfDamage)
    {
        foreach (var category in damageCategories)
        {
            if (category.damageType == typeOfDamage)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}

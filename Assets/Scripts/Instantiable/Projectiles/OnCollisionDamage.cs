using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : AbstractActionOnCollision, IDamageDealer
{
    [Header("Basic Stats")]
    [SerializeField] protected bool hurtsAllies;

    [Header("Damage type")]
    [SerializeField] protected List<DamageInstance.DamageCategory> damageCategories = new List<DamageInstance.DamageCategory>();


    #region Collisions
    protected override void HandleExit(GameObject collisionObject)
    {
        //Leave empty
    }
    protected override void HandleCollision(Collision2D collision)
    {
        if (CanDealDamage(collision.gameObject))
        {
            DealDamageToObject(collision.gameObject);
        }
    }
    protected override void HandleTriggerEnter(Collider2D trigger)
    {
        if (CanDealDamage(trigger.gameObject))
        {
            DealDamageToObject(trigger.gameObject);
        }
    }
    private bool CanDealDamage(GameObject collisionObject)
    {
        IDamageable iDamageReceiver = collisionObject.GetComponent<IDamageable>();
        bool objectCanReceiveDamage = iDamageReceiver != null;
        return objectCanReceiveDamage;
    }
    private void DealDamageToObject(GameObject collisionObject)
    {
        IDamageable iDamageable = collisionObject.GetComponent<IDamageable>();
        DamageCalculator.DealDamage(iDamageable, GetDamageInstance());
    }
    #endregion

    #region Accessor methods
    public virtual DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = new DamageInstance();
        damageInstance.createdBy = createdBy;
        damageInstance.damageCategories = damageCategories;
        damageInstance.dealtBy = gameObject;
        damageInstance.SetTeam(team);
        damageInstance.hurtsAllies = hurtsAllies;
        damageInstance.lifetime = Time.time - creationTime;

        return damageInstance;
    }

    public bool DamageCategoryContains(DamageInstance.TypeOfDamage typeOfDamage)
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

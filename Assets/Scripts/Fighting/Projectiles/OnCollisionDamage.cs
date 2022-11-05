using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : TeamUpdater, IDamageDealer
{
    [Header("Basic Stats")]
    [SerializeField] protected bool hurtsAllies;

    [Header("Damage type")]
    [SerializeField] protected List<DamageInstance.DamageCategory> damageCategories = new List<DamageInstance.DamageCategory>();

    protected List<GameObject> dealtDamageTo = new List<GameObject>();

    public enum TypeOfDamage
    {
        Explosion,
        Physical,
        Fire
    }

    #region Collisions
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
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
        bool alreadyDealtDamageToThisObject = dealtDamageTo.Contains(collisionObject);
        if (alreadyDealtDamageToThisObject)
        {
            return false;
        }
        return true;
    }
    private void DealDamageToObject(GameObject collisionObject)
    {
        dealtDamageTo.Add(collisionObject);
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

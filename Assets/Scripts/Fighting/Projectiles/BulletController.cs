using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController, IPiercingDamage
{
    [Header("Piercing")]
    [SerializeField] bool isPiercing;

    #region Mutator methods
    /// <summary>
    /// Lowers the physical damage by given value. All other types of damage reamin constant.
    /// However, the bullet is destroyed, when physical damage drops to zero.
    /// </summary>
    public void LowerDamageBy(int change)
    {
        foreach (var category in damageCategories)
        {
            if (category.damageType == TypeOfDamage.Physical)
            {
                category.damage -= change;
                if (category.damage <= 0)
                {
                    DestroyObject();
                }
            }
        }
    }
    #endregion

    #region Accessor methods
    public override DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = base.GetDamageInstance();
        damageInstance.isPiercing = isPiercing;
        damageInstance.iPiercingDamage = this;
        return damageInstance;
    }
    #endregion
}



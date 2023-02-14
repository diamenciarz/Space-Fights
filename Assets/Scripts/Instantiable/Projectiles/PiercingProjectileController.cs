using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BreakOnCollision))]
public class PiercingProjectileController : ProjectileController, IPiercingDamage
{
    //TODO: Implement total damage lowering upon damage dealt
    [Tooltip("The total damage that this projectile can deal, even if it pierces through many entities. The damage it deals to each entity is subtracted from totalDamage")]
    [SerializeField] protected List<DamageInstance.DamageCategory> totalDamageCategories = new List<DamageInstance.DamageCategory>();

    #region Mutator methods
    /// <summary>
    /// Lowers the physical damage by given value. All other types of damage reamin constant.
    /// However, the bullet is destroyed, when physical damage drops to zero.
    /// </summary>
    public void LowerDamageBy(List<DamageInstance.DamageCategory> change)
    {
        foreach (var category in damageCategories)
        {
            foreach (var change_category in change)
            {
                if (category.damageType == change_category.damageType)
                {
                    category.damage -= change_category.damage;
                    if (category.damage < 0)
                    {
                        category.damage = 0;
                    }
                }
            }
        }
        DestroyIfZeroDamageLeft();
    }
    private void DestroyIfZeroDamageLeft()
    {
        foreach (var category in damageCategories)
        {
            if (category.damage != 0)
            {
                return;
            }
        }
        // All categories have zero damage left, so the object should be destroyed
        breakOnCollision.DestroyObject();
    }
    #endregion

    #region Accessor methods
    public override DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = base.GetDamageInstance();
        damageInstance.isPiercing = true;
        damageInstance.iPiercingDamage = this;
        return damageInstance;
    }
    #endregion
}



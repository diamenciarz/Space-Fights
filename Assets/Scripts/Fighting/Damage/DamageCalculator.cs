using System;
using System.Collections.Generic;
using UnityEngine;
using static DamageInstance;

public class DamageCalculator : MonoBehaviour
{
    /// <summary>
    /// The time after the creation of an obejct that this object cannot be damaged for.
    /// </summary>
    private const float INVULNERABILITY_TIME = 0.25f;

    public static void DealDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!CanDealDamage(damageable, damageInstance))
        {
            return;
        }
        int totalDamage = damageInstance.GetTotalApplicableDamage(damageable);
        ApplyDamage(damageable, damageInstance, totalDamage);
    }
    private static void ApplyDamage(IDamageable damageable, DamageInstance damageInstance, int totalDamage)
    {
        if (totalDamage <= 0)
        {
            return;
        }
        HandlePiercing(damageable, damageInstance);
        damageable.DealDamage(totalDamage);
        damageable.NotifyAboutDamage(damageInstance.dealtBy);
    }

    #region Can deal damage
    private static bool CanDealDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        bool isInvulnerable = IsInvulnerableTo(damageable.GetGameObject(), damageInstance);
        if (isInvulnerable)
        {
            return false;
        }
        bool canDealDamage = damageInstance.hurtsAllies || damageable.GetTeam().IsEnemy(damageInstance.GetTeam());
        if (!canDealDamage)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Every unit is invulnerable to its own projectiles for INVULNERABILITY_TIME sec
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns>Whether the collisionObject is invulnerable to this game object</returns>
    protected static bool IsInvulnerableTo(GameObject target, DamageInstance damageInstance)
    {
        bool isTouchingParent = damageInstance.createdBy == target;
        bool isStillInvulnerable = damageInstance.lifetime < INVULNERABILITY_TIME;
        //The shooting object should be immune to its own projectiles for a split second
        if (isTouchingParent && isStillInvulnerable)
        {
            return true;
        }
        return false;
    }
    #endregion

    

    #region OnHit effects
    private static void HandlePiercing(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!damageInstance.isPiercing)
        {
            return;
        }
        List<DamageCategory> lowerCategoryDamageBy = damageInstance.GetApplicableDamageByCategory(damageable);
        damageInstance.iPiercingDamage.LowerDamageBy(lowerCategoryDamageBy);
    }
    #endregion

    [Serializable]
    public class Immunity
    {
        public TypeOfDamage damageType;
        /// <summary>
        /// The higher the value, the less damage will be taken
        /// </summary>
        [Range(0, 1)] public float immunityPercentage;
    }
}

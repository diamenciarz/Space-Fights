using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    private const float INVULNERABILITY_TIME = 0.25f;

    public static void ApplyDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!CanDealDamage(damageable, damageInstance))
        {
            return;
        }
        int totalDamage = GetTotalDamage(damageable, damageInstance);

        if (totalDamage > 0)
        {
            damageable.DealDamage(totalDamage);
            damageable.NotifyAboutDamage(damageInstance.dealtBy);
            HandlePiercing(damageable, damageInstance);
        }
    }

    #region Can deal damage
    private static bool CanDealDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        bool isInvulnerable = IsInvulnerableTo(damageable.GetGameObject(), damageInstance);
        if (isInvulnerable)
        {
            return false;
        }
        bool canDealDamage = damageInstance.hurtsAllies || damageable.GetTeam() != damageInstance.team;
        if (!canDealDamage)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Every unit is invulnerable to its own projectiles for 0.1 sec
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

    #region Get damage
    private static int GetTotalDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        int totalDamage = 0;
        foreach (var damageType in damageInstance.damageCategories)
        {
            int categoryDamage = GetDamageFromCategory(damageable, damageType);
            totalDamage += categoryDamage;
        }
        return totalDamage;
    }
    private static int GetDamageFromCategory(IDamageable damageable, DamageInstance.DamageCategory damageCategory)
    {
        float trueDamage = ((float)damageCategory.damage) * (1 - GetApplicableImmunityPercentage(damageable, damageCategory));
        return (int)trueDamage;
    }
    private static float GetApplicableImmunityPercentage(IDamageable damageable, DamageInstance.DamageCategory damageCategory)
    {
        List<Immunity> immunities = damageable.GetImmunities();
        foreach (var immunity in immunities)
        {
            if (immunity.damageType == damageCategory.damageType)
            {
                return immunity.immunityPercentage;
            }
        }
        return 0;
    }
    private static void HandlePiercing(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!damageInstance.isPiercing)
        {
            return;
        }
        DamageInstance.DamageCategory category = damageInstance.GetCategoryWithDamageOfType(OnCollisionDamage.TypeOfDamage.Physical);
        if (category == null)
        {
            return;
        }
        damageInstance.iPiercingDamage.LowerDamageBy(GetDamageFromCategory(damageable, category));
    }
    #endregion

    [Serializable]
    public class Immunity
    {
        public OnCollisionDamage.TypeOfDamage damageType;
        /// <summary>
        /// The higher the value, the less damage will be taken
        /// </summary>
        [Range(0, 1)] public float immunityPercentage;
    }
}

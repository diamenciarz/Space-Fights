using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    private const float INVULNERABILITY_TIME = 0.25f;

    public static void DealDamage(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!CanDealDamage(damageable, damageInstance))
        {
            return;
        }
        int totalDamage = GetTotalDamage(damageable, damageInstance);
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
    #endregion

    #region OnHit effects
    private static void HandlePiercing(IDamageable damageable, DamageInstance damageInstance)
    {
        if (!damageInstance.isPiercing)
        {
            return;
        }
        DamageInstance.DamageCategory category = damageInstance.GetCategoryWithDamageOfType(DamageInstance.TypeOfDamage.Physical);
        if (category == null)
        {
            return;
        }
        int categoryDamage = GetDamageFromCategory(damageable, category);
        int clampedDamage = Mathf.Clamp(categoryDamage, 0, damageable.GetHealth());
        damageInstance.iPiercingDamage.LowerDamageBy(clampedDamage);
    }
    #endregion

    [Serializable]
    public class Immunity
    {
        public DamageInstance.TypeOfDamage damageType;
        /// <summary>
        /// The higher the value, the less damage will be taken
        /// </summary>
        [Range(0, 1)] public float immunityPercentage;
    }
}
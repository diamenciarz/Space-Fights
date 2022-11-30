using System;
using System.Collections.Generic;
using UnityEngine;
using static DamageCalculator;
using static TeamUpdater;

/// <summary>
/// A data storing class used to conduct information about a shot to the DamageCalculator
/// </summary>
public class DamageInstance
{
    private Team team;
    public bool isAProjectile = false;
    public bool isPiercing = false;
    public bool hurtsAllies = false;
    public float lifetime;

    public List<DamageCategory> damageCategories;
    public GameObject createdBy;
    public GameObject dealtBy;
    /// <summary>
    /// Used to lower the damage of a bullet if it pierced through its target
    /// </summary>
    public IPiercingDamage iPiercingDamage;


    #region Getters
    #region Get damage
    /// <summary>
    /// Takes a Dabageable instance as input and calculates all damage that would be applied to it based on its resistances
    /// </summary>
    /// <param name="damageable"></param>
    /// <returns></returns>
    public int GetTotalApplicableDamage(IDamageable damageable)
    {
        int totalDamage = 0;
        foreach (var damageType in damageCategories)
        {
            int categoryDamage = GetDamageFromCategory(damageable, damageType);
            totalDamage += categoryDamage;
        }
        return totalDamage;
    }
    public List<DamageCategory> GetApplicableDamageByCategory(IDamageable damageable)
    {
        List<DamageCategory> damageByCategory = new List<DamageCategory>();
        foreach (var damageType in damageCategories)
        {
            int applicableDamage = GetDamageFromCategory(damageable, damageType);
            damageByCategory.Add(new DamageCategory(damageType.damageType, applicableDamage));
        }
        return damageByCategory;
    }
    private int GetDamageFromCategory(IDamageable damageable, DamageCategory damageCategory)
    {
        float trueDamage = ((float)damageCategory.damage) * (1 - GetApplicableImmunityPercentage(damageable, damageCategory));
        return (int)trueDamage;
    }
    private float GetApplicableImmunityPercentage(IDamageable damageable, DamageCategory damageCategory)
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
    public bool ContainsTypeOfDamage(TypeOfDamage typeOfDamage)
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
    public DamageCategory GetCategoryWithDamageOfType(TypeOfDamage typeOfDamage)
    {
        foreach (var category in damageCategories)
        {
            if (category.damageType == typeOfDamage)
            {
                return category;
            }
        }
        return null;
    }
    public Team GetTeam()
    {
        return team;
    }
    #endregion

    #region Setters
    public void SetTeam(Team team)
    {
        this.team = new Team(team);
    }
    #endregion

    [Serializable]
    public class DamageCategory
    {
        public DamageCategory()
        {

        }
        public DamageCategory(TypeOfDamage damageType, int damage)
        {
            this.damageType = damageType;
            this.damage = damage;
        }
        public TypeOfDamage damageType;
        public int damage;
    }
    public enum TypeOfDamage
    {
        Explosion,
        Physical,
        Fire
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
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

    public void SetTeam(Team team)
    {
        this.team = new Team(team);
    }
    public Team GetTeam()
    {
        return team;
    }
    
    [Serializable]
    public class DamageCategory
    {
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

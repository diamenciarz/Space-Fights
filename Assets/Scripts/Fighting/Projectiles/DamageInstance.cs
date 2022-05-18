using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInstance : MonoBehaviour
{
    public int team = -1;
    public bool isAProjectile = false;
    public bool isPiercing = false;
    public bool hurtsAllies = false;
    public float lifetime;

    public List<DamageCategory> damageCategories;
    public GameObject createdBy;
    public GameObject dealtBy;
    public IPiercingDamage piercingDamage;

    public bool ContainsTypeOfDamage(OnCollisionDamage.TypeOfDamage typeOfDamage)
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
    public class DamageCategory
    {
        public OnCollisionDamage.TypeOfDamage damageType;
        public int damage;
    }
}

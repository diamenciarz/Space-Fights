using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageDealer
{
    /// <summary>
    /// Allows the damage dealer to check, if damage should be dealt
    /// </summary>
    /// <param name="damageReceiver"></param>
    /// <returns></returns>
    public abstract DamageInstance GetDamageInstance();
    public abstract bool DamageCategoryContains(OnCollisionDamage.TypeOfDamage typeOfDamage);
}

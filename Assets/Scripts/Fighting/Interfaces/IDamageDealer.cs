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
    public abstract int GetDamage();
    public abstract int GetTeam();
    public abstract List<OnCollisionDamage.TypeOfDamage> GetDamageTypes();
    public abstract bool DamageTypeContains(OnCollisionDamage.TypeOfDamage damageType);
    public abstract bool IsAProjectile();
    /// <summary>
    /// The object that dealt this damage 
    /// </summary>
    /// <returns></returns>
    public abstract GameObject CreatedBy();
}

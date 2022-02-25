using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReceived
{
    public abstract int GetDamage();
    public abstract int GetTeam();
    public abstract List<OnCollisionDamage.TypeOfDamage> GetDamageTypes();
    public abstract bool DamageTypeContains(OnCollisionDamage.TypeOfDamage damageType);
    public abstract bool IsAProjectile();
    public abstract Vector3 GetVelocityVector3();
    /// <summary>
    /// The object that dealt this damage 
    /// </summary>
    /// <returns></returns>
    public abstract GameObject CreatedBy();
}

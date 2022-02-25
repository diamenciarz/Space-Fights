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
    public abstract bool GetIsPushing();
    /// <summary>
    /// Where the magnitude is the force and the direction is the velocity normalized
    /// </summary>
    /// <returns></returns>
    public abstract Vector3 GetPushVector(Vector3 colisionPosition);
    /// <summary>
    /// The object that dealt this damage 
    /// </summary>
    /// <returns></returns>
    public abstract GameObject CreatedBy();
}

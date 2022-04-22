using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public abstract GameObject GetCreatedBy();
    public abstract int GetTeam();
    public abstract bool DealDamage(int damage);
    public abstract int GetHealth();
    /// <summary>
    /// Returns true, if the target has successfully received the damage.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public abstract bool HandleDamage(IDamageDealer iDamageReceived);
    void DoFullHeal();
}

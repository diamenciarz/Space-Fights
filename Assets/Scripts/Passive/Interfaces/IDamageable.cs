using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TeamUpdater;

public interface IDamageable
{
    #region Accessor methods
    public abstract Team GetTeam();
    public abstract int GetHealth();
    public abstract GameObject GetCreatedBy();
    public abstract GameObject GetGameObject();
    public abstract List<DamageCalculator.Immunity> GetImmunities();
    #endregion
    
    #region Mutator methods
    /// <summary>
    /// Returns true, if the target has successfully received the damage.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public abstract bool DealDamage(int damage);
    public abstract void DoFullHeal();
    /// <summary>
    /// This is used by behaviour scripts that can react to being attacked by an entity
    /// </summary>
    /// <param name="damagedBy"></param>
    public abstract void NotifyAboutDamage(GameObject damagedBy);
    #endregion
}

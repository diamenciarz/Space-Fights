using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPiercingDamage
{
    public abstract void LowerDamageBy(List<DamageInstance.DamageCategory> change);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollision
{
    public abstract int GetMass();
    public abstract Vector2 GetVelocity();
    public abstract bool DealsDamageOnCollision();
}

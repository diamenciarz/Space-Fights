using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModifiableStartingSpeed
{
    public abstract bool ShouldModifyVelocity();
    public abstract void IncreaseStartingSpeed(Vector2 deltaVelocity);
}

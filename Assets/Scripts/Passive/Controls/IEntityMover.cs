using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityMover
{
    public abstract void SetInputVector(Vector2 inputVector);
    public abstract float GetMaxSpeed();
}

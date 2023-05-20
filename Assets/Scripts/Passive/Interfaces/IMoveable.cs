using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable
{
    public float GetAngularAcceleration();
    public Vector2 GetAcceleration();
}

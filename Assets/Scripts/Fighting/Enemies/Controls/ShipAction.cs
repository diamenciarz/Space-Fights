using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShipAction : ScriptableObject
{
    public abstract void callAction(Rigidbody2D rigidbody2D);
}

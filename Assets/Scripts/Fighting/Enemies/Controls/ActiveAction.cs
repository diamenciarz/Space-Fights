using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveAction : ShipAction
{
    public abstract void applyAction();
    public override void callAction(Rigidbody2D rigidbody2D)
    {

    }
}

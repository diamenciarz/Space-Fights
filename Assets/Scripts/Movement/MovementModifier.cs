using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementModifier : ScriptableObject
{
    public abstract void modifyRigidbody(Rigidbody2D rb);
}

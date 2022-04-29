using UnityEngine;


public abstract class MoveAction : ScriptableObject
{
    public abstract void applyAction(Rigidbody2D rigidbody2D);
}

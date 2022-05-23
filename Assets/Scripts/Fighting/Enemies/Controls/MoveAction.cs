using UnityEngine;


public abstract class MoveAction : ShipAction
{
    public abstract void applyAction(Rigidbody2D rigidbody2D, EntityMover entityMover);

    public override void callAction(Rigidbody2D rigidbody2D, EntityMover entityMover)
    {
        applyAction(rigidbody2D, entityMover);
    }
}

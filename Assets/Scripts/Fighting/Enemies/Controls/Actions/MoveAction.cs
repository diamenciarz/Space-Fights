using UnityEngine;


public abstract class MoveAction : ShipAction
{
    public abstract void applyAction(EntityInput.ActionData actionData);

    public override void callAction(EntityInput.ActionData actionData)
    {
        applyAction(actionData);
    }
}

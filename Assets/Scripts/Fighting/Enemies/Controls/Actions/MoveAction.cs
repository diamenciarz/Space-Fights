using UnityEngine;


public abstract class MoveAction : ShipAction
{
    public abstract void ApplyAction(EntityInput.ActionData actionData);

    public override void callAction(EntityInput.ActionData actionData)
    {
        ApplyAction(actionData);
    }
}

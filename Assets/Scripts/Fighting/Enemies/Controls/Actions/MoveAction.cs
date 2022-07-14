using UnityEngine;
using static EntityInput;

public abstract class MoveAction : ShipAction
{
    public abstract void ApplyAction(ActionData actionData);

    public override void CallAction(ActionData actionData)
    {
        ApplyAction(actionData);
    }
}

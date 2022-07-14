using UnityEngine;
using static EntityInput;

public abstract class ShipAction : ScriptableObject
{
    /// <summary>
    /// Applies the action's effect to the ship given in "Action Data".
    /// </summary>
    /// <param name="actionData"></param>
    public abstract void CallAction(ActionData actionData);
}

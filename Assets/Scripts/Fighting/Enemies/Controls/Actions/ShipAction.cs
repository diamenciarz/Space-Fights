using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityInput;

public abstract class ShipAction : ScriptableObject
{
    public abstract void callAction(ActionData actionData);
}

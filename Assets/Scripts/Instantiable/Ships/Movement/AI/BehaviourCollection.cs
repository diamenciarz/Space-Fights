using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourCollection : ScriptableObject
{
    public abstract MovementBehaviour GetMovementBehaviour(ConditionData data);
}

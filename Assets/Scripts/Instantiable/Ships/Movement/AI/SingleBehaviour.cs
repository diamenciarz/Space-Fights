using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleBehaviour", menuName = "AI/Behaviours/SingleBehaviour")]
public class SingleBehaviour : BehaviourCollection
{
    public MovementBehaviour movementBehaviour;

    public override MovementBehaviour GetMovementBehaviour(ConditionData data)
    {
        return movementBehaviour;
    }
}

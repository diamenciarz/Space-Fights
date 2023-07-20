using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourTree", menuName = "AI/Behaviours/BehaviourTree")]
public class BehaviourTree : BehaviourCollection
{
    [SerializeField] BehaviourCollection ifBehaviour;
    [SerializeField] BehaviourCollection elseBehaviour;
    [SerializeField] Condition condition;

    public override MovementBehaviour GetMovementBehaviour(ConditionData data)
    {
        if (condition.IsSatisfied(data))
        {
            return ifBehaviour.GetMovementBehaviour(data);
        }
        else
        {
            return elseBehaviour.GetMovementBehaviour(data);
        }
    }
}

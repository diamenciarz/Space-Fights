using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourList", menuName = "AI/Behaviours/BehaviourList")]
public class BehaviourList : BehaviourCollection
{
    [SerializeField] BehaviourCollection[] behaviours;
    [SerializeField] Condition[] conditions;

    private int behaviourIndex;
    private float lastBehaviourChangeTime;
    private bool conditionChanged = false;
    public override MovementBehaviour GetMovementBehaviour(ConditionData data)
    {
        if(behaviours == null)
        {
            return null;
        }
        // Method order matters
        UpdateBehaviourIndex(data);
        UpdateData(data);
        return behaviours[behaviourIndex].GetMovementBehaviour(data);
    }
    private void UpdateBehaviourIndex(ConditionData data)
    {
        if (behaviours.Length == 1)
        {
            return;
        }
        if (conditions[behaviourIndex].IsSatisfied(data))
        {
            behaviourIndex++;
            conditionChanged = true;
            if (behaviourIndex == behaviours.Length)
            {
                behaviourIndex = 0;
            }
            lastBehaviourChangeTime = Time.time;
        }
    }
    private void UpdateData(ConditionData data)
    {
        data.firstConditionCall = conditionChanged;
        data.lastBehaviourChangeTime = lastBehaviourChangeTime;
    }
}

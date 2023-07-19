using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Timeout", menuName = "AI/Conditions/Timeout")]
[Serializable]
public class TimeoutCondition : Condition
{
    [SerializeField] float timeoutDelay;
    public override bool IsSatisfied(ConditionData data)
    {
        float timePassed = Time.time - data.lastBehaviourChangeTime;
        return timePassed > timeoutDelay;
    }
}

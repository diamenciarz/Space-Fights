using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomTimeout", menuName = "AI/Conditions/RandomTimeout")]
[Serializable]
public class RandomTimeoutCondition : Condition
{
    [SerializeField] float minTimeoutDelay;
    [SerializeField] float maxTimeoutDelay;

    private float time = -1;
    private ConditionData data;
    public override bool IsSatisfied(ConditionData newData)
    {
        data = newData;
        GenerateTime();
        float timePassed = Time.time - data.lastBehaviourChangeTime;
        return timePassed > time;
    }
    private void GenerateTime()
    {
        if(data.firstConditionCall == true)
        {
            time = UnityEngine.Random.Range(minTimeoutDelay, maxTimeoutDelay);
        }
    }
}

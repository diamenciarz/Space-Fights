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

    private Dictionary<GameObject, float> times = new Dictionary<GameObject, float>();
    public override bool IsSatisfied(ConditionData data)
    {
        GenerateTime(data);
        float timePassed = Time.time - data.lastBehaviourChangeTime;
        return timePassed > times[data.gameObject];
    }
    private void GenerateTime(ConditionData data)
    {
        if(data.firstConditionCall == true)
        {
            times[data.gameObject] = UnityEngine.Random.Range(minTimeoutDelay, maxTimeoutDelay);
            Debug.Log("New time: " + times[data.gameObject]);
        }
    }
}

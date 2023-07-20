using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourList", menuName = "AI/Behaviours/BehaviourList")]
public class BehaviourList : BehaviourCollection
{
    [SerializeField] BehaviourCollection[] behaviours;
    [SerializeField] Condition[] conditions;

    private Dictionary<GameObject, int> indexes = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, float> lastBehaviourChangeTimes = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> conditionChanged = new Dictionary<GameObject, bool>();
    public override MovementBehaviour GetMovementBehaviour(ConditionData data)
    {
        if(behaviours == null)
        {
            return null;
        }
        // Method order matters
        EnsureIndexPresent(data);
        UpdateData(data);
        UpdateBehaviourIndex(data);
        return behaviours[indexes[data.gameObject]].GetMovementBehaviour(data);
    }
    private void EnsureIndexPresent(ConditionData data)
    {
        if (!indexes.ContainsKey(data.gameObject))
        {
            indexes.Add(data.gameObject, 0);
        }
        if (!lastBehaviourChangeTimes.ContainsKey(data.gameObject))
        {
            lastBehaviourChangeTimes.Add(data.gameObject, Time.time);
        }
        if (!conditionChanged.ContainsKey(data.gameObject))
        {
            conditionChanged.Add(data.gameObject, true);
        }
    }
    private void UpdateBehaviourIndex(ConditionData data)
    {
        if (behaviours.Length == 1)
        {
            return;
        }
        
        if (conditions[indexes[data.gameObject]].IsSatisfied(data))
        {
            indexes[data.gameObject]++;
            conditionChanged[data.gameObject] = true;
            if (indexes[data.gameObject] == behaviours.Length)
            {
                indexes[data.gameObject] = 0;
            }
            lastBehaviourChangeTimes[data.gameObject] = Time.time;
        }
    }
    private void UpdateData(ConditionData data)
    {
        data.firstConditionCall = conditionChanged[data.gameObject];
        data.lastBehaviourChangeTime = lastBehaviourChangeTimes[data.gameObject];
        conditionChanged[data.gameObject] = false;
    }
}

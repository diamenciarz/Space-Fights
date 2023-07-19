using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : ScriptableObject
{
    public abstract bool IsSatisfied(ConditionData data);
}

public class ConditionData
{
    public GameObject gameObject;
    public float lastBehaviourChangeTime;
}
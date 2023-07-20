using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TeamUpdater;

public abstract class Condition : ScriptableObject
{
    public abstract bool IsSatisfied(ConditionData data);
}

public class ConditionData
{
    public GameObject gameObject;
    public GameObject target;
    public float lastBehaviourChangeTime;
    public bool firstConditionCall;
    public int gunCount;
    public float shipSize;
    public Team team;
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;

[CreateAssetMenu(fileName = "GunCount", menuName = "AI/Conditions/GunCount")]
[Serializable]
public class GunCountCondition : Condition
{
    public override bool IsSatisfied(ConditionData data)
    {
        return data.gunCount == 0;
    }
}
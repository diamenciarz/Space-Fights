using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;

[CreateAssetMenu(fileName = "ObjectTypeCount", menuName = "AI/Conditions/ObjectTypeCountInRange")]
[Serializable]
public class ObjectTypeCountCondition : Condition
{
    [SerializeField] int minObjectCount;
    [SerializeField] float range;
    [SerializeField] ObjectTypes objectType;    

    public override bool IsSatisfied(ConditionData data)
    {
        List<GameObject> enemies = ListContents.Generic.GetObjectList(objectType);

        List<GameObject> enemiesWithinRange = HelperMethods.ObjectUtils.GetGameObjectsWithinRange(enemies, data.gameObject.transform.position, range);
        return enemiesWithinRange.Count >= minObjectCount;
    }
}
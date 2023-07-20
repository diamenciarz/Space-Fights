using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;

[CreateAssetMenu(fileName = "TeamCount", menuName = "AI/Conditions/TeamCountInRange")]
[Serializable]
public class TeamCountCondition : Condition
{
    [SerializeField] int minObjectCount;
    [SerializeField] float range;
    [SerializeField] TeamMember teamMember;

    enum TeamMember
    {
        Ally,
        Enemy
    }

    public override bool IsSatisfied(ConditionData data)
    {
        List<GameObject> objects = null;
        if(teamMember == TeamMember.Ally)
        {
            objects = ListContents.Allies.GetAllyList(data.team, data.gameObject);
        }
        else
        {
            objects = ListContents.Enemies.GetEnemyList(data.team);
        }

        List<GameObject> enemiesWithinRange = HelperMethods.ObjectUtils.GetGameObjectsWithinRange(objects, data.gameObject.transform.position, range);
        return enemiesWithinRange.Count >= minObjectCount;
    }
}
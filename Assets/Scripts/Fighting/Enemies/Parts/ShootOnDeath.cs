using System.Collections;
using UnityEngine;

public class ShootOnDeath : TriggerOnDeath
{
    [Header("Upon Breaking")]
    [SerializeField] SingleShotScriptableObject shot;
    [Tooltip("If true, the shot will target the closest enemy. If false, will shoot forward")]
    [SerializeField] protected bool targetEnemies;

    private bool isDestroyed;
    protected float deltaRotationToTarget = -90;

    protected override void Awake()
    {
        base.Awake();
    }

    #region OnDestroy
    public override void DoDestroyAction()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CreateShot();
        }
    }
    private void CreateShot()
    {
        SummonedShotData data = new SummonedShotData();
        data.summonRotation = transform.rotation;
        data.summonPosition = transform.position;
        data.team = team;
        data.createdBy = createdBy;
        data.shot = shot;
        data.target = GetShotTarget();

        EntityCreator.SummonShot(data);
    }
    private GameObject GetShotTarget()
    {
        if (!targetEnemies)
        {
            return null;
        }
        else
        {
            return StaticDataHolder.GetClosestEnemyInSight(transform.position, team);
        }
    }
    #endregion
}

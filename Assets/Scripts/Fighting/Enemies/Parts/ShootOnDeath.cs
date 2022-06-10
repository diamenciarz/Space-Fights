using UnityEngine;

public class ShootOnDeath : TriggerOnDeath
{
    [Header("Upon Breaking")]
    [SerializeField] SingleShotScriptableObject shot;
    [Tooltip("The delta direction from the direction that the object had when it got destroyed")]
    [SerializeField] float directionOffset;
    [Tooltip("If true, the shot will target the closest enemy. If false, will shoot forward")]
    [SerializeField] protected bool targetEnemies;
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypes;

    private bool isDestroyed;

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
        data.summonRotation = transform.rotation * Quaternion.Euler(0,0, directionOffset);
        data.summonPosition = transform.position;
        data.SetTeam(team);
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
            return StaticDataHolder.ListContents.Enemies.GetClosestEnemyInSight(transform.position, team);
        }
    }
    #endregion
}

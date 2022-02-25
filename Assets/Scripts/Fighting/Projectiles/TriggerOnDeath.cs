using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOnDeath : TeamUpdater
{
    [Header("Upon Breaking")]
    [SerializeField] SingleShotScriptableObject shot;
    [Tooltip("If true, the shot will target the closest enemy. If false, will shoot forward")]
    [SerializeField] protected bool targetEnemies;

    protected EntityCreator entityCreator;
    private bool isDestroyed;
    protected float deltaRotationToTarget = -90;

    protected override void Awake()
    {
        base.Awake();
        entityCreator = FindObjectOfType<EntityCreator>();
    }

    #region OnDestroy
    public void DestroyObject()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CreateShot();
            StartCoroutine(DestroyAtTheEndOfFrame());
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

        entityCreator.SummonShot(data);
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
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion
}

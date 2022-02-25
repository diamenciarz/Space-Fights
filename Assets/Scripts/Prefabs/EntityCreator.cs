using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    [Header("Projectiles")]
    public GameObject laserPrefab;
    public GameObject splittingBulletPrefab;
    public GameObject rocketPrefab;
    public GameObject bombPrefab;
    public GameObject bombExplosionPrefab;
    public GameObject grenadePrefab;
    public GameObject grenadeExplosionPrefab;
    public GameObject bouncyLaserPrefab;

    [Header("Enemies")]
    public GameObject walkerPrefab;

    public enum BulletTypes
    {
        Nothing,
        SplittingBullet,
        Laser,
        Rocket,
        Bomb,
        BombExplosion,
        Grenade,
        GrenadeExplosion,
        BouncyLaser
    }
    public enum EntityTypes
    {
        Walker
    }
    #region Salvos
    public void SummonShot(SummonedShotData data)
    {
        if (!data.shot)
        {
            if (data.createdBy)
            {
                Debug.LogError("Salvo created by " + data.createdBy + " was null!");
            }
            return;
        }
        //Play shot sound and create projectiles one by one
        PlayShotSound(data.shot);
        for (int i = 0; i < data.shot.projectilesToCreateList.Count; i++)
        {
            CreateProjectile(data, i);
        }
    }

    private void CreateProjectile(SummonedShotData data, int i)
    {
        SummonedProjectileData newData = new SummonedProjectileData();
        newData.bulletType = data.shot.projectilesToCreateList[i];
        newData.summonPosition = data.summonPosition;
        newData.summonRotation = data.summonRotation * CountDeltaRotation(data, i);
        newData.team = data.team;
        newData.createdBy = data.createdBy;
        newData.target = data.target;

        SummonProjectile(newData);
    }

    #region Count shot rotation
    private Quaternion CountDeltaRotation(SummonedShotData data, int index)
    {
        if (data.target)
        {
            return ShootAtTarget(data.shot, data.target.transform.position, index);
        }
        else
        {
            return ShootAtNoTarget(data.shot, index);
        }
    }
    private Quaternion ShootAtTarget(SingleShotScriptableObject shot, Vector3 targetPosition, int i)
    {
        if (shot.spreadProjectilesEvenly)
        {
            return RotToPosRegularSpread(shot, i, targetPosition);
        }
        else
        {
            return RotToPosRandomSpread(shot, targetPosition);
        }
    }
    private Quaternion ShootAtNoTarget(SingleShotScriptableObject shot, int i)
    {
        if (shot.spreadProjectilesEvenly)
        {
            return RotForwardRegularSpread(shot, i);
        }
        else
        {
            return RotForwardRandomSpread(shot);
        }
    }
    private Quaternion RotForwardRandomSpread(SingleShotScriptableObject shot)
    {
        return HelperMethods.RandomRotationInRange(shot.leftBulletSpread, shot.rightBulletSpread);
    }
    private Quaternion RotForwardRegularSpread(SingleShotScriptableObject shot, int index)
    {
        float bulletOffset = (shot.spreadDegrees * (index - (shot.projectilesToCreateList.Count - 1f) / 2));
        return Quaternion.Euler(0, 0, bulletOffset);
    }
    private Quaternion RotToPosRandomSpread(SingleShotScriptableObject shot, Vector3 shootAtPosition)
    {
        Quaternion randomDeltaRotation = HelperMethods.RandomRotationInRange(shot.leftBulletSpread, shot.rightBulletSpread);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(transform.position, shootAtPosition);
        return randomDeltaRotation * rotationToTarget;
    }
    private Quaternion RotToPosRegularSpread(SingleShotScriptableObject shot, int index, Vector3 shootAtPosition)
    {
        float bulletOffset = (shot.spreadDegrees * (index - (shot.projectilesToCreateList.Count - 1f) / 2));
        Quaternion deltaRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(transform.position, shootAtPosition);
        return deltaRotation * rotationToTarget;
    }
    #endregion

    #endregion

    #region Projectiles
    public void SummonProjectile(SummonedProjectileData data)
    {
        GameObject bulletToSummon = GetProjectilePrefab(data.bulletType);
        if (bulletToSummon != null)
        {
            GameObject summonedBullet = Instantiate(bulletToSummon, data.summonPosition, data.summonRotation);
            SetupStartingValues(summonedBullet, data.team, data.createdBy);
            CheckForRocket(summonedBullet, data);
        }
    }
    private GameObject GetProjectilePrefab(BulletTypes bulletType)
    {
        if (bulletType == BulletTypes.Laser)
        {
            return laserPrefab;
        }
        if (bulletType == BulletTypes.SplittingBullet)
        {
            return splittingBulletPrefab;
        }
        if (bulletType == BulletTypes.Rocket)
        {
            return rocketPrefab;
        }
        if (bulletType == BulletTypes.Bomb)
        {
            return bombPrefab;
        }
        if (bulletType == BulletTypes.BombExplosion)
        {
            return bombExplosionPrefab;
        }
        if (bulletType == BulletTypes.Grenade)
        {
            return grenadePrefab;
        }
        if (bulletType == BulletTypes.GrenadeExplosion)
        {
            return grenadeExplosionPrefab;
        }
        if (bulletType == BulletTypes.BouncyLaser)
        {
            return bouncyLaserPrefab;
        }
        return null;
    }
    #endregion

    #region Entities
    public void SummonEntity(SummonedEntityData data)
    {
        GameObject entityToSummon = GetEntityPrefab(data.entityType);
        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);

        SetupStartingValues(summonedEntity, data.team, data.parent);
    }
    private GameObject GetEntityPrefab(EntityTypes entityType)
    {
        if (entityType == EntityTypes.Walker)
        {
            return walkerPrefab;
        }
        return null;
    }
    #endregion

    #region Helper methods
    private void SetupStartingValues(GameObject summonedObject, int team, GameObject parent)
    {
        TeamUpdater[] teamUpdaters = summonedObject.GetComponentsInChildren<TeamUpdater>();
        if (teamUpdaters.Length != 0)
        {
            foreach (TeamUpdater item in teamUpdaters)
            {
                item.UpdateTeam(team);
                SetCreatedBy(item, parent);
            }
        }
    }
    private void SetCreatedBy(TeamUpdater item, GameObject createdBy)
    {
        if (createdBy)
        {
            item.SetCreatedBy(createdBy);
        }
        else
        {
            item.SetCreatedBy(item.gameObject);
        }
    }
    private void CheckForRocket(GameObject summonedObject, SummonedProjectileData data)
    {
        if (data.target)
        {
            RocketController rocketController = summonedObject.GetComponent<RocketController>();
            if (rocketController)
            {
                rocketController.SetTarget(data.target);
            }
        }
    }

    #region Unused
    private bool CanFitSummon(GameObject summonedObject)
    {
        Vector3 dir = HelperMethods.DirectionVectorNormalized(transform.rotation.eulerAngles.z);
        ContactFilter2D filter = CreateObstacleContactFilter();
        RaycastHit2D[] hits = new RaycastHit2D[0];

        Collider2D collider = summonedObject.GetComponent<Collider2D>();
        collider.Cast(dir, filter, hits);

        bool hitSomeObstacle = hits.Length != 0;
        if (hitSomeObstacle)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private ContactFilter2D CreateObstacleContactFilter()
    {
        LayerMask layerMask = LayerMask.GetMask("Obstacles");
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(layerMask);

        return filter;
    }
    #endregion

    #endregion

    #region Sounds
    private void PlayShotSound(SingleShotScriptableObject shot)
    {
        if (shot.shotSounds.Length != 0)
        {
            AudioClip sound = shot.shotSounds[Random.Range(0, shot.shotSounds.Length)];
            StaticDataHolder.PlaySound(sound, transform.position, shot.shotSoundVolume);
        }
    }
    #endregion
}
public class SummonedShotData
{
    public SingleShotScriptableObject shot;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject createdBy;
    [Tooltip("If null, will shoot forward")]
    public GameObject target;
}
public class SummonedProjectileData
{
    public EntityCreator.BulletTypes bulletType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject createdBy;
    public GameObject target;
}
public class SummonedEntityData
{
    public EntityCreator.EntityTypes entityType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject parent;
    public GameObject target;
}
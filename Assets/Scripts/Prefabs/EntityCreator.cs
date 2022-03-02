using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    public enum EntityTypes
    {
        Nothing,
        SplittingBullet,
        Laser,
        Rocket,
        Bomb,
        BombExplosion,
        Grenade,
        GrenadeExplosion,
        BouncyLaser,
        Ship
    }

    #region Salvos
    public static void SummonShot(SummonedShotData data)
    {
        if (IsShotNull(data))
        {
            return;
        }
        //Play shot sound and create projectiles one by one
        PlayShotSound(data);
        for (int i = 0; i < data.shot.projectilesToCreateList.Count; i++)
        {
            CreateProjectile(data, i);
        }
    }

    #region Null Check
    private static bool IsShotNull(SummonedShotData data)
    {
        if (!data.shot)
        {
            LogError(data);
            return true;
        }
        return false;

    }
    private static void LogError(SummonedShotData data)
    {
        if (data.createdBy)
        {
            Debug.LogError("Salvo created by " + data.createdBy + " was null!");
        }
    }
    #endregion

    private static void CreateProjectile(SummonedShotData data, int i)
    {
        SummonedEntityData newData = new SummonedEntityData();
        newData.summonPosition = data.summonPosition;
        newData.summonRotation = data.summonRotation * CountDeltaRotation(data, i);
        newData.team = data.team;
        newData.createdBy = data.createdBy;
        newData.target = data.target;

        EntityTypes bulletType = data.shot.projectilesToCreateList[i];

        Summon(newData, bulletType);
    }

    #region Count shot rotation
    private static Quaternion CountDeltaRotation(SummonedShotData data, int index)
    {
        if (data.target)
        {
            return ShootAtTarget(data, index);
        }
        else
        {
            return ShootAtNoTarget(data, index);
        }
    }
    private static Quaternion ShootAtTarget(SummonedShotData data, int i)
    {
        if (data.shot.spreadProjectilesEvenly)
        {
            return RotToPosRegularSpread(data, i);
        }
        else
        {
            return RotToPosRandomSpread(data);
        }
    }
    private static Quaternion ShootAtNoTarget(SummonedShotData data, int i)
    {
        if (data.shot.spreadProjectilesEvenly)
        {
            return RotForwardRegularSpread(data, i);
        }
        else
        {
            return RotForwardRandomSpread(data);
        }
    }

    //Count rotation
    private static Quaternion RotForwardRandomSpread(SummonedShotData data)
    {
        return HelperMethods.RandomRotationInRange(data.shot.leftBulletSpread, data.shot.rightBulletSpread);
    }
    private static Quaternion RotForwardRegularSpread(SummonedShotData data, int index)
    {
        float bulletOffset = (data.shot.spreadDegrees * (index - (data.shot.projectilesToCreateList.Count - 1f) / 2));
        return Quaternion.Euler(0, 0, bulletOffset);
    }
    private static Quaternion RotToPosRandomSpread(SummonedShotData data)
    {
        Quaternion randomDeltaRotation = HelperMethods.RandomRotationInRange(data.shot.leftBulletSpread, data.shot.rightBulletSpread);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(data.summonPosition, data.target.transform.position);
        return randomDeltaRotation * rotationToTarget;
    }
    private static Quaternion RotToPosRegularSpread(SummonedShotData data, int index)
    {
        float bulletOffset = (data.shot.spreadDegrees * (index - (data.shot.projectilesToCreateList.Count - 1f) / 2));
        Quaternion deltaRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(data.summonPosition, data.target.transform.position);
        return deltaRotation * rotationToTarget;
    }
    #endregion

    #endregion

    #region Entities
    public static void Summon(SummonedEntityData data, EntityTypes entityType)
    {
        GameObject entityToSummon = EntityFactory.GetPrefab(entityType);
        if (entityToSummon != null)
        {
            SummonEntity(data, entityToSummon);
        }
    }
    public static void SummonEntity(SummonedEntityData data, GameObject entityToSummon)
    {
        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);
        CheckForRocket(summonedEntity, data);

        SetupStartingValues(summonedEntity, data.team, data.createdBy);
    }
    #endregion

    #region Helper methods
    private static void SetupStartingValues(GameObject summonedObject, int team, GameObject parent)
    {
        IParent teamUpdater = summonedObject.GetComponent<IParent>();
        if (teamUpdater == null)
        {
            return;
        }
        teamUpdater.SetTeam(team);
        SetCreatedBy(summonedObject, parent);
    }
    private static void SetCreatedBy(GameObject summonedObject, GameObject createdBy)
    {
        IParent iParent = summonedObject.GetComponent<IParent>();
        if (createdBy)
        {
            iParent.SetCreatedBy(createdBy);
        }
        else
        {
            iParent.SetCreatedBy(summonedObject.gameObject);
        }
    }
    private static void CheckForRocket(GameObject summonedObject, SummonedEntityData data)
    {
        if (!data.target)
        {
            return;
        }

        RocketController rocketController = summonedObject.GetComponent<RocketController>();
        if (rocketController)
        {
            rocketController.SetTarget(data.target);
        }
    }

    #endregion

    #region Sounds
    private static void PlayShotSound(SummonedShotData data)
    {
        SingleShotScriptableObject shotSO = data.shot;
        if (shotSO.shotSounds.Length == 0)
        {
            return;
        }

        AudioClip sound = shotSO.shotSounds[UnityEngine.Random.Range(0, shotSO.shotSounds.Length)];
        StaticDataHolder.PlaySound(sound, data.summonPosition, shotSO.shotSoundVolume);
    }
    #endregion
}

#region Data types
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
public class SummonedEntityData
{
    public EntityCreator.EntityTypes entityType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject createdBy;
    public GameObject target;

}
#endregion


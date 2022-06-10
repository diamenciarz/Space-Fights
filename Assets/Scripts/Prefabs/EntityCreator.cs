using UnityEngine;
using static TeamUpdater;

public class EntityCreator : MonoBehaviour
{
    public enum Projectiles
    {
        Nothing,
        Splitting_Bullet,
        Laser,
        Rocket,
        Rocket_Explosion,
        Bomb,
        Bomb_Explosion,
        Grenade,
        Grenade_Explosion,
        Bouncy_Laser,
        SmallRock,
        TinyRock,
        Invisible_Explosion,
        AntiRocket,
        AntiBomb_Explosion
    }
    public enum Entities
    {
        Ship
    }
    public enum ProgressionBars
    {
        HealthBar,
        AmmoBar
    }
    public enum ObjectMissingIcons
    {
        RocketMissingIcon,
        AntirocketMissingIcon,
        ShipMissingIcon,
        TargetIcon
    }
    private void Awake()
    {
        EntityFactory.InitializeFactory();
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
        GameObject bulletToSummon = GetBulletToSummon(data, i);
        if (bulletToSummon == null)
        {
            return;
        }

        SummonedEntityData newData = new SummonedEntityData();
        newData.summonPosition = data.summonPosition;
        newData.summonRotation = CountSummonRotation(data, i);
        newData.SetTeam(data.GetTeam());
        newData.createdBy = data.createdBy;
        newData.target = data.target;

        SummonEntity(newData, bulletToSummon);
    }

    private static GameObject GetBulletToSummon(SummonedShotData data, int i)
    {
        Projectiles bulletType = data.shot.projectilesToCreateList[i];
        GameObject bulletToSummon = EntityFactory.GetPrefab(bulletType);
        if (IsEntityNull(bulletToSummon, bulletType))
        {
            return null;
        }
        return bulletToSummon;
    }

    #region Count shot rotation
    private static Quaternion CountSummonRotation(SummonedShotData data, int i)
    {
        /*
        if (data.target)
        {
            return ShootAtTarget(data, i);
        }
        */
        return data.summonRotation * ShootAtNoTarget(data, i);
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
    private static Quaternion RotForwardRegularSpread(SummonedShotData data, int index)
    {
        float bulletOffset = (data.shot.spreadDegrees * (index - (data.shot.projectilesToCreateList.Count - 1f) / 2));
        return Quaternion.Euler(0, 0, bulletOffset);
    }
    private static Quaternion RotForwardRandomSpread(SummonedShotData data)
    {
        return HelperMethods.RotationUtils.RandomRotationInRange(data.shot.leftBulletSpread, data.shot.rightBulletSpread);
    }
    private static Quaternion RotToPosRandomSpread(SummonedShotData data)
    {
        Quaternion randomDeltaRotation = HelperMethods.RotationUtils.RandomRotationInRange(data.shot.leftBulletSpread, data.shot.rightBulletSpread);
        Quaternion rotationToTarget = HelperMethods.RotationUtils.DeltaPositionRotation(data.summonPosition, data.target.transform.position);
        Quaternion weirdOffset = Quaternion.Euler(0,0,-90);
        return randomDeltaRotation * rotationToTarget * weirdOffset;
    }
    private static Quaternion RotToPosRegularSpread(SummonedShotData data, int index)
    {
        float bulletOffset = (data.shot.spreadDegrees * (index - (data.shot.projectilesToCreateList.Count - 1f) / 2));
        Quaternion deltaRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = HelperMethods.RotationUtils.DeltaPositionRotation(data.summonPosition, data.target.transform.position);
        Quaternion weirdOffset = Quaternion.Euler(0,0,-90);
        return deltaRotation * rotationToTarget * weirdOffset;
    }
    #endregion
    #endregion

    #region Entities

    #region Null Check
    private static bool IsEntityNull(GameObject entityToSummon, Projectiles entityType)
    {
        if (entityType == Projectiles.Nothing)
        {
            return true;
            //This is supposed to be a placeholder that skips one summoning action without returning an error
        }
        if (entityToSummon == null)
        {
            Debug.LogError("Entity type " + entityType.ToString() + " couldn't be found. The name of the prefab should match the name on the list!");
            return true;
        }
        return false;
    }
    #endregion

    public static void SummonEntity(SummonedEntityData data, GameObject entityToSummon)
    {
        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);
        CheckForRocket(summonedEntity, data);

        SetupStartingValues(summonedEntity, data);
    }
    #endregion

    #region Helper methods
    private static void SetupStartingValues(GameObject summonedObject, SummonedEntityData data)
    {
        SetTeam(summonedObject, data);
        SetCreatedBy(summonedObject, data.createdBy);
        ModifyStartingSpeed(summonedObject, data);
    }
    private static void SetTeam(GameObject summonedObject, SummonedEntityData data)
    {
        IParent teamUpdater = summonedObject.GetComponent<IParent>();
        if (teamUpdater != null)
        {
            teamUpdater.SetTeam(data.GetTeam());
        }
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
    private static void ModifyStartingSpeed(GameObject summonedObject, SummonedEntityData data)
    {
        if (data.createdBy == null)
        {
            return;
        }
        IModifiableStartingSpeed iSpeed = summonedObject.GetComponent<IModifiableStartingSpeed>();
        Rigidbody2D parentRB2D = data.createdBy.GetComponent<Rigidbody2D>();
        if (iSpeed != null && parentRB2D != null)
        {
            if (iSpeed.ShouldModifyVelocity())
            {
                Vector2 parentVelocity = parentRB2D.velocity;
                Vector2 summonDirection = HelperMethods.VectorUtils.DirectionVectorNormalized(data.summonRotation.eulerAngles.z);
                Vector2 velocityInObjectDirection = HelperMethods.VectorUtils.ProjectVector(parentVelocity, summonDirection);
                float deltaVelocity = velocityInObjectDirection.magnitude;
                if (Vector2.Dot(parentVelocity, summonDirection) < 0)
                {
                    return;
                    //deltaVelocity *= -1;
                }
                iSpeed.IncreaseStartingSpeed(deltaVelocity);
            }
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
        StaticDataHolder.Sounds.PlaySound(sound, data.summonPosition, shotSO.shotSoundVolume);
    }
    #endregion
}

#region Data types
public class SummonedShotData
{
    public SingleShotScriptableObject shot;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    private Team team;
    public GameObject createdBy;
    [Tooltip("If null, will shoot forward")]
    public GameObject target;
    public Team GetTeam()
    {
        return team;
    }
    public void SetTeam(Team team)
    {
        this.team = team;
    }
}
public class SummonedEntityData
{
    public EntityCreator.Projectiles entityType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    private Team team;
    public GameObject createdBy;
    [Tooltip("If null, will shoot forward")]
    public GameObject target;
    public Team GetTeam()
    {
        return team;
    }
    public void SetTeam(Team team)
    {
        this.team = team;
    }
}
#endregion


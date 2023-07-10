using System.Collections.Generic;
using UnityEngine;
using static EntityCreator;
using static TeamUpdater;

public class EntityCreator : MonoBehaviour
{
    // Change ProjectileIsRocket, when adding more rockets
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
        Small_Rock,
        Tiny_Rock,
        Invisible_Explosion,
        AntiRocket,
        AntiRocket_Explosion
    }
    public enum Entities
    {
        Ship,
        SquareWall,
        LongWall
    }
    public enum ProgressionBars
    {
        HealthBar,
        AmmoBar,
    }
    public enum ProgressionCones
    {
        ShootingZone
    }
    public enum ObjectMissingIcons
    {
        RocketMissingIcon,
        AntirocketMissingIcon,
        ShipMissingIcon
    }
    public enum ObjectFollowers
    {
        TargetIcon
    }
    private void Awake()
    {
        EntityFactory.InitializeFactory();
    }

    #region Accessor Methods
    public static TargetFollowerProperty GetFirstTargetFollower(List<Projectiles> projectiles)
    {
        foreach (var projectile in projectiles)
        {
            GameObject obj = EntityFactory.GetPrefab(projectile);
            TargetFollowerProperty targetFollowerProperty = obj.GetComponentInChildren<TargetFollowerProperty>();
            if (targetFollowerProperty != null)
            {
                return targetFollowerProperty;
            }
        }
        return null;
    }
    private static bool ProjectileIsRocket(Projectiles projectile)
    {
        switch (projectile)
        {
            case Projectiles.Rocket:
                return true;
            case Projectiles.AntiRocket:
                return true;
            default:
                return false;
        }
    }
    #endregion

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
        Projectiles projectileToSummon = data.shot.projectilesToCreateList[i];
        if (projectileToSummon == Projectiles.Nothing)
        {
            return;
        }

        SummonedProjectileData newData = new SummonedProjectileData();
        newData.projectileType = projectileToSummon;
        newData.summonPosition = data.summonPosition;
        newData.startingVelocity = data.startingVelocity;
        newData.summonRotation = CountSummonRotation(data, i);
        newData.SetTeam(data.GetTeam());
        newData.createdBy = data.createdBy;
        newData.target = data.target;

        SummonProjectile(newData);
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
        Quaternion weirdOffset = Quaternion.Euler(0, 0, -90);
        return randomDeltaRotation * rotationToTarget * weirdOffset;
    }
    private static Quaternion RotToPosRegularSpread(SummonedShotData data, int index)
    {
        float bulletOffset = (data.shot.spreadDegrees * (index - (data.shot.projectilesToCreateList.Count - 1f) / 2));
        Quaternion deltaRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = HelperMethods.RotationUtils.DeltaPositionRotation(data.summonPosition, data.target.transform.position);
        Quaternion weirdOffset = Quaternion.Euler(0, 0, -90);
        return deltaRotation * rotationToTarget * weirdOffset;
    }
    #endregion
    #endregion

    #region Projectiles

    #region Null Check
    private static bool IsProjectileNull(GameObject entityToSummon, Projectiles entityType)
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

    public static void SummonProjectile(SummonedProjectileData data)
    {
        GameObject entityToSummon = EntityFactory.GetPrefab(data.projectileType);
        if (IsProjectileNull(entityToSummon, data.projectileType))
        {
            return;
        }

        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);
        SetRocketTarget(summonedEntity, data);

        SetupStartingProjectileValues(summonedEntity, data);
    }

    #region Helper methods
    private static void SetupStartingProjectileValues(GameObject summonedObject, SummonedProjectileData data)
    {
        SetTeam(summonedObject, data);
        SetCreatedBy(summonedObject, data.createdBy);
        ModifyStartingSpeed(summonedObject, data);
    }
    private static void SetTeam(GameObject summonedObject, SummonedProjectileData data)
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
        if (iParent != null)
        {
            if (createdBy)
            {
                iParent.SetCreatedBy(createdBy);
            }
            else
            {
                iParent.SetCreatedBy(summonedObject.gameObject);
            }
        }
    }
    private static void ModifyStartingSpeed(GameObject summonedObject, SummonedProjectileData data)
    {
        if (data.startingVelocity == null)
        {
            return;
        }
        IModifiableStartingSpeed iSpeed = summonedObject.GetComponentInParent<IModifiableStartingSpeed>();
        if (iSpeed == null)
        {
            return;
        }
        if (!iSpeed.ShouldModifyVelocity())
        {
            return;
        }
        Vector2 summonDirection = HelperMethods.VectorUtils.DirectionVectorNormalized(data.summonRotation.eulerAngles.z);
        Vector2 velocityInObjectDirection = HelperMethods.VectorUtils.ProjectVector(data.startingVelocity, summonDirection);
        float deltaVelocity = velocityInObjectDirection.magnitude;
        if (Vector2.Dot(data.startingVelocity, summonDirection) < 0)
        {
            return;
            //deltaVelocity *= -1;
        }
        iSpeed.IncreaseStartingSpeed(deltaVelocity);
    }
    private static void SetRocketTarget(GameObject summonedObject, SummonedProjectileData data)
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

    #endregion

    #region Progression Bars
    public static GameObject SummonProgressionBar(SummonedProgressionBarData data)
    {
        GameObject entityToSummon = EntityFactory.GetPrefab(data.progressionBarType);
        if (entityToSummon == null)
        {
            Debug.LogError("The prefab was null! Probably, the name does not match the enum name.");
            return null;
        }
        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);
        summonedEntity.transform.SetParent(StaticProgressionBarUpdater.UIParent.transform, true);

        SetupStartingProgressionBarValues(summonedEntity, data);
        return summonedEntity;
    }

    #region Helper methods
    private static void SetupStartingProgressionBarValues(GameObject summonedEntity, SummonedProgressionBarData data)
    {
        ProgressionBarController barScript = summonedEntity.GetComponent<ProgressionBarController>();
        barScript.SetDeltaPositionToObject(data.barDeltaPosition);
        barScript.SetHideDelay(data.hideDelay);
        barScript.SetIsAlwaysVisible(data.isAlwaysOn);
        barScript.SetObjectToFollow(data.objectToFollow);
    }
    #endregion
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
    public Vector2 startingVelocity;
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
public class SummonedProjectileData
{
    public EntityCreator.Projectiles projectileType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    private Team team;
    public GameObject createdBy;
    public Vector2 startingVelocity;
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
public class SummonedProgressionBarData
{
    public EntityCreator.ProgressionBars progressionBarType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;

    public bool isAlwaysOn;
    public float hideDelay;
    public GameObject objectToFollow;
    public Vector2 barDeltaPosition;


}
#endregion


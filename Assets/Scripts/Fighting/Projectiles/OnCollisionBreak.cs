using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionBreak : SpriteUpdater
{
    [Header("Collision Settings")]
    public List<BreaksOn> breakEnum = new List<BreaksOn>();

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;

    protected GameObject objectThatCreatedThisProjectile;
    protected bool isDestroyed = false;
    protected float creationTime;
    const float INVULNERABILITY_TIME = 0.25f;


    public enum BreaksOn
    {
        Allies,
        Enemies,
        AllyBullets,
        EnemyBullets,
        Explosions,
        Rockets,
        Obstacles
    }

    protected virtual void Start()
    {
        UpdateStartingVariables();
    }
    private void UpdateStartingVariables()
    {
        creationTime = Time.time;
    }

    #region Collisions
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void HandleCollision(GameObject collisionObject)
    {
        if (CheckIfShouldBreak(collisionObject))
        {
            Break();
        }
    }
    private bool CheckIfShouldBreak(GameObject collisionObject)
    {
        if (CheckObstacle(collisionObject))
        {
            return true;
        }

        if (CheckProjectile(collisionObject))
        {
            return true;
        }

        if (CheckActors(collisionObject))
        {
            return true;
        }
        return false;
    }

    #region Break Checks
    private bool CheckObstacle(GameObject collisionObject)
    {
        return HelperMethods.IsAnObstacle(collisionObject) && BreaksOnContactWith(BreaksOn.Obstacles);
    }

    #region Check Actors
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns>True, if should break on actor</returns>
    private bool CheckActors(GameObject collisionObject)
    {
        if (IsInvulnerableTo(collisionObject))
        {
            return false;
        }

        bool areTeamsEqual = team == HelperMethods.GetObjectTeam(collisionObject);
        if (areTeamsEqual)
        {
            return BreaksOnAlly(collisionObject);
        }
        else
        {
            return BreaksOnEnemy(collisionObject);
        }
    }
    /// <summary>
    /// Every unit is invulnerable to its own projectiles for 0.1 sec
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns>Whether the collisionObject is invulnerable to this game object</returns>
    protected bool IsInvulnerableTo(GameObject collisionObject)
    {
        bool isTouchingParent = createdBy == collisionObject;
        bool isStillInvulnerable = Time.time < creationTime + INVULNERABILITY_TIME; //The shooting object should be immune to its own projectiles for a split second
        if (isTouchingParent && isStillInvulnerable)
        {
            return true;
        }
        return false;
    }
    private bool BreaksOnAlly(GameObject collisionObject)
    {
        bool breaksOnAlly = BreaksOnContactWith(BreaksOn.Allies) && HelperMethods.IsObjectAnEntity(collisionObject);
        if (breaksOnAlly)
        {
            return true;
        }
        return false;
    }
    private bool BreaksOnEnemy(GameObject collisionObject)
    {
        bool breaksOnEnemy = BreaksOnContactWith(BreaksOn.Enemies) && HelperMethods.IsObjectAnEntity(collisionObject);
        if (breaksOnEnemy)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Check projectiles
    private bool CheckProjectile(GameObject collisionObject)
    {
        IDamageDealer damageReceiver = collisionObject.GetComponent<IDamageDealer>();
        if (damageReceiver != null && BreaksOnProjectile(damageReceiver))
        {
            return true;
        }
        return false;
    }
    private bool BreaksOnProjectile(IDamageDealer iDamage)
    {
        if (BreaksOnAllyBullet(iDamage))
        {
            return true;
        }
        if (BreaksOnEnemyBullet(iDamage))
        {
            return true;
        }
        if (BreaksOnExplosion(iDamage))
        {
            return true;
        }
        if (BreaksOnRocket(iDamage))
        {
            return true;
        }
        return false;
    }
    private bool BreaksOnAllyBullet(IDamageDealer iDamage)
    {
        int collisionTeam = iDamage.GetTeam();
        bool areTeamsEqual = collisionTeam == team;
        return BreaksOnContactWith(BreaksOn.AllyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Bullet) && areTeamsEqual;
    }
    private bool BreaksOnEnemyBullet(IDamageDealer iDamage)
    {
        int collisionTeam = iDamage.GetTeam();
        bool areTeamsEqual = collisionTeam == team;
        return BreaksOnContactWith(BreaksOn.EnemyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Bullet) && !areTeamsEqual;
    }
    private bool BreaksOnExplosion(IDamageDealer iDamage)
    {
        return BreaksOnContactWith(BreaksOn.Explosions) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Explosion);
    }
    private bool BreaksOnRocket(IDamageDealer iDamage)
    {
        return BreaksOnContactWith(BreaksOn.Rockets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Rocket);
    }
    #endregion

    #endregion
    #endregion

    #region Destroy
    protected void Break()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            StaticDataHolder.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);

            DestroyObject();
        }
    }
    private void DestroyObject()
    {
        if (!HelperMethods.CallAllTriggers(gameObject))
        {
            StartCoroutine(DestroyAtTheEndOfFrame());
        }
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion

    //Sounds
    private AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }
    //Accessor methods
    private bool BreaksOnContactWith(BreaksOn contact)
    {
        if (breakEnum.Contains(contact))
        {
            return true;
        }
        return false;
    }
}

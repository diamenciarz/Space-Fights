using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class BreakOnCollision : TeamUpdater
{
    [Header("Collision Settings")]
    public List<BreaksOn> breakEnum = new List<BreaksOn>();

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;

    protected const float INVULNERABILITY_TIME = 0.5f;
    protected GameObject objectThatCreatedThisProjectile;
    protected bool isDestroyed = false;

    /// <summary>
    /// This entity should break if it collides with one of these types of objects
    /// </summary>
    public enum BreaksOn
    {
        AllyParts, // Parts are objects that contain the HelthManager or DamageReceiver components
        EnemyParts,
        AllyProjectiles,
        EnemyProjectiles,
        AllyExplosions,
        EnemyExplosions,
        Obstacles
    }

    #region Collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void HandleCollision(GameObject collisionObject)
    {
        if (ShouldBreak(collisionObject))
        {
            Break();
        }
    }
    private bool ShouldBreak(GameObject collisionObject)
    {
        List<BreaksOn> collisionPropertyList = HelperMethods.CollisionUtils.GetCollisionProperties(collisionObject, team);

        if (IsInvulnerableTo(collisionObject))
        {
            collisionPropertyList.Remove(BreaksOn.AllyParts);
        }
        return DoPropertiesOverlap(collisionPropertyList);
    }
    private bool DoPropertiesOverlap(List<BreaksOn> collisionPropertyList)
    {
        foreach (BreaksOn property in collisionPropertyList)
        {
            if (BreaksOnContactWith(property))
            {
                return true;
            }
        }
        return false;
    }
    private bool BreaksOnContactWith(BreaksOn contact)
    {
        if (breakEnum.Contains(contact))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Every unit is invulnerable to its own projectiles for 0.1 sec
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns>Whether the collisionObject is invulnerable to this game object</returns>
    public bool IsInvulnerableTo(GameObject collisionObject)
    {
        bool isTouchingParent = createdBy == collisionObject;
        bool isStillInvulnerable = Time.time < creationTime + INVULNERABILITY_TIME; //The shooting object should be immune to its own projectiles for a split second
        if (isTouchingParent && isStillInvulnerable)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Destroy
    protected void Break()
    {
        if (!isDestroyed)
        {
            StaticDataHolder.Sounds.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);
            DestroyObject();
        }
    }
    private AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }
    public void DestroyObject()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            HelperMethods.CollisionUtils.DoDestroyActions(gameObject, TriggerOnDeath.DestroyCause.InstantBreak);
            DestroyAtTheEndOfFrame();
        }
    }
    private async void DestroyAtTheEndOfFrame()
    {
        await Task.Yield();
        Destroy(gameObject);
    }
    /* OLD
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    */

    #endregion
}

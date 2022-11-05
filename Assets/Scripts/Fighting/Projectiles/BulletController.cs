using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController, IPiercingDamage
{
    [Header("Piercing")]
    [SerializeField] bool isPiercing;

    [Header("Wall bouncing")]
    [Tooltip("Min angle from the collision's normal to reflect")]
    [SerializeField] float minAngleToReflect = 45;
    [Tooltip("-1 for infinite bounces")]
    [SerializeField] int maxReflectionNumber = 3;
    [Tooltip("How much time to add to the bullet's lifetime after a reflection")]
    //[SerializeField] float timeAddedUponReflection = 2f; // Could create infinite lifetime - avoid

    private int reflections;

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        reflections = 0;
    }
    #endregion

    #region Collisions
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        BounceCheck(collision);
        StartCoroutine(WaitAndUpdateRotation());
    }
    private void BounceCheck(Collision2D collision)
    {
        if (IsInvulnerableTo(collision.gameObject))
        {
            return;
        }
        if (ShouldReflect(collision))
        {
            HandleReflection();
        }
    }
    private bool ShouldReflect(Collision2D collision)
    {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        float hitAngle = Vector3.Angle(myRigidbody2D.velocity, collisionNormal);
        //The collision angle has to be bigger than "minAngleToReflect" for the bullet to not get destroyed
        bool isAngleBigEnough = Mathf.Abs(hitAngle) >= minAngleToReflect;
        //Some bullets have a limited number of bounces
        bool hasBouncesLeft = maxReflectionNumber == -1 || reflections < maxReflectionNumber;

        return hasBouncesLeft && isAngleBigEnough;
    }
    private void HandleReflection()
    {
        reflections++;
    }
    private IEnumerator WaitAndUpdateRotation()
    {
        yield return new WaitForEndOfFrame();
        UpdateRotationToFaceForward();
    }

    #endregion

    #region Mutator methods
    /// <summary>
    /// Lowers the physical damage by given value. All other types of damage reamin constant.
    /// However, the bullet is destroyed, when physical damage drops to zero.
    /// </summary>
    public void LowerDamageBy(int change)
    {
        foreach (var category in damageCategories)
        {
            if (category.damageType == TypeOfDamage.Physical)
            {
                category.damage -= change;
                if (category.damage <= 0)
                {
                    DestroyObject();
                }
            }
        }
    }
    #endregion

    #region Accessor methods
    public override DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = base.GetDamageInstance();
        damageInstance.isPiercing = isPiercing;
        damageInstance.iPiercingDamage = this;
        return damageInstance;
    }
    #endregion
}



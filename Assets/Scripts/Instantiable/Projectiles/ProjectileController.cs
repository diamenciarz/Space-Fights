﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileController : OnCollisionDamage, IParent, IModifiableStartingSpeed
{
    [Header("Projectile Properties")]
    [SerializeField] protected float startingSpeed = 2f;
    [Tooltip("If true, the projectiles velocity is modified by the velocity of the object that created it. Only the projection in the projectile's direction is added.")]
    [SerializeField] bool addCreatorVelocity;
    //Private variables
    protected Rigidbody2D myRigidbody2D;
    protected SpriteRenderer spriteRenderer;
    protected BreakOnCollision breakOnCollision;
    protected Vector2 velocityVector;


    #region Startup
    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        breakOnCollision = GetComponent<BreakOnCollision>();
    }
    protected override void Start()
    {
        base.Start();
        SetStartingVelocityVector();
    }
    private void SetStartingVelocityVector()
    {
        float dir = transform.rotation.eulerAngles.z;
        Vector3 newVelocity = HelperMethods.VectorUtils.DirectionVector(startingSpeed, dir);
        SetVelocityVector(newVelocity);
    }
    #endregion

    #region Movement
    protected virtual void SetVelocityVector(Vector3 newVelocityVector)
    {
        velocityVector = newVelocityVector;
        myRigidbody2D.velocity = newVelocityVector;
        UpdateRotationToFaceForward();
    }
    protected void UpdateRotationToFaceForward()
    {
        Vector3 velocity = myRigidbody2D.velocity;
        transform.rotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, transform.position + velocity);
    }
    #endregion

    #region Mutator methods
    public void IncreaseStartingSpeed(Vector2 deltaVelocity)
    {
        Vector2 summonDirection = HelperMethods.VectorUtils.DirectionVectorNormalized(transform.rotation.eulerAngles.z);
        if (Vector2.Dot(deltaVelocity, summonDirection) < 0)
        {
            return;
            //deltaVelocity *= -1;
        }
        Vector2 velocityInObjectDirection = HelperMethods.VectorUtils.ProjectVector(deltaVelocity, summonDirection);
        float deltaSpeed = velocityInObjectDirection.magnitude;
        startingSpeed += deltaSpeed;
    }

    #region Team
    /// <summary>
    /// Change team of this object and all its children. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public override void SetTeam(Team newTeam)
    {
        base.SetTeam(newTeam);
        UpdateTeam();
    }
    private void UpdateTeam()
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.ParentUpdatesTeam(this);
        }
    }
    #endregion
    #endregion

    #region Accessor methods
    public float GetStartingSpeed()
    {
        return startingSpeed;
    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public bool ShouldModifyVelocity()
    {
        return addCreatorVelocity;
    }
    public Vector2 GetVelocityVector2()
    {
        return velocityVector;
    }
    public Vector3 GetVelocityVector3()
    {
        return myRigidbody2D.velocity;
    }
    public virtual ObjectType GetObjectType()
    {
        return ObjectType.PROJECTILE;
    }
    public override DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = base.GetDamageInstance();
        damageInstance.isAProjectile = true;
        return damageInstance;
    }
    #endregion

}

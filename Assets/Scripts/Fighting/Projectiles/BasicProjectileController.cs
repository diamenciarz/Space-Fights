using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BasicProjectileController : OnCollisionDamage, IParent, IModifiableStartingSpeed
{
    [Header("Projectile Properties")]
    [SerializeField] protected float startingSpeed = 2f;
    [Tooltip("If true, the projectiles velocity is modified by the velocity of the object that created it. Only the projection in the projectile's direction is added.")]
    [SerializeField] bool addCreatorVelocity;
    //Private variables
    protected Rigidbody2D myRigidbody2D;
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

        creationTime = Time.time;
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
    public void IncreaseStartingSpeed(float deltaSpeed)
    {
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
            item.UpdateTeam(this);
        }
    }
    #endregion
    #endregion

    #region Accessor methods
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
    public override DamageInstance GetDamageInstance()
    {
        DamageInstance damageInstance = base.GetDamageInstance();
        damageInstance.isAProjectile = true;
        return damageInstance;
    }
    #endregion

}

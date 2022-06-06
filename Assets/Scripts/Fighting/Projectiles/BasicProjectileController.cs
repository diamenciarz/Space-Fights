using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BasicProjectileController : OnCollisionDamage, IParent
{
    [Header("Projectile Properties")]
    [SerializeField] protected float startingSpeed = 2f;

    //Private variables
    protected Rigidbody2D myRigidbody2D;

    protected Vector2 velocityVector;


    #region Startup
    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    protected override void Start()
    {
        base.Start();
        float dir = transform.rotation.eulerAngles.z;
        Vector3 newVelocity = HelperMethods.VectorUtils.DirectionVector(startingSpeed, dir);
        SetVelocityVector(newVelocity);
    }
    private void SetupStartingValues()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();

        creationTime = Time.time;
    }
    #endregion

    #region Mutator methods
    public virtual void SetVelocityVector(Vector3 newVelocityVector)
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

    #region Accessor methods
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
}

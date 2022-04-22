using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BasicProjectileController : OnCollisionDamage, IParent
{
    [Header("Projectile Properties")]
    [SerializeField] protected List<Sprite> spriteList;
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
        Vector3 newVelocity = HelperMethods.DirectionVector(startingSpeed, dir);
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
        transform.rotation = HelperMethods.DeltaPositionRotation(transform.position, transform.position + velocity);
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
    public GameObject GetCreatedBy()
    {
        return gameObject;
    }
    #endregion
}

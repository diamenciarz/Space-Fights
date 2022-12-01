using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceProperty : TeamUpdater
{
    [Header("Wall bouncing")]
    [Tooltip("Min angle from the collision's normal to reflect")]
    [SerializeField] float minAngleToReflect = 45;
    [Tooltip("-1 for infinite bounces")]
    [SerializeField] int maxReflectionNumber = 3;

    private int bounces = 0;
    protected Rigidbody2D myRigidbody2D;
    private BreakOnCollision myBreakOnCollision;

    protected override void Start()
    {
        base.Start();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myBreakOnCollision = GetComponent<BreakOnCollision>();

        creationTime = Time.time;
    }

    #region Collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        BounceCheck(collision);
        StartCoroutine(WaitAndUpdateRotation());
    }
    private IEnumerator WaitAndUpdateRotation()
    {
        yield return new WaitForEndOfFrame();
        UpdateRotationToFaceForward();
    }
    private void UpdateRotationToFaceForward()
    {
        Vector3 velocity = myRigidbody2D.velocity;
        transform.rotation = HelperMethods.RotationUtils.DeltaPositionRotation(transform.position, transform.position + velocity);
    }
    private bool ShouldBounce(Collision2D collision)
    {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        float hitAngle = Vector3.Angle(myRigidbody2D.velocity, collisionNormal);
        //The collision angle has to be bigger than "minAngleToReflect" for the bullet to not get destroyed
        bool isAngleBigEnough = Mathf.Abs(hitAngle) >= minAngleToReflect;
        //Some bullets have a limited number of bounces
        bool hasBouncesLeft = maxReflectionNumber == -1 || bounces < maxReflectionNumber;

        return hasBouncesLeft && isAngleBigEnough;
    }
    private void HandleBounce()
    {
        bounces++;
    }
    private void BounceCheck(Collision2D collision)
    {
        if (IsInvulnerableTo(collision.gameObject))
        {
            return;
        }
        if (ShouldBounce(collision))
        {
            HandleBounce();
        }
    }
    private bool IsInvulnerableTo(GameObject gameObject)
    {
        if (myBreakOnCollision == null)
        {
            return false;
        }
        return myBreakOnCollision.IsInvulnerableTo(gameObject);
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{
    [Header("Timed destroy")]
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinity")]
    [SerializeField] float destroyDelay = 5f;
    [Tooltip("Destroy the bullet after it has travelled this much distance. -1 for infinity")]
    [SerializeField] float destroyDistance = 1;

    [Header("Wall bouncing")]
    [Tooltip("Min angle from the collision's normal to reflect")]
    [SerializeField] float minAngleToReflect = 45;
    [Tooltip("-1 for infinite bounces")]
    [SerializeField] int maxReflectionNumber = 3;
    [Tooltip("How much time to add to the bullet's lifetime after a reflection")]
    [SerializeField] float timeAddedUponReflection = 2f;

    private float destroyTime;
    private int reflections;
    private bool timedDestroy = false;
    private float MAX_DESTROY_DELAY = 100f;

    protected override void Start()
    {
        base.Start();
        SetupStartingVariables();
        if (timedDestroy)
        {
            StartCoroutine(CheckDestroyDelay());
        }
    }
    private void SetupStartingVariables()
    {
        reflections = 0;
        SetupDestroyTime();
    }

    #region Destroy
    private void SetupDestroyTime()
    {
        destroyTime = Time.time + CountDestroyDelay();
    }
    private float CountDestroyDelay()
    {
        float returnTime = MAX_DESTROY_DELAY;
        //Sets the destroy delay to the lowest number
        float distanceDelay = TranslateDistanceDelay();
        if (distanceDelay < returnTime)
        {
            returnTime = distanceDelay;
            timedDestroy = true;
        }

        float timeDelay = TranslateTimeDelay();
        if (timeDelay < returnTime)
        {
            returnTime = timeDelay;
            timedDestroy = true;
        }
        return returnTime;
    }
    private float TranslateDistanceDelay()
    {
        if (destroyDistance == -1)
        {
            return MAX_DESTROY_DELAY;
        }
        else
        {
            return destroyDistance / myRigidbody2D.velocity.magnitude;
        }
    }
    private float TranslateTimeDelay()
    {
        if (destroyDelay == -1)
        {
            return MAX_DESTROY_DELAY;
        }
        else
        {
            return destroyDelay;
        }
    }
    private IEnumerator CheckDestroyDelay()
    {
        yield return new WaitUntil(() => destroyTime < Time.time);
        DestroyObject();
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
        if (!IsInvulnerableTo(collision.gameObject))
        {
            if (ShouldReflect(collision))
            {
                HandleReflection();
            }
            else
            {
                DestroyObject();
            }
        }
    }
    private bool ShouldReflect(Collision2D collision)
    {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        float hitAngle = Vector3.Angle(GetVelocityVector3(), collisionNormal);
        //The collision angle has to be bigger than "minAngleToReflect" for the bullet to not get destroyed
        bool isAngleBigEnough = Mathf.Abs(hitAngle) >= minAngleToReflect;
        //Some bullets have a limited number of bounces
        bool hasBouncesLeft = maxReflectionNumber == -1 || reflections < maxReflectionNumber;

        return hasBouncesLeft && isAngleBigEnough;
    }
    private void HandleReflection()
    {
        reflections++;
        destroyTime += timeAddedUponReflection;
    }
    private IEnumerator WaitAndUpdateRotation()
    {
        yield return new WaitForEndOfFrame();
        UpdateRotationToFaceForward();
    }
    #endregion
}



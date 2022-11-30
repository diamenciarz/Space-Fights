using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenteredPushingForce : AbstractActionOnCollision
{
    [Tooltip("How many times the starting radius should the final range be")]
    [SerializeField] float expandRate; // Sprite scale
    public float timeToExpire;
    [Tooltip("How strong the pushing force of this explosion should be at its strongest point, which is the middle. The force decreases with as distance from the middle gets lower")]
    [SerializeField][Range(0, 1000)] float maxPushingForce;
    [Header("Info")]
    float startingRadius;
    //[SerializeField][Range(0.01f, 100)] float startingRadius;

    protected override void Awake()
    {
        base.Awake();
        startingRadius = transform.localScale.x;
    }

    #region OnCollision
    protected override void HandleExit(GameObject collisionObject)
    {
        //Leave empty
    }
    protected override void HandleCollision(Collision2D collision)
    {
        HandlePush(collision.gameObject, collision.contacts);
    }
    protected override void HandleTriggerEnter(Collider2D trigger)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        trigger.GetContacts(contacts);

        HandlePush(trigger.gameObject, contacts);
    }
    private void HandlePush(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        if (CanPush(collisionObject))
        {
            PushObject(collisionObject, contacts);
        }
    }
    private bool CanPush(GameObject collisionObject)
    {
        IPushable iPushable = collisionObject.GetComponent<IPushable>();
        bool objectCanBePushed = iPushable != null;
        return objectCanBePushed;
    }
    private void PushObject(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        IPushable iPushable = collisionObject.GetComponent<IPushable>();
        Vector2 pushingForce = CountForce(collisionObject, contacts);

        iPushable.Push(pushingForce);
    }
    #region Helper methods
    private Vector2 CountForce(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        Vector2 collisionPosition = FindTheClosestCollisionPoint(contacts);
        Vector2 explosionPosition = transform.position;
        Vector2 obstaclePosition = collisionObject.transform.position;
        Vector2 deltaPosition = HelperMethods.VectorUtils.DeltaPosition(explosionPosition, obstaclePosition);
        float distance = deltaPosition.magnitude;
        float maxRadius = startingRadius * expandRate;

        float maxForcePercentage = Mathf.Clamp((maxRadius - distance), 0, maxRadius) / maxRadius;
        //Debug.Log("Max radius: " + maxRadius + " percentage: " + maxForcePercentage);
        float forceStrength = maxPushingForce * maxForcePercentage;
        Vector2 force = deltaPosition.normalized * forceStrength;

        return force;
    }
    /// <summary>
    /// This was discontinued due to funky behaviour. Center of mass is better
    /// </summary>
    /// <param name="contacts"></param>
    /// <returns></returns>
    private Vector2 FindTheClosestCollisionPoint(ContactPoint2D[] contacts)
    {
        //TODO Compare contact pushing to center of mass pushing
        ContactPoint2D closestPoint = contacts[0];
        Vector2 explosionPosition = transform.position;
        foreach (ContactPoint2D contactPoint in contacts)
        {
            bool foundCloserPoint = HelperMethods.VectorUtils.Distance(explosionPosition, contactPoint.point) < HelperMethods.VectorUtils.Distance(explosionPosition, closestPoint.point);
            if (foundCloserPoint)
            {
                closestPoint = contactPoint;
            }
        }
        return closestPoint.point;
    }
    #endregion
    #endregion
}

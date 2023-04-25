using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPushingForce : AbstractActionOnCollision
{
    [SerializeField][Range(0, 1000)] float pushingForce;

    private Collider2D myCollider2D;

    protected override void Start()
    {
        base.Start();
        myCollider2D = GetComponent<Collider2D>();
    }
    protected override void HandleCollision(Collision2D collision)
    {
        HandlePush(collision.gameObject, collision.contacts);
    }
    protected override void HandleExit(GameObject obj)
    {
        // Nothing
    }
    protected override void HandleTriggerEnter(Collider2D trigger)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        Collider2D[] colliders = new Collider2D[1];
        trigger.GetContacts(contacts);

        // Figure out how to see, where two colliders are touching
        HandlePush(trigger.gameObject, contacts);
    }

    #region OnCollision
    private void HandlePush(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        if (CanPush(collisionObject))
        {
            PushObject(collisionObject, contacts);
        }
    }
    private bool CanPush(GameObject collisionObject)
    {
        return collisionObject.GetComponent<IPushable>() != null;
    }
    private void PushObject(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        IPushable iPushable = collisionObject.GetComponent<IPushable>();
        Vector2 force = CalculateForce(collisionObject);
        //Vector2 pushPosition = FindTheClosestCollisionPoint(contacts);

        iPushable.Push(force, transform.position, true);
    }
    private Vector2 FindTheClosestCollisionPoint(ContactPoint2D[] contacts)
    {
        //TODO Compare contact pushing to center of mass pushing
        ContactPoint2D closestPoint = contacts[0];
        Vector2 pushOriginPosition = transform.position;
        foreach (ContactPoint2D contactPoint in contacts)
        {
            if (contactPoint.otherCollider == null)
            {
                continue;
            }
            Debug.DrawLine(transform.position, contactPoint.point, Color.cyan, 0.3f);
            bool foundCloserPoint = HelperMethods.VectorUtils.Distance(pushOriginPosition, contactPoint.point) < HelperMethods.VectorUtils.Distance(pushOriginPosition, closestPoint.point);
            if (foundCloserPoint)
            {
                closestPoint = contactPoint;
            }
        }
        return closestPoint.point;
    }
    private Vector2 CalculateForce(GameObject collisionObject)
    {
        Vector2 positionToCollider = HelperMethods.VectorUtils.DeltaPosition(transform.position, collisionObject.transform.position);
        return pushingForce * positionToCollider.normalized;
    }
    #endregion
}

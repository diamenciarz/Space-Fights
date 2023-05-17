using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;
using static HelperMethods;
using System.Linq;

public class ExplosionTest : MonoBehaviour
{
    int maxRadius = 10;
    [SerializeField][Range(0, 1000)] float maxPushingForce = 30f;
    void Start()
    {
        Collider2D[] collidersHit = LineOfSightUtils.GetOverlappingCollidersWithCircle(transform.position, maxRadius, GetObjectTypes()).ToArray();
        PushColliders(collidersHit);
    }
    private ObjectTypes[] GetObjectTypes()
    {
        ObjectTypes[] types = new ObjectTypes[2];
        types[0] = ObjectTypes.Entity;
        types[1] = ObjectTypes.Obstacle;
        return types;
    }
    private void PushColliders(Collider2D[] visibleColliders)
    {
        foreach (Collider2D collider in visibleColliders)
        {
            Rigidbody2D rb2D = collider.attachedRigidbody;
            if (rb2D != null)
            {
                Vector2 deltaPositionToCollider = collider.transform.position - transform.position;
                float maxForcePercentage = Mathf.Clamp((maxRadius - deltaPositionToCollider.magnitude) / maxRadius, 0, maxRadius);
                Vector2 force = maxForcePercentage * maxPushingForce * deltaPositionToCollider.normalized;
                rb2D.AddForceAtPosition(force, collider.transform.position, ForceMode2D.Impulse);
            }
        }
    }
}

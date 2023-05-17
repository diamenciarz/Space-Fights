using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperMethods;
using static StaticDataHolder;
using System.Linq;

[RequireComponent(typeof(CircleCollider2D))]
public class CenteredPushingForce : MonoBehaviour
{

    [Tooltip("How strong the pushing force of this explosion should be at its strongest point, which is the middle. The force decreases with as distance from the middle gets lower")]
    [SerializeField][Range(0, 1000)] float maxPushingForce = 30f;

    private float maxRadius;
    private float startingRadius;

    void Start()
    {
        SetupStartingVariables();

        Collider2D[] collidersHit = LineOfSightUtils.GetOverlappingCollidersWithCircle(transform.position, maxRadius, GetObjectTypes()).ToArray();
        Debug.Log("Hit " + collidersHit.Length + " targets");
        PushColliders(collidersHit);
    }
    private void SetupStartingVariables()
    {
        startingRadius = GetComponent<CircleCollider2D>().radius;
        float expandRate = GetComponent<ExplosionController>().GetExpandRate();
        maxRadius = startingRadius * expandRate;
        Debug.Log("Max radius " + maxRadius);
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
                Vector2 collisionPoint = LineOfSightUtils.GetRaycastHitPositionIgnoreEverything(transform.position, collider.gameObject);
                Vector2 deltaPositionToCollider = collisionPoint - (Vector2)transform.position;
                float maxForcePercentage = Mathf.Clamp((maxRadius - deltaPositionToCollider.magnitude) / maxRadius, 0, maxRadius);
                Vector2 force = maxForcePercentage * maxPushingForce * deltaPositionToCollider.normalized;
                rb2D.AddForceAtPosition(force, collider.transform.position, ForceMode2D.Impulse);
            }
        }
    }
}

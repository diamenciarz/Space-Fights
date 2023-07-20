using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;
using static TeamUpdater;
using static AIShipController;

[CreateAssetMenu(fileName = "Chase target", menuName = "AI/MovementBehaviours/ChaseTargetRelation")]
public class BehaviourInRelationToTarget : MovementBehaviour
{

    [SerializeField][Range(2, 200)][Tooltip("Will stop chasing its target above that distance")] protected float chaseRange = 45;
    [SerializeField][Range(1, 60)][Tooltip("Will start avoiding obstacles below that distance")] protected float avoidRange = 8;
    [SerializeField][Range(1, 60)][Tooltip("Will not come closer to target than this distance")] protected float attackRange = 6;

    [SerializeField][Range(0, 1)][Tooltip("0 - projectiles ignored when chasing; 1 - projectiles avoided at close range even when chasing")] protected float projectileAvoidance = 0.5f;
    [SerializeField][Range(0, 1)][Tooltip("0 - obstacles ignored when chasing; 1 - obstacles avoided at close range even when chasing")] protected float entityAvoidance = 0.5f;

    [SerializeField] BattleMode battleMode;
    [SerializeField] MovementMode movementMode;
    [SerializeField] MovementStyle movementStyle;

    enum MovementStyle
    {
        DIRECTLY_AT_TARGET,
        PREDICT_TARGET,
        PREDICT_EVERYTHINGGGG
    }

    private GameObject targetToChase;
    private MovementBehaviourData data;

    public override Vector2 CalculateMovementVector(MovementBehaviourData newData)
    {
        data = newData;
        targetToChase = ListContents.Enemies.GetClosestEnemy(data.position, data.team);
        //Vector2 chaseVector = Vector2.zero;
        Vector2 chaseVector = CalculateChaseVector();

        Vector2 obstacleAvoidanceVector = CalculateObstacleAvoidanceVector();
        Vector2 projectileAvoidanceVector = CalculateProjectileAvoidanceVector();

        float chaseLength = Mathf.Min(chaseVector.magnitude, 1);
        float obstacleAvoidanceLength = Mathf.Min(obstacleAvoidanceVector.magnitude, 1);
        float projectileAvoidanceLength = Mathf.Min(projectileAvoidanceVector.magnitude, 1);

        Vector2 projectilePart = (1 - obstacleAvoidanceLength) * projectileAvoidance * projectileAvoidanceLength * projectileAvoidanceVector.normalized;
        Vector2 obstaclePart = obstacleAvoidanceLength * obstacleAvoidanceVector.normalized;
        Vector2 avoidanceVector = obstaclePart + projectilePart;

        return chaseLength * chaseVector.normalized + (1 + entityAvoidance - chaseLength) * avoidanceVector.normalized;
    }

    #region Chasing
    private Vector2 CalculateChaseVector()
    {
        if (targetToChase == null)
        {
            return Vector2.zero;
        }
        Vector2 deltaPositionToItem = GetDeltaPositionToItem(targetToChase);
        if (!IsInChaseRange(deltaPositionToItem))
        {
            return Vector2.zero;
        }

        return HandleChaseObject(deltaPositionToItem);
    }
    private bool IsInChaseRange(Vector2 deltaPositionToItem)
    {
        return deltaPositionToItem.magnitude < chaseRange;
    }
    private Vector2 HandleChaseObject(Vector2 deltaPositionToItem)
    {
        // Prioritize chasing more, if farther away from the target!
        float multiplier = CalculateMultiplier(deltaPositionToItem);

        Vector2 myPosition = CalculateMyPosition();
        Vector2 targetPosition = CalculateTargetPosition(myPosition);

        Vector2 predictedDeltaPosition = targetPosition - myPosition;
        Debug.DrawRay(myPosition, predictedDeltaPosition.normalized * multiplier, Color.blue, 0.05f);
        return (predictedDeltaPosition).normalized * multiplier;
    }
    const float SIGNIFICANT_SPEED = 2;
    protected float CalculateMultiplier(Vector2 deltaPositionToItem)
    {
        if (movementMode == MovementMode.AVOIDING)
        {
            return -1;
        }
        else
        {
            if (battleMode == BattleMode.MELEE)
            {
                return 1;
            }
            else
            {
                return deltaPositionToItem.magnitude - attackRange;
            }
        }
    }
    private Vector2 CalculateMyPosition()
    {
        float distance = HelperMethods.VectorUtils.Distance(data.position, (Vector2)targetToChase.transform.position);
        bool velocityAndDistanceAreSignificant = distance > attackRange && data.velocity.magnitude > SIGNIFICANT_SPEED;
        if (ShouldPredictMyPosition() && velocityAndDistanceAreSignificant)
        {
            float timeToTarget = distance / data.velocity.magnitude;
            return data.position + data.velocity * timeToTarget;
        }
        else
        {
            return data.position;
        }
    }
    private Vector2 CalculateTargetPosition(Vector2 myPosition)
    {
        bool velocityIsSignificant = data.velocity.magnitude > SIGNIFICANT_SPEED;
        if (ShouldPredictTarget() && velocityIsSignificant)
        {
            return HelperMethods.ObjectUtils.PredictTargetPositionUponHit(myPosition, targetToChase, data.velocity.magnitude);
        }
        else
        {
            return targetToChase.transform.position;
        }
    }
    private bool ShouldPredictMyPosition()
    {
        return movementStyle == MovementStyle.PREDICT_EVERYTHINGGGG;
    }
    private bool ShouldPredictTarget()
    {
        return movementStyle == MovementStyle.PREDICT_TARGET || movementStyle == MovementStyle.PREDICT_EVERYTHINGGGG;
    }
    #endregion

    #region Obstacle Avoidance
    private Vector2 CalculateObstacleAvoidanceVector()
    {
        List<GameObject> avoidObjects = GetObjectsToAvoid();
        return CalculateAvoidVector(avoidObjects);
    }
    private Vector2 CalculateAvoidVector(List<GameObject> avoidObjects)
    {
        Vector2 proximityVector = Vector2.zero;
        foreach (var item in avoidObjects)
        {
            Vector2 deltaPositionToItem = GetDeltaPositionToItem(item);
            bool shouldIgnore = deltaPositionToItem.magnitude > chaseRange;
            if (shouldIgnore)
            {
                continue;
            }

            proximityVector += HandleAvoidObject(item, deltaPositionToItem);
        }
        return proximityVector;
    }
    private Vector2 HandleAvoidObject(GameObject obj, Vector2 deltaPositionToItem)
    {
        bool isInAvoidRange = deltaPositionToItem.magnitude < avoidRange;
        if (!isInAvoidRange)
        {
            //Debug.DrawRay(transform.position, deltaPositionToItem, Color.red, 0.05f);
            return Vector2.zero;
        }
        else
        {
            // Multiply by more, if closer to target
            float multiplier = CalculateMassMultiplier(obj) / deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(data.position, -deltaPositionToItem * multiplier, Color.yellow, 0.05f);
            return -deltaPositionToItem.normalized * multiplier;
        }
    }
    private float CalculateMassMultiplier(GameObject obj)
    {
        Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            return rb2D.mass;
        }
        else
        {
            return 0f;
        }
    }
    private List<GameObject> GetObjectsToAvoid()
    {
        List<GameObject> avoidObjects = ListContents.Allies.GetAllyList(data.team, data.gameObject);
        RemoveMyParts(avoidObjects);
        avoidObjects.AddRange(ListContents.Generic.GetObjectList(ObjectTypes.Obstacle));
        avoidObjects.AddRange(ListContents.Generic.GetObjectList(ObjectTypes.Indestructible));
        return avoidObjects;
    }
    private void RemoveMyParts(List<GameObject> avoidObjects)
    {
        for (int i = avoidObjects.Count - 1; i >= 0; i--)
        {
            if (IsMyPart(avoidObjects[i]))
            {
                avoidObjects.RemoveAt(i);
            }
        }
    }
    private bool IsMyPart(GameObject part)
    {
        IParent partParent = part.GetComponentInParent<IParent>();

        if (partParent == null || data.parent == null)
        {
            return false;
        }
        return partParent.GetGameObject() == data.parent.GetGameObject();
    }
    private Vector2 GetDeltaPositionToItem(GameObject item)
    {
        return HelperMethods.LineOfSightUtils.EdgeDeltaPosition(data.gameObject, item);
    }
    #endregion

    #region Projectile Avoidance
    private Vector2 CalculateProjectileAvoidanceVector()
    {
        Vector2 avoidVector = Vector2.zero;
        foreach (GameObject projectile in GetProjectilesToAvoid())
        {
            if (projectile == null)
            {
                continue;
            }
            Rigidbody2D rb2D = projectile.GetComponent<Rigidbody2D>();
            if (rb2D == null)
            {
                continue;
            }

            avoidVector += CalculateSingleProjectileAvoidance(projectile, rb2D);
        }
        return avoidVector;
    }
    private List<GameObject> GetProjectilesToAvoid()
    {
        List<GameObject> allProjectiles = ListContents.Generic.GetObjectList(ObjectTypes.Projectile);
        return ListModification.SubtractNeutralsAndAllies(allProjectiles, data.team);
    }
    private Vector2 CalculateSingleProjectileAvoidance(GameObject projectile, Rigidbody2D rb2D)
    {
        Vector2 positionToEnemy = HelperMethods.VectorUtils.DeltaPosition(data.gameObject, projectile);
        if (!IsEnemyAhead(projectile, rb2D))
        {
            return Vector2.zero;
        }

        Vector2 projectedPosition = Vector3.Project(positionToEnemy, rb2D.velocity);
        // This is the delta vector towards the closest point that the projectile will pass by next to the ship
        Vector2 projectilePassPosition = positionToEnemy - projectedPosition;
        float projectilePassDistance = projectilePassPosition.magnitude;

        bool projectileWillMiss = projectilePassDistance > data.shipSize;
        if (projectileWillMiss)
        {
            return Vector2.zero;
        }

        float timeForProjectileToPass = projectedPosition.magnitude / rb2D.velocity.magnitude;
        const float MAX_REACT_TIME = 3f;

        float modifier = Mathf.Pow(Mathf.Max(0, MAX_REACT_TIME - timeForProjectileToPass), 2) / projectilePassDistance;
        //Debug.Log("Modifier " + modifier);
        //Debug.DrawLine(transform.position, ((Vector2)transform.position - (projectilePassPosition.normalized * modifier)), Color.red, 0.1f);
        //Debug.DrawLine(projectile.transform.position, (Vector2)projectile.transform.position-projectedPosition, Color.blue, 0.1f);
        return -projectilePassPosition.normalized * modifier;
    }
    private bool IsEnemyAhead(GameObject projectile, Rigidbody2D rb2D)
    {
        Vector2 positionToEnemy = HelperMethods.VectorUtils.DeltaPosition(data.gameObject, projectile);
        float dot = Vector2.Dot(rb2D.velocity, positionToEnemy);
        return dot < 0;
    }
    #endregion
}
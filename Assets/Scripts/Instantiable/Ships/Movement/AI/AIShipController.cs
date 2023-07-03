using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;
using static StaticDataHolder;

public class AIShipController : TeamUpdater, ISerializationCallbackReceiver, INotifyOnDestroy
{
    [SerializeField][Range(2, 200)][Tooltip("Will stop chasing its target above that distance")] float chaseRange = 45;
    [SerializeField][Range(1, 60)][Tooltip("Will start avoiding obstacles below that distance")] float avoidRange = 8;
    [SerializeField][Range(1, 60)][Tooltip("Will not come closer to target than this distance")] float attackRange = 6;
    [SerializeField] bool isForceGlobal = false;
    [SerializeField][Range(0, 1)][Tooltip("0 - obstacles ignored when chasing; 1 - obstacles avoided at close range even when chasing")] float entityAvoidance = 0.5f;
    [SerializeField][Range(0, 1)][Tooltip("0 - projectiles ignored when chasing; 1 - projectiles avoided at close range even when chasing")] float projectileAvoidance = 0.5f;
    [SerializeField][Range(0.1f, 5)][Tooltip("How often the ship will change direction of random movement")] float minMovementPeriod = 1;
    [SerializeField][Range(1, 10)][Tooltip("How often the ship will change direction of random movement")] float maxMovementPeriod = 3;
    [SerializeField][Range(0.1f, 60)][Tooltip("How often the ship will change tactic from offensive to defensive")] float minChaseDuration = 10;
    [SerializeField][Range(1, 60)][Tooltip("How often the ship will change tactic from offensive to defensive")] float maxChaseDuration = 20;
    [SerializeField][Range(0.1f, 60)][Tooltip("How often the ship will change tactic from defensive to offensive")] float minFleeDuration = 5;
    [SerializeField][Range(1, 60)][Tooltip("How often the ship will change tactic from defensive to offensive")] float maxFleeDuration = 10;
    [SerializeField][Range(10, 180)][Tooltip("How often and how much the ship will turn, while randomly exploring the map")] float randomMovementAngle = 30;
    [SerializeField] float shipSize = 1.5f;
    [SerializeField] GameObject followObject;
    [SerializeField] float followLeash = 5;

    IEntityMover myVehicle;
    private Rigidbody2D rb2D;
    private GameObject targetToChase;
    private FightTactics fightTactics;
    private IParent myParent;
    private int gunCount;
    private bool orderSent = false;
    private float timeSinceStartedRunningAway;

    //Random movement
    private Vector2 randomMovementVector;

    enum MovementMode
    {
        CHASING,
        AVOIDING,
        IDLE
    }
    enum BattleMode
    {
        RANGED,
        MELEE
    }

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetupStartingVariables();
        GenerateRandomMovementVariables();
        StartRandomMovementCoroutine();
        //SetNotMouseControlled();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        myVehicle = GetComponent<IEntityMover>();
        myParent = gameObject.GetComponentInParent<IParent>();
        CountGuns();
        CheckForMeleeMode();
        fightTactics = new FightTactics();
    }
    private void CountGuns()
    {
        GunController[] gunControllers = GetComponentsInChildren<GunController>();
        foreach (GunController controller in gunControllers)
        {
            controller.AddOnDestroyAction(this);
        }
        gunCount = gunControllers.Length;
    }
    private void GenerateRandomMovementVariables()
    {
        entityAvoidance = Random.Range(0.5f, 1f);
        projectileAvoidance = Random.Range(0.5f, 1f);
        chaseRange = Random.Range(30, 60);
        avoidRange = Random.Range(4, 8);
        attackRange = Random.Range(2, 6);
    }

    #region Serialization
    public void OnBeforeSerialize()
    {
        FixRange();
    }

    public void OnAfterDeserialize()
    {

    }
    private void FixRange()
    {
        if (chaseRange < avoidRange - 1)
        {
            chaseRange = avoidRange + 1;
        }
        if (chaseRange < attackRange - 1)
        {
            chaseRange = attackRange + 1;
        }
    }
    #endregion

    private void SetNotMouseControlled()
    {
        IPlayerControllable[] components = GetComponentsInChildren<IPlayerControllable>();
        foreach (IPlayerControllable component in components)
        {
            component.SetIsControlledByMouse(false);
        }
    }
    #endregion

    #region Update
    void Update()
    {
        //Order important
        UpdateTactics();
        Vector2 movementVector = CalculateMovementVector();
        movementVector = ApplyEdgeOfMapVector(movementVector);
        myVehicle.SetInputVector(TranslateMovementVector(movementVector));
    }

    #region Helper methods
    private Vector2 TranslateMovementVector(Vector2 globalForce)
    {
        if (isForceGlobal)
        {
            return globalForce;
        }
        else
        {
            return TranslateToLocalForce(globalForce);
        }
    }
    private Vector2 TranslateToLocalForce(Vector2 globalForce)
    {
        float shipGlobalAngle = rb2D.gameObject.transform.rotation.eulerAngles.z;
        Vector2 rotatedVector = HelperMethods.VectorUtils.RotateVector(globalForce, shipGlobalAngle);
        //Debug.Log("Rotate by angle: " + entityGlobalAngle);
        Debug.DrawRay(transform.position, rotatedVector, Color.green, 0.1f);

        return rotatedVector;
    }
    #endregion

    #region Movement vector
    private Vector2 CalculateMovementVector()
    {
        targetToChase = ListContents.Enemies.GetClosestEnemy(transform.position, team);
        //Vector2 chaseVector = Vector2.zero;
        Vector2 chaseVector = CalculateChaseVector();

        Vector2 obstacleAvoidanceVector = CalculateObstacleAvoidanceVector();
        Vector2 projectileAvoidanceVector = CalculateProjectileAvoidanceVector();

        //Vector2 randomMovement = Vector2.zero;
        Vector2 randomMovement = CalculateRandomMovementVector();

        float chaseLength = Mathf.Min(chaseVector.magnitude, 1);
        float obstacleAvoidanceLength = Mathf.Min(obstacleAvoidanceVector.magnitude, 1);
        float projectileAvoidanceLength = Mathf.Min(projectileAvoidanceVector.magnitude, 1);

        Vector2 projectilePart = (1 - obstacleAvoidanceLength) * projectileAvoidance * projectileAvoidanceLength * projectileAvoidanceVector.normalized;
        Vector2 obstaclePart = obstacleAvoidanceLength * obstacleAvoidanceVector.normalized;
        Vector2 avoidanceVector = obstaclePart + projectilePart;

        Vector2 movementVector = chaseVector;

        if (chaseLength < 1)
        {
            movementVector += (1 - chaseLength) * randomMovement;
            chaseLength = 1;
        }
        return chaseLength * movementVector.normalized + (1 + entityAvoidance - chaseLength) * avoidanceVector.normalized;
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
    private Vector2 GetDeltaPositionToItem(GameObject item)
    {
        return HelperMethods.LineOfSightUtils.EdgeDeltaPosition(gameObject, item);
    }
    private bool IsInChaseRange(Vector2 deltaPositionToItem)
    {
        return deltaPositionToItem.magnitude < chaseRange;
    }
    private Vector2 HandleChaseObject(Vector2 deltaPositionToItem)
    {
        bool isAboveRange = IsAboveAttackRange(deltaPositionToItem);
        if (isAboveRange)
        {
            // Prioritize chasing more, if farther away from the target!
            float multiplier = CalculateMultiplier(isAboveRange, deltaPositionToItem);
            Vector2 predictedTargetPosition = HelperMethods.ObjectUtils.PredictTargetPositionUponHit(transform.position, targetToChase, rb2D.velocity.magnitude);
            Vector2 predictedDeltaPosition = predictedTargetPosition - (Vector2)targetToChase.transform.position;
            Debug.DrawRay(transform.position, (deltaPositionToItem + predictedDeltaPosition).normalized * multiplier, Color.blue, 0.05f);
            return (deltaPositionToItem + predictedDeltaPosition).normalized * multiplier;
        }
        else
        {
            // Fall back, if too close to target. Keep distance!
            float multiplier = CalculateMultiplier(isAboveRange, deltaPositionToItem);
            Debug.DrawRay(transform.position, -deltaPositionToItem.normalized * multiplier, Color.red, 0.05f);
            return -deltaPositionToItem.normalized * multiplier;
        }
    }
    private float CalculateMultiplier(bool isAboveRange, Vector2 deltaPositionToItem)
    {
        if (fightTactics.battleMode == BattleMode.RANGED)
        {
            if (fightTactics.movementMode == MovementMode.CHASING)
            {
                if (isAboveRange)
                {
                    return deltaPositionToItem.magnitude - attackRange;
                }
                else
                {
                    return 1 / (attackRange - deltaPositionToItem.magnitude);
                }
            }
            else
            //if (fightTactics.movementMode == MovementMode.AVOIDING)
            {
                if (isAboveRange)
                {
                    return 1 / (deltaPositionToItem.magnitude - attackRange);
                }
                else
                {
                    return 1;
                }
            }

        }
        else
        {
            return 1;
        }
    }
    private bool IsAboveAttackRange(Vector2 deltaPositionToItem)
    {
        if (fightTactics.battleMode == BattleMode.RANGED)
        {
            if (fightTactics.movementMode == MovementMode.CHASING)
            {
                return deltaPositionToItem.magnitude > attackRange;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (fightTactics.movementMode == MovementMode.CHASING)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
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

            proximityVector += HandleAvoidObject(deltaPositionToItem);
        }
        return proximityVector;
    }
    private Vector2 HandleAvoidObject(Vector2 deltaPositionToItem)
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
            float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(transform.position, -deltaPositionToItem * multiplier, Color.yellow, 0.05f);
            return -deltaPositionToItem.normalized * multiplier;
        }
    }
    private List<GameObject> GetObjectsToAvoid()
    {
        List<GameObject> avoidObjects = ListContents.Allies.GetAllyList(team, gameObject);
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

        if (partParent == null || myParent == null)
        {
            return false;
        }
        return partParent.GetGameObject() == myParent.GetGameObject();
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
        return ListModification.SubtractNeutralsAndAllies(allProjectiles, team);
    }
    private Vector2 CalculateSingleProjectileAvoidance(GameObject projectile, Rigidbody2D rb2D)
    {
        Vector2 positionToEnemy = HelperMethods.VectorUtils.DeltaPosition(gameObject, projectile);
        if (!IsEnemyAhead(projectile, rb2D))
        {
            return Vector2.zero;
        }

        Vector2 projectedPosition = Vector3.Project(positionToEnemy, rb2D.velocity);
        // This is the delta vector towards the closest point that the projectile will pass by next to the ship
        Vector2 projectilePassPosition = positionToEnemy - projectedPosition;
        float projectilePassDistance = projectilePassPosition.magnitude;

        bool projectileWillMiss = projectilePassDistance > shipSize;
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
        Vector2 positionToEnemy = HelperMethods.VectorUtils.DeltaPosition(gameObject, projectile);
        float dot = Vector2.Dot(rb2D.velocity, positionToEnemy);
        return dot < 0;
    }
    #endregion

    #region Random movement
    private void StartRandomMovementCoroutine()
    {
        StartCoroutine(SetRandomMovementDirection());
    }
    private IEnumerator SetRandomMovementDirection()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minMovementPeriod, maxMovementPeriod));
            float randomMovementRotation = transform.rotation.eulerAngles.z + Random.Range(-randomMovementAngle, randomMovementAngle);
            randomMovementVector = HelperMethods.VectorUtils.RotateVector(Vector2.up, randomMovementRotation);
        }
    }
    private Vector2 CalculateRandomMovementVector()
    {
        Vector2 followVector = CalculateFollowVector();
        float followLength = Mathf.Min(followVector.magnitude, 1);
        Debug.Log("Follow vector " + followVector);
        return followLength * followVector.normalized;
        //return followLength * followVector.normalized + (1 - followLength) * randomMovementVector;
    }
    private Vector2 CalculateFollowVector()
    {
        if (followObject == null)
        {
            return Vector2.zero;
        }
        Vector2 deltaPositionToFollowObject = HelperMethods.VectorUtils.DeltaPosition(gameObject, followObject);
        if (deltaPositionToFollowObject.magnitude < followLeash)
        {
            return Vector2.zero;
        }
        float followVectorMagnitude = (deltaPositionToFollowObject.magnitude - followLeash) / followLeash;
        return followVectorMagnitude * deltaPositionToFollowObject.normalized;
    }
    #endregion
    #endregion

    #region Edge of map
    private Vector2 ApplyEdgeOfMapVector(Vector2 movementVector)
    {
        // If at the edge of map, the ship is forced to come back
        Vector2 edgeOfMapVector = Vector2.zero;

        return edgeOfMapVector + movementVector * (1 - edgeOfMapVector.magnitude);
    }
    #endregion

    #region Update tactics
    private void UpdateTactics()
    {
        if (targetToChase == null)
        {
            return;
        }
        //shipController.targetToChase;
        if (fightTactics.battleMode == BattleMode.MELEE)
        {
            if (fightTactics.movementMode == MovementMode.CHASING)
            {
                float distanceToTarget = HelperMethods.VectorUtils.Distance(gameObject, targetToChase);
                // If the ship gets below this distance to a target, then it either has hit it already, or missed and has no velocity left anymore

                if (distanceToTarget < GetMissRange(targetToChase))
                {
                    StartAvoiding();
                }
            }
            if (fightTactics.movementMode == MovementMode.AVOIDING)
            {
                float distanceToTarget = HelperMethods.VectorUtils.Distance(gameObject, targetToChase);
                // If the ship gets above this range to target, it will start attacking again, as it has enough distance to gain useful velocity.
                float ATTACK_RANGE = chaseRange * 2 / 3;
                // In case that the ship would get stuck at a wall, a delay of two seconds is enough for the ship to return
                float runawayDuration = Time.time - timeSinceStartedRunningAway;
                bool shouldStartChasing = runawayDuration > 4 || distanceToTarget > ATTACK_RANGE;
                if (shouldStartChasing)
                {
                    StartChasing();
                }
            }
        }
        if (fightTactics.battleMode == BattleMode.RANGED)
        {
            // How often the ship will change tactic from offensive to defensive
            if (fightTactics.movementMode == MovementMode.AVOIDING)
            {
                StartShootingAfterDelay();
            }
            if (fightTactics.movementMode == MovementMode.CHASING)
            {
                StartFleeingAfterDelay();
            }
        }
    }
    private float GetMissRange(GameObject target)
    {
        AIShipController otherShipController = target.GetComponentInParent<AIShipController>();
        if (otherShipController)
        {
            return otherShipController.shipSize + shipSize;
        }
        else
        {
            return 1;
        }
    }
    
    private void StartShootingAfterDelay()
    {
        if (!orderSent)
        {
            orderSent = true;
            StartCoroutine(SetMovementModeAfterDelay(MovementMode.CHASING, Random.Range(minFleeDuration, maxFleeDuration)));
        }
    }
    private void StartFleeingAfterDelay()
    {
        if (!orderSent)
        {
            orderSent = true;
            StartCoroutine(SetMovementModeAfterDelay(MovementMode.AVOIDING, Random.Range(minMovementPeriod, maxMovementPeriod)));
        }
    }
    private void StartAvoiding()
    {
        if (!orderSent)
        {
            timeSinceStartedRunningAway = Time.time;
            orderSent = true;
            StartCoroutine(SetMovementModeAfterDelay(MovementMode.AVOIDING));
        }
    }
    private void StartChasing()
    {
        if (!orderSent)
        {
            orderSent = true;
            StartCoroutine(SetMovementModeAfterDelay(MovementMode.CHASING));
        }
    }
    /// <summary>
    /// Default value of 1 is enough for the ship to bump into its target
    /// </summary>
    private IEnumerator SetMovementModeAfterDelay(MovementMode newMode, float delay = 0.3f)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Started " + newMode.ToString());
        fightTactics.movementMode = newMode;
        orderSent = false;
    }
    #endregion
    #endregion

    #region Mutator methods
    public void NofityOnDestroy(GameObject obj)
    {
        gunCount--;
        CheckForMeleeMode();
    }
    private void CheckForMeleeMode()
    {
        if (gunCount == 0)
        {
            fightTactics.battleMode = BattleMode.MELEE;
        }
    }
    #endregion
    private class FightTactics
    {
        public FightTactics()
        {
            battleMode = BattleMode.RANGED;
            movementMode = MovementMode.CHASING;
        }

        public BattleMode battleMode;
        public MovementMode movementMode;

    }
}
public class ActionCallData
{
    public EntityInputs input;
    /// <summary>
    /// The Percentage of max force that the action should be called with
    /// </summary>
    public float percentage;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;
using static StaticDataHolder;
using static UnityEditor.Progress;

public class ShipController : TeamUpdater, ISerializationCallbackReceiver
{
    [SerializeField][Tooltip("Will stop chasing its target above that distance")] float chaseRange;
    [SerializeField][Tooltip("Will start avoiding obstacles below that distance")] float avoidRange;
    [SerializeField][Tooltip("Will not come closer to target than this distance")] float attackRange;
    [SerializeField] bool isForceGlobal;
    [SerializeField][Range(0, 1)][Tooltip("0 - obstacles ignored when chasing; 1 - obstacles avoided at close range even when chasing")] float obstacleAvoidance;

    IEntityMover myVehicle;
    private Rigidbody2D rb2D;
    public GameObject targetToChase;
    private MovementMode movementMode;
    private IParent myParent;

    //Random movement
    private float movementPeriod;

    enum MovementMode
    {
        CHASING,
        AVOIDING,
        IDLE
    }

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetupStartingVariables();
        GenerateRandomMovementVariables();
        //SetNotMouseControlled();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        myVehicle = GetComponent<IEntityMover>();
        myParent = gameObject.GetComponentInParent<IParent>();
    }

    private void GenerateRandomMovementVariables()
    {
        movementPeriod = Random.Range(2, 8);
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
        if (chaseRange < avoidRange)
        {
            chaseRange = avoidRange + 1;
        }
        if (chaseRange < attackRange)
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
        Vector2 movementVector = CalculateMovementVector();
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
        Vector2 chaseVector = CalculateChaseVector();
        //Debug.Log(chaseVector);

        List<GameObject> avoidObjects = GetObjectsToAvoid();
        Vector2 avoidVector = CalculateAvoidVector(avoidObjects);

        Vector2 randomMovement = CalculateRandomMovementVector();

        float chaseLength = chaseVector.magnitude;

        if (chaseLength < 1)
        {
            Vector2 movementVector = chaseVector + (1-chaseLength)* randomMovement;
            return chaseLength * movementVector + (1 + obstacleAvoidance - chaseLength) * avoidVector.normalized;
        }

        if (chaseLength > 1)
        {
            chaseLength = 1;
        }
        return chaseLength * chaseVector.normalized + (1 + obstacleAvoidance - chaseLength) * avoidVector.normalized;
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
        bool isAboveAttackRange = deltaPositionToItem.magnitude > attackRange;
        if (isAboveAttackRange)
        {
            // Prioritize chasing more, if farther away from the target!
            float multiplier = deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(transform.position, deltaPositionToItem.normalized * multiplier, Color.blue, 0.05f);
            return deltaPositionToItem;
        }
        else
        {
            // Fall back, if too close to target. Keep distance!
            float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(transform.position, -deltaPositionToItem.normalized * multiplier, Color.red, 0.05f);
            return -deltaPositionToItem.normalized * multiplier;
        }
    }
    #endregion

    #region Obstacle Avoidance
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

    #region Random movement
    private Vector2 CalculateRandomMovementVector()
    {
        float period = (Time.time / movementPeriod) * 2 * Mathf.PI;
        return transform.right * Mathf.Sin(period);
    }
    #endregion
    #endregion

    #endregion
}
public class ActionCallData
{
    public EntityInputs input;
    /// <summary>
    /// The Percentage of max force that the action should be called with
    /// </summary>
    public float percentage;
}

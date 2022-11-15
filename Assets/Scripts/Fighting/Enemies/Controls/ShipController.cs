using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;
using static StaticDataHolder;

[RequireComponent(typeof(EntityInput))]
public class ShipController : TeamUpdater
{
    [SerializeField] float chaseRange;
    [SerializeField] float avoidRange;
    [SerializeField] bool isForceGlobal;

    private EntityInput entityInput;
    private Rigidbody2D rb2D;

    #region Startup
    protected override void Start()
    {
        base.Start();
        entityInput = GetComponent<EntityInput>();
        rb2D = GetComponent<Rigidbody2D>();
        fixRange();
    }
    private void fixRange()
    {
        if (chaseRange < avoidRange)
        {
            chaseRange = avoidRange;
        }
    }
    #endregion

    #region Update
    void Update()
    {
        Vector2 movementVector = CalculateMovementVector();
        Debug.DrawRay(transform.position, movementVector, Color.blue, 0.1f);
        ActionCallData[] callActions = entityInput.controlScheme.actionCalculator.CalculateActionsToCall(TranslateMovementVector(movementVector));
        CallInputActions(callActions);
    }
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
        float entityGlobalAngle = rb2D.gameObject.transform.rotation.eulerAngles.z;
        Vector2 rotatedVector = HelperMethods.VectorUtils.RotateVector(globalForce, entityGlobalAngle);
        //Debug.Log("Rotate by angle: " + entityGlobalAngle);
        Debug.DrawRay(transform.position, rotatedVector, Color.green, 0.1f);

        return rotatedVector;
    }

    #region Movement vector
    private Vector2 CalculateMovementVector()
    {
        Vector2 proximityVector = Vector2.zero;
        List<GameObject> chaseObjects = ListContents.Enemies.GetEnemyList(team);
        List<GameObject> avoidObjects = GetObjectsToAvoid();

        foreach (var item in chaseObjects)
        {
            Vector2 deltaPositionToItem = CountDeltaPositionToItem(item);
            if (!IsInChaseRange(deltaPositionToItem))
            {
                continue;
            }

            proximityVector += HandleChaseObject(deltaPositionToItem);
        }
        foreach (var item in avoidObjects)
        {
            Vector2 deltaPositionToItem = HelperMethods.LineOfSightUtils.EdgeDeltaPosition(gameObject, item);
            bool isInChaseRange = deltaPositionToItem.magnitude < chaseRange;
            if (!IsInChaseRange(deltaPositionToItem))
            {
                continue;
            }

            proximityVector += HandleAvoidObject(deltaPositionToItem);
        }
        return proximityVector.normalized;
    }
    private Vector2 CountDeltaPositionToItem(GameObject item)
    {
        return HelperMethods.LineOfSightUtils.EdgeDeltaPosition(gameObject, item);
    }
    private bool IsInChaseRange(Vector2 deltaPositionToItem)
    {
        return deltaPositionToItem.magnitude < chaseRange;

    }
    private Vector2 HandleChaseObject(Vector2 deltaPositionToItem)
    {
        float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
        bool isInAvoidRange = deltaPositionToItem.magnitude < avoidRange;
        if (!isInAvoidRange)
        {
            Debug.DrawRay(transform.position, deltaPositionToItem, Color.red, 0.05f);
        }
        else
        {
            Debug.DrawRay(transform.position, -deltaPositionToItem, Color.yellow, 0.05f);
            multiplier *= -1;
        }
        return deltaPositionToItem.normalized * multiplier;
    }
    private Vector2 HandleAvoidObject(Vector2 deltaPositionToItem)
    {
        float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
        bool isInAvoidRange = deltaPositionToItem.magnitude < avoidRange;
        if (!isInAvoidRange)
        {
            //Debug.DrawRay(transform.position, deltaPositionToItem, Color.red, 0.05f);
            multiplier = 0;
        }
        else
        {
            Debug.DrawRay(transform.position, -deltaPositionToItem, Color.yellow, 0.05f);
            multiplier *= -1;
        }
        return deltaPositionToItem.normalized * multiplier;
    }
    private List<GameObject> GetObjectsToAvoid()
    {
        List<GameObject> avoidObjects = ListContents.Allies.GetAllyList(team, gameObject);
        RemoveMyParts(avoidObjects);
        avoidObjects.AddRange(ListContents.Generic.GetObjectList(ObjectTypes.Obstacle));
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
        IParent myParent = gameObject.GetComponentInParent<IParent>();

        if (partParent == null || myParent == null)
        {
            return false;
        }
        return partParent.GetGameObject() == myParent.GetGameObject();
    }
    #endregion

    #region Actions
    private EntityInputs[] CalculateActionsToCall(Vector2 movementVector)
    {
        List<EntityInputs> entityInputs = new List<EntityInputs>();

        TryAddAction(entityInputs, movementVector.y > 0, EntityInputs.FORWARD);
        TryAddAction(entityInputs, movementVector.y < 0, EntityInputs.BACKWARD);
        TryAddAction(entityInputs, movementVector.x > 0, EntityInputs.RIGHT);
        TryAddAction(entityInputs, movementVector.x < 0, EntityInputs.LEFT);
        FillInActions(entityInputs);
        TryAddAction(entityInputs, movementVector.x > 0.7, EntityInputs.DOUBLE_RIGHT);
        TryAddAction(entityInputs, movementVector.x < -0.7, EntityInputs.DOUBLE_LEFT);

        return entityInputs.ToArray();
    }
    private void TryAddAction(List<EntityInputs> entityInputs, bool requirement, EntityInputs input)
    {
        if (requirement)
        {
            entityInputs.Add(input);
        }
    }
    private void FillInActions(List<EntityInputs> entityInputs)
    {
        bool forwardLeft = entityInputs.Contains(EntityInputs.FORWARD) && entityInputs.Contains(EntityInputs.LEFT);
        TryAddAction(entityInputs, forwardLeft, EntityInputs.FORWARD_LEFT);
        bool forwardRight = entityInputs.Contains(EntityInputs.FORWARD) && entityInputs.Contains(EntityInputs.RIGHT);
        TryAddAction(entityInputs, forwardRight, EntityInputs.FORWARD_RIGHT);
        bool backwardLeft = entityInputs.Contains(EntityInputs.BACKWARD) && entityInputs.Contains(EntityInputs.LEFT);
        TryAddAction(entityInputs, backwardLeft, EntityInputs.BACKWARD_LEFT);
        bool backwardRight = entityInputs.Contains(EntityInputs.BACKWARD) && entityInputs.Contains(EntityInputs.RIGHT);
        TryAddAction(entityInputs, backwardRight, EntityInputs.BACKWARD_RIGHT);
    }
    private void CallInputActions(ActionCallData[] callActions)
    {
        foreach (var action in callActions)
        {
            entityInput.TryCallAction(action, true);
        }
    }
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

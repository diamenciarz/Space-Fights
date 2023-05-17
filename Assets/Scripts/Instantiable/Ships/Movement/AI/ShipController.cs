using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;
using static StaticDataHolder;
using static UnityEditor.Progress;

public class ShipController : TeamUpdater
{
    [SerializeField] float chaseRange;
    [SerializeField] float avoidRange;
    [SerializeField] bool isForceGlobal;

    IEntityMover myVehicle;
    private Rigidbody2D rb2D;
    public GameObject targetToChase;
    private MovementMode movementMode;

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
        FixRange();
        SetNotMouseControlled();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        myVehicle = GetComponent<IEntityMover>();
    }
    private void FixRange()
    {
        if (chaseRange < avoidRange)
        {
            chaseRange = avoidRange;
        }
    }
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
        float entityGlobalAngle = rb2D.gameObject.transform.rotation.eulerAngles.z;
        Vector2 rotatedVector = HelperMethods.VectorUtils.RotateVector(globalForce, entityGlobalAngle);
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
        Debug.Log(chaseVector);

        List<GameObject> avoidObjects = GetObjectsToAvoid();
        Vector2 avoidVector = CalculateAvoidVector(avoidObjects);

        if (avoidVector.magnitude > 1)
        {
            return avoidVector.normalized;
        }
        else
        {
            if (avoidVector.magnitude > chaseVector.magnitude)
            {
                return avoidVector;
            }
            else
            {
                if (chaseVector.magnitude > 1)
                {
                    chaseVector = chaseVector.normalized;
                }
                return chaseVector + avoidVector;
            }
        }
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
        bool isInAvoidRange = deltaPositionToItem.magnitude < avoidRange;
        if (isInAvoidRange)
        {
            float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(transform.position, -deltaPositionToItem.normalized * multiplier, Color.red, 0.05f);
            return -deltaPositionToItem.normalized * multiplier;
        }
        else
        {
            float multiplier = deltaPositionToItem.sqrMagnitude;
            Debug.DrawRay(transform.position, deltaPositionToItem.normalized * multiplier, Color.blue, 0.05f);
            return deltaPositionToItem;
        }
    }
    #endregion

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
        IParent myParent = gameObject.GetComponentInParent<IParent>();

        if (partParent == null || myParent == null)
        {
            return false;
        }
        return partParent.GetGameObject() == myParent.GetGameObject();
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

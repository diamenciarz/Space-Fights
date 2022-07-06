using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;
using static StaticDataHolder;

[RequireComponent(typeof(EntityInput))]
public class ShipController : TeamUpdater
{
    private EntityInput entityInput;

    void Start()
    {
        entityInput = GetComponent<EntityInput>();
    }

    #region Update
    void Update()
    {
        Vector2 movementVector = CalculateMovementVector();
        Debug.DrawRay(transform.position, (Vector2)transform.position + movementVector, Color.blue, 0.1f);
        EntityInputs[] callActions = CalculateActionsToCall(movementVector);
        CallInputActions(callActions);
    }

    #region Movement vector
    private Vector2 CalculateMovementVector()
    {
        Vector2 proximityVector = Vector2.zero;
        List<GameObject> chaseObjects = ListContents.Enemies.GetEnemyList(team);
        List<GameObject> avoidObjects = GetObjectsToAvoid();
        foreach (var item in chaseObjects)
        {
            Vector2 deltaPositionToItem = HelperMethods.LineOfSightUtils.EdgeDeltaPosition(gameObject, item);
            Debug.DrawRay(transform.position, (Vector2)transform.position + deltaPositionToItem, Color.red, 0.05f);
            float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
            proximityVector += deltaPositionToItem.normalized * multiplier;
        }
        foreach (var item in avoidObjects)
        {
            Vector2 deltaPositionToItem = HelperMethods.LineOfSightUtils.EdgeDeltaPosition(gameObject, item);
            Debug.DrawRay(transform.position, (Vector2)transform.position + deltaPositionToItem, Color.red, 0.05f);
            float multiplier = 1 / deltaPositionToItem.sqrMagnitude;
            proximityVector -= deltaPositionToItem.normalized * multiplier;
        }

        return proximityVector.normalized;
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
    private void CallInputActions(EntityInputs[] callActions)
    {
        foreach (var action in callActions)
        {
            entityInput.TryCallAction(action, true);
        }
    }
    #endregion
    #endregion

}

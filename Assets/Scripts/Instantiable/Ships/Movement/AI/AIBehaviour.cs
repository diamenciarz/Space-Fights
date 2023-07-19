using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIBehaviour: TeamUpdater
{
    [SerializeField] float shipSize = 1.5f;
    [SerializeField] bool isForceGlobal = false;
    [SerializeField] Behaviour[] behaviours;

    IEntityMover myVehicle;
    private IParent myParent;
    private Rigidbody2D rb2D;
    private float lastBehaviourChangeTime;
    private int behaviourIndex = 0;

    #region Startup
    protected override void Start()
    {
        base.Start();
        rb2D = GetComponent<Rigidbody2D>();
        myVehicle = GetComponent<IEntityMover>();
        myParent = gameObject.GetComponentInParent<IParent>();
    }
    #endregion

    #region Update
    void Update()
    {
        if(behaviours.Length == 0)
        {
            return;
        }

        UpdateBehaviourIndex();
        MovementBehaviourData data = GetMovementData();
        Behaviour behaviour = behaviours[behaviourIndex];
        Vector2 movementVector = behaviour.movementBehaviour.CalculateMovementVector(data);
        myVehicle.SetInputVector(TranslateMovementVector(movementVector));
    }
    private MovementBehaviourData GetMovementData()
    {
        MovementBehaviourData data = new MovementBehaviourData();
        data.parent = myParent;
        data.position = transform.position;
        data.shipSize = shipSize;
        data.gameObject = gameObject;
        data.velocity = rb2D.velocity;
        data.team = team;
        return data;
    }
    private void UpdateBehaviourIndex()
    {
        if (behaviours.Length == 1)
        {
            return;
        }
        if (behaviours[behaviourIndex].condition.IsSatisfied(GetConditionData()))
        {
            behaviourIndex++;
            if (behaviourIndex == behaviours.Length)
            {
                behaviourIndex = 0;
            }
            lastBehaviourChangeTime = Time.time;
        }
    }
    private ConditionData GetConditionData()
    {
        ConditionData conditionData = new ConditionData();
        conditionData.lastBehaviourChangeTime = lastBehaviourChangeTime;
        conditionData.gameObject = gameObject;
        return conditionData;
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
    #endregion
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AIBehaviour: TeamUpdater
{
    [SerializeField] float shipSize = 1.5f;
    [SerializeField] bool isForceGlobal = false;
    [SerializeField] MovementBehaviour[] behaviours;

    IEntityMover myVehicle;
    private IParent myParent;
    private Rigidbody2D rb2D;


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
        MovementBehaviourData data = GetMovementData();
        MovementBehaviour behaviour = behaviours[0];
        Vector2 movementVector = behaviour.CalculateMovementVector(data);
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
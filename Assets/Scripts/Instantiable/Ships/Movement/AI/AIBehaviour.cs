using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;

[Serializable]
public class AIBehaviour: TeamUpdater, INotifyOnDestroy
{
    [SerializeField] float shipSize = 1.5f;
    [SerializeField] bool isForceGlobal = false;
    [SerializeField] BehaviourCollection behaviours;

    IEntityMover myVehicle;
    private IParent myParent;
    private Rigidbody2D rb2D;
    private float lastBehaviourChangeTime;
    private int behaviourIndex = 0;
    private bool conditionChanged;
    private int gunCount;

    #region Startup
    protected override void Start()
    {
        base.Start();
        SetupStartingVariables();
        CountGuns();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        myVehicle = GetComponent<IEntityMover>();
        myParent = gameObject.GetComponentInParent<IParent>();
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
    #endregion

    #region Update
    void Update()
    {
        if(behaviours == null)
        {
            myVehicle.SetInputVector(TranslateMovementVector(Vector2.zero));
            return;
        }

        MovementBehaviourData data = GetMovementData();
        MovementBehaviour behaviour = behaviours.GetMovementBehaviour(GetConditionData());
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
    private ConditionData GetConditionData()
    {
        ConditionData conditionData = new ConditionData();
        conditionData.lastBehaviourChangeTime = lastBehaviourChangeTime;
        conditionData.gameObject = gameObject;
        conditionData.target = ListContents.Enemies.GetClosestEnemy(transform.position, team);
        conditionData.firstConditionCall = conditionChanged;
        conditionData.gunCount = gunCount;
        conditionData.shipSize = shipSize;
        conditionData.team = team;
        conditionChanged = false;
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

    #region Mutator methods
    public void NofityOnDestroy(GameObject obj)
    {
        if (DestroyedGun(obj))
        {
            gunCount--;
        }
    }
    private bool DestroyedGun(GameObject obj)
    {
        GunController controller = obj.GetComponentInChildren<GunController>();
        return controller != null;
    }
    #endregion
}
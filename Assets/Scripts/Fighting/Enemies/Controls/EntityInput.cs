using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityMover))]
public class EntityInput : MonoBehaviour
{
    public List<ActionTriplet> controls;
    public bool controlledByPlayer;

    private Rigidbody2D rb2D;
    private EntityMover entityMover;
    private ActionData actionData = new ActionData();

    public enum EntityInputs
    {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        DOUBLE_LEFT,
        DOUBLE_RIGHT,
        FORWARD_LEFT,
        FORWARD_RIGHT,
        BACKWARD_LEFT,
        BACKWARD_RIGHT,
        PRIMARY_ACTION,
        SECONDARY_ACTION,
        TERNARY_ACTION
    }

    [Serializable]
    public class ActionTriplet
    {
        public KeyCode key;
        public EntityInputs type;
        public ShipAction action;
        public List<ShootingController> shootingControllers;

    }
    public class ActionData
    {
        public Rigidbody2D rigidbody2D;
        public EntityMover entityMover;
    }
    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        entityMover = GetComponent<EntityMover>();
        actionData.entityMover = entityMover;
        actionData.rigidbody2D = rb2D;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < controls.Count; i++)
        {
            CheckIfKeyPressed(i);
        }
    }

    private void CheckIfKeyPressed(int i)
    {
        if (!controlledByPlayer)
        {
            return;
        }

        ActionTriplet actionTriplet = controls[i];
        if (Input.GetKey(actionTriplet.key))
        {
            callAction(actionTriplet, true);
        }
        else
        {
            callAction(actionTriplet, false);
        }
    }

    private void callAction(ActionTriplet actionTriplet, bool isOn)
    {
        if (actionTriplet.action && isOn)
        {
            actionTriplet.action.callAction(actionData);
        }

        foreach (ShootingController controller in actionTriplet.shootingControllers)
        {
            controller.SetShoot(isOn);
        }
    }
}

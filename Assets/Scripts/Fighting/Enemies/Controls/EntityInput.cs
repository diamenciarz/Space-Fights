using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityMover))]
public class EntityInput : MonoBehaviour
{
    public List<ActionTriplet> controls;

    public enum EntityInputs
    {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        DOUBLE_LEFT,
        DOUBLE_RIGHT,
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
    }

    private Rigidbody2D rb2D;
    public EntityMover entityMover;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        entityMover = GetComponent<EntityMover>();
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
        ActionTriplet actionTriplet = controls[i];
        if (Input.GetKey(actionTriplet.key))
        {
            callAction(actionTriplet);
        }
    }

    private void callAction(ActionTriplet actionTriplet)
    {
        actionTriplet.action.callAction(rb2D, entityMover);
    }
}

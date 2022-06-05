using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementScheme", menuName = "Moves/Controls")]
public class MovementScheme : ScriptableObject
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
        FORWARD_LEFT,
        FORWARD_RIGHT,
        BACKWARD_LEFT,
        BACKWARD_RIGHT,
        PRIMARY_ACTION,
        SECONDARY_ACTION,
        TERNARY_ACTION,
        SHIFT,
        SPACE,
        ALWAYS_ACTIVE
    }

    [Serializable]
    public class ActionTriplet
    {
        public KeyCode key;
        public EntityInputs type;
        public ShipAction action;
        public List<ShootingController> shootingControllers;
    }
}

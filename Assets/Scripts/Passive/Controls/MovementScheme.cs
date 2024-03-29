using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementScheme", menuName = "Moves/Controls")]
public class MovementScheme : ScriptableObject
{
    public List<ActionTriplet> controls;
    public AbstractActionCalculator actionCalculator;

    public enum EntityInputs
    {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        DOUBLE_LEFT,
        DOUBLE_RIGHT,
        DOUBLE_FORWARD,
        DOUBLE_BACKWARD,
        LEFT_ROTATION,
        RIGHT_ROTATION,
        PRIMARY_ACTION,
        SECONDARY_ACTION,
        TERNARY_ACTION,
        ALWAYS_ACTIVE
    }

    [Serializable]
    public class ActionTriplet
    {
        public KeyCode key;
        public EntityInputs type;
        public AbstractShipAction action;
        public List<ActionController> actionControllers;
    }
}

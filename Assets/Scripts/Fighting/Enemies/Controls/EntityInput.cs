using System;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static MovementScheme;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EntityMover))]
public class EntityInput : MonoBehaviour
{
    public bool controlledByPlayer;
    public MovementScheme controlScheme;
    public ActionTriplet primaryAction;
    public ActionTriplet secondaryAction;
    public ActionTriplet ternaryAction;

    private Rigidbody2D rb2D;
    private EntityMover entityMover;
    private ActionData actionData = new ActionData();
    
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
        foreach (ActionTriplet controls in controlScheme.controls)
        {
            CheckIfKeyPressed(controls);
        }
        CheckIfKeyPressed(primaryAction);
        CheckIfKeyPressed(secondaryAction);
        CheckIfKeyPressed(ternaryAction);
    }

    private void CheckIfKeyPressed(ActionTriplet actionTriplet)
    {
        if (actionTriplet == null)
        {
            return;
        }
        if (!controlledByPlayer)
        {
            return;
        }

        bool isActionActive = actionTriplet.type == EntityInputs.ALWAYS_ACTIVE || Input.GetKey(actionTriplet.key);
        if (isActionActive)
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

        ActionControllerData data = new ActionControllerData(isOn);
        foreach (ActionController controller in actionTriplet.actionControllers)
        {
            controller.UpdateController(data);
        }
    }
}

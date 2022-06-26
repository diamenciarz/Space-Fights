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
    private List<ActionTriplet> allActionTriplets = new List<ActionTriplet>();

    public class ActionData
    {
        public Rigidbody2D rigidbody2D;
        public EntityMover entityMover;
    }

    #region Startup
    private void Start()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        entityMover = GetComponent<EntityMover>();
        actionData.entityMover = entityMover;
        actionData.rigidbody2D = rb2D;
        SetupAllActionTriplets();
    }
    private void SetupAllActionTriplets()
    {
        allActionTriplets.AddRange(controlScheme.controls);
        allActionTriplets.Add(primaryAction);
        allActionTriplets.Add(secondaryAction);
        allActionTriplets.Add(ternaryAction);
        RemoveNullActions();
    }
    private void RemoveNullActions()
    {
        for (int i = allActionTriplets.Count - 1; i >= 0; i--)
        {
            if (allActionTriplets[i] == null)
            {
                allActionTriplets.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Update
    private void FixedUpdate()
    {
        foreach (ActionTriplet controls in allActionTriplets)
        {
            CheckIfKeyPressed(controls);
        }
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
            CallAction(actionTriplet, true);
        }
        else
        {
            CallAction(actionTriplet, false);
        }
    }
    private void CallAction(ActionTriplet actionTriplet, bool isOn)
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
    #endregion

    #region Mutator methods
    public void TryCallAction(EntityInputs input, bool isOn)
    {
        ActionTriplet action = FindAction(input);
        if (action != null)
        {
            CallAction(action, isOn);
        }
    }
    private ActionTriplet FindAction(EntityInputs input)
    {
        try
        {
            foreach (ActionTriplet action in allActionTriplets)
            {
                if (action.type == input)
                {
                    return action;
                }
            }
            return null;
        }
        catch (Exception)
        {
            Debug.LogError("Action was null!");
            return null;
        }
    }
    #endregion
}

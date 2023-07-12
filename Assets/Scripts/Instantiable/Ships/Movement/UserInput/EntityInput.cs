using System;
using System.Collections.Generic;
using UnityEngine;
using static ActionController;
using static MovementScheme;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityInput : MonoBehaviour, IActionControllerCaller, IPlayerControllable
{
    public bool isControlledByPlayer;
    public List<ActionTriplet> actions;

    private Rigidbody2D rb2D;
    private IEntityMover entityMover;
    private ActionData actionData;
    private List<ActionTriplet> allActionTriplets = new List<ActionTriplet>();

    public class ActionData
    {
        public Rigidbody2D rigidbody2D;
        public IEntityMover entityMover;
        public float percentage;
    }

    #region Startup
    private void Start()
    {
        SetupStartingVariables();
        AddActionsToListeners();
    }
    private void SetupStartingVariables()
    {
        rb2D = GetComponent<Rigidbody2D>();
        entityMover = GetComponent<IEntityMover>();
        DefineActionData();
        SetupAllActionTriplets();
    }
    private void DefineActionData()
    {
        actionData = new ActionData();
        actionData.entityMover = entityMover;
        actionData.rigidbody2D = rb2D;
    }
    private void SetupAllActionTriplets()
    {
        allActionTriplets.AddRange(actions);
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
    private void AddActionsToListeners()
    {
        foreach (var action in actions)
        {
            AddToListener(action);
        }
    }
    private void AddToListener(ActionTriplet actionTriplet)
    {
        foreach (ActionController controller in actionTriplet.actionControllers)
        {
            controller.AddActionCaller(this, actionTriplet.type);
        }
    }
    #endregion

    #region Update
    private void FixedUpdate()
    {
        CheckKeyPresses();
    }
    private void CheckKeyPresses()
    {
        foreach (ActionTriplet controls in allActionTriplets)
        {
            CheckIfKeyPressed(controls);
        }
    }
    private void CheckIfKeyPressed(ActionTriplet actionTriplet)
    {
        if (IsManualInputOff(actionTriplet))
        {
            return;
        }

        if (IsActionActive(actionTriplet))
        {
            CallAction(actionTriplet, true, 1);
        }
        else
        {
            CallAction(actionTriplet, false, 1);
        }
    }
    private bool IsManualInputOff(ActionTriplet actionTriplet)
    {
        return actionTriplet == null || !isControlledByPlayer;
    }
    private bool IsActionActive(ActionTriplet actionTriplet)
    {
        return actionTriplet.type == EntityInputs.ALWAYS_ACTIVE || Input.GetKey(actionTriplet.key);
    }
    private void CallAction(ActionTriplet actionTriplet, bool isOn, float percentage)
    {
        bool actionIsOn = actionTriplet.action != null && isOn;
        if (actionIsOn)
        {
            actionData.percentage = percentage;
            actionTriplet.action.CallAction(actionData);
        }
        //The Action controllers are calling themselves using the listener pattern
    }
    #endregion

    #region Mutator methods
    /// <summary>
    /// Action caller used by Ship controller. 
    /// It tries to call all actions compatible with the movement vector but only some of them are defined for the ship.
    /// This might be changed into a strategy pattern for calling actions based on the movement vector differently for each implementation.
    /// </summary>
    public void TryCallAction(ActionCallData data, bool isOn)
    {
        ActionTriplet action = FindAction(data.input);
        if (action != null)
        {
            CallAction(action, isOn, data.percentage);
        }
    }
    private ActionTriplet FindAction(EntityInputs input)
    {
        //try
        //{
        foreach (ActionTriplet action in allActionTriplets)
        {
            if (action.type == input)
            {
                return action;
            }
        }
        return null;
        /*}
        catch (Exception)
        {
            Debug.LogError("Action was null!");
            return null;
        }*/
    }
    public void SetIsControlledByMouse(bool isOn)
    {
        isControlledByPlayer = isOn;
    }
    #endregion

    #region Accessor methods
    public bool IsOn(EntityInputs input)
    {
        ActionTriplet actionTriplet = FindAction(input);
        if (IsManualInputOff(actionTriplet))
        {
            return false;
        }

        return IsActionActive(actionTriplet);
    }
    public ActionControllerData GetData()
    {
        return new ActionControllerData();
    }
    #endregion
}

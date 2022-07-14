using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MovementScheme;

public abstract class ActionController : TeamUpdater
{
    List<CallerPair> callers = new List<CallerPair>();
    IActionControllerCaller firstActiveCaller;
    public void AddActionCaller(IActionControllerCaller caller)
    {
        callers.Add(new CallerPair(caller));
    }
    public void AddActionCaller(IActionControllerCaller caller, EntityInputs input)
    {
        callers.Add(new CallerPair(caller, input));
    }
    protected bool IsControllerOn()
    {
        foreach (var callerPair in callers)
        {
            bool callerIsOn = callerPair.caller.IsOn(callerPair.input);
            if (callerIsOn)
            {
                firstActiveCaller = callerPair.caller;
                return true;
            }
        }
        return false;
    }
    protected ActionControllerData GetActionControllerData()
    {
        return firstActiveCaller.GetData();
    }
    public class ActionControllerData
    {
        public ActionControllerData()
        {
            this.target = null;
        }
        public ActionControllerData(GameObject target)
        {
            this.target = target;
        }
        public GameObject target;
    }
    private class CallerPair
    {
        public CallerPair(IActionControllerCaller caller)
        {
            this.caller = caller;
            input = EntityInputs.ALWAYS_ACTIVE;
        }
        public CallerPair(IActionControllerCaller caller, EntityInputs input)
        {
            this.caller = caller;
            this.input = input;
        }

        public IActionControllerCaller caller;
        public EntityInputs input;
    }
}

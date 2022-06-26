using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionController : TeamUpdater
{
    public abstract void UpdateController(ActionControllerData data);
    public class ActionControllerData
    {
        public ActionControllerData(bool isOn)
        {
            this.isOn = isOn;
        }
        public bool isOn;
        public GameObject target;
    }
}

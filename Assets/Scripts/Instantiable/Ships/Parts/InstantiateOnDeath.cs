using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateOnDeath : TriggerOnDeath
{
    [SerializeField] GameObject objToInstantiate;

    protected override void DoDestroyAction()
    {
        Instantiate(objToInstantiate, transform.position, transform.rotation);
    }
}

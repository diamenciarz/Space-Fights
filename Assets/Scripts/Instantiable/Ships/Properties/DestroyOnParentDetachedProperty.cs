using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParentDetachedProperty : MonoBehaviour, IActOnParentDetach
{
    [SerializeField] GameObject particleEffect;
    public void PerformAction()
    {
        if (particleEffect != null)
        {
            Instantiate(particleEffect);
        }
        Destroy(gameObject);
    }
}

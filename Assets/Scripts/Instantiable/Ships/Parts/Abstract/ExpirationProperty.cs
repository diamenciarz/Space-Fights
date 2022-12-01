using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExpirationProperty : MonoBehaviour
{
    [SerializeField] bool doDestroyActions = true;

    #region Destroy methods
    protected void DestroyObject()
    {
        if (doDestroyActions)
        {
            HelperMethods.CollisionUtils.DoDestroyActions(gameObject, TriggerOnDeath.DestroyCause.InstantBreak);
        }
        StartCoroutine(DestroyAtTheEndOfFrame());
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion
}

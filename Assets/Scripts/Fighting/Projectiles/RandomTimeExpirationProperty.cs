using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RandomTimeExpirationProperty : MonoBehaviour
{
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinity")]
    [SerializeField] [Range(0, 500)] float minExpirationTime = 0;
    [SerializeField] [Range(0, 500)] float maxExpirationTime = 5;
    [SerializeField] bool doDestroyActions = true;

    #region Delay
    private void Start()
    {
        float expirationTime = Random.Range(minExpirationTime, maxExpirationTime);
        if (expirationTime >= 0)
        {
            DestroyDelay(expirationTime);
        }
    }
    private async void DestroyDelay(float delay)
    {
        int dealyInMiliseconds = (int)(1000 * delay);
        await Task.Delay(dealyInMiliseconds);
        DestroyObject();
    }
    #endregion

    #region Destroy methods
    private void DestroyObject()
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

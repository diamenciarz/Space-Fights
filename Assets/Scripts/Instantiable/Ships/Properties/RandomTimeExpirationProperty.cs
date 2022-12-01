using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RandomTimeExpirationProperty : ExpirationProperty
{
    [Tooltip("Destroy the bullet after it has existed for this long.")]
    [SerializeField] [Range(0, 500)] float minExpirationTime = 0;
    [SerializeField] [Range(0, 500)] float maxExpirationTime = 5;

    #region Delay
    private void Start()
    {
        float expirationTime = Random.Range(minExpirationTime, maxExpirationTime);
        if (expirationTime >= 0)
        {
            StartCoroutine(DestroyDelay(expirationTime));
        }
    }
    private IEnumerator DestroyDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyObject();
    }
    #endregion
}

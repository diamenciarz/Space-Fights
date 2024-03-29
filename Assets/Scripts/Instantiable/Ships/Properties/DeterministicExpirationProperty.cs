using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DeterministicExpirationProperty : ExpirationProperty
{
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinite time")]
    public float expireAfterTime = -1;
    [Tooltip("Destroy the bullet after it has travelled this much distance. -1 for infinite distance")]
    public float expirationDistance = -1;

    private float travelledDistance;
    private Vector2 lastFramePosition;


    #region Delay
    private void Start()
    {
        if (expireAfterTime >= 0)
        {
            StartCoroutine(DestroyDelay(expireAfterTime));
        }
    }
    private IEnumerator DestroyDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyObject();
    }
    #endregion

    #region Distance
    private void Update()
    {
        if (expirationDistance >= 0)
        {
            UpdateDistance();
            CheckDistance();
        }
    }
    private void UpdateDistance()
    {
        float distance = HelperMethods.VectorUtils.Distance((Vector2)transform.position, lastFramePosition);
        travelledDistance += distance;
        lastFramePosition = transform.position;
    }
    private void CheckDistance()
    {
        if (travelledDistance >= expirationDistance)
        {
            DestroyObject();
        }
    }
    #endregion
}

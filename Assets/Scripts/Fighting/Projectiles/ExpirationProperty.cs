using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ExpirationProperty : MonoBehaviour
{
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinity")]
    [SerializeField] float expirationTime = -1;
    [Tooltip("Destroy the bullet after it has travelled this much distance. -1 for infinity")]
    [SerializeField] float expirationDistance = -1;
    [SerializeField] bool doDestroyActions = true;

    private float travelledDistance;
    private Vector2 lastFramePosition;


    #region Delay
    private void Start()
    {
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

    #region Distance
    private void Update()
    {
        UpdateDistance();
        CheckDistance();
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

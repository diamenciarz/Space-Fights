using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : BasicProjectileController
{

    [Header("Bomb Settings")]
    // Ustawienia dla bomby
    public float timeToExpire;
    [Tooltip("How many times larger should the explosion get than the starting size")]
    [SerializeField] float expandRate; // Sprite scale
    [SerializeField] float rotateDuringLifetime;

    //Private variables
    private float originalSize;
    private float originalZRotation;

    #region Initialization
    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        originalSize = transform.localScale.x;
        originalZRotation = transform.rotation.eulerAngles.z;
    }
    #endregion

    #region Every frame
    protected void Update()
    {
        UpdateTransform();
        CheckLifetime();
    }
    private void UpdateTransform()
    {
        float lifetimePercentage = (Time.time - creationTime) / timeToExpire;
        UpdateScale(lifetimePercentage);
        UpdateRotation(lifetimePercentage);
    }
    private void UpdateScale(float lifetimePercentage)
    {
        float newSize = lifetimePercentage * expandRate * originalSize;
        gameObject.transform.localScale = new Vector3(newSize, newSize, 0);
    }
    private void UpdateRotation(float lifetimePercentage)
    {
        Quaternion newRotation = Quaternion.Euler(0, 0, originalZRotation + lifetimePercentage * rotateDuringLifetime);
        transform.rotation = newRotation;
    }
    private void CheckLifetime()
    {
        if (Time.time - creationTime > timeToExpire)
        {
            SetSizeToMax();
            Destroy(gameObject);
        }
    }
    private void SetSizeToMax()
    {
        gameObject.transform.localScale = new Vector3(expandRate * originalSize, expandRate * originalSize, 0);
    }
    #endregion
}



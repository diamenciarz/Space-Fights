using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CenteredPushingForce))]
[RequireComponent(typeof(DeterministicExpirationProperty))]

public class ExplosionController : ProjectileController
{

    [Header("Bomb Settings")]
    // Ustawienia dla bomby
    private float timeToExpire;
    [Tooltip("How many times larger should the explosion get than the starting size")]
    [SerializeField] float expandRate; // Sprite scale
    [Tooltip("The total rotation that the explosion will do during its lifetime in degrees")]
    [SerializeField] float rotateDuringLifetime;

    //Private variables
    private CenteredPushingForce pushingForce;
    private DeterministicExpirationProperty expirationProperty;
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
        pushingForce = GetComponent<CenteredPushingForce>();
        expirationProperty = GetComponent<DeterministicExpirationProperty>();

        timeToExpire = expirationProperty.expireAfterTime;
        if (timeToExpire <= 0)
        {
            Debug.LogError("Time to expire in the DeterministicExpirationProperty has to be higher than 0 for the ExplosionController to work");
        }
    }
    #endregion

    #region Transform
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
            if (breakOnCollision)
            {
                breakOnCollision.DestroyObject();
            }
        }
    }
    private void SetSizeToMax()
    {
        gameObject.transform.localScale = new Vector3(expandRate * originalSize, expandRate * originalSize, 0);
    }
    #endregion

    #region Accessor methods
    public float GetExpandRate()
    {
        return expandRate;
    }
    public TeamUpdater.ObjectType GetObjectType()
    {
        return ObjectType.PROJECTILE;
    }
    #endregion
}



using System.Threading.Tasks;
using UnityEngine;

public class ExplosionController : BasicProjectileController
{

    [Header("Bomb Settings")]
    // Ustawienia dla bomby
    public float timeToExpire;
    [Tooltip("How many times larger should the explosion get than the starting size")]
    [SerializeField] float expandRate; // Sprite scale
    [SerializeField] float rotateDuringLifetime;
    [Tooltip("How strong the pushing force of this explosion should be at its strongest point, which is the middle. The force decreases with as distance from the middle gets lower")]
    [SerializeField][Range(0, 1000)] float maxPushingForce;
    [Header("Info")]
    [SerializeField][Range(0.01f, 100)] float startingRadius;

    //Private variables
    private float originalSize;
    private float originalZRotation;
    private bool isDestroyed = false;

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
        if (startingRadius == 0)
        {
            Debug.LogError("startingRadius value cannot be zero!");
        }
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
            breakOnCollision.DestroyObject();
        }
    }
    private void SetSizeToMax()
    {
        gameObject.transform.localScale = new Vector3(expandRate * originalSize, expandRate * originalSize, 0);
    }
    #endregion

    #region OnCollision
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        collision.GetContacts(contacts);

        HandleExplosion(collision.gameObject, contacts);
        base.OnTriggerEnter2D(collision);
    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        HandleExplosion(collision.gameObject, collision.contacts);
        base.OnCollisionEnter2D(collision);
    }
    private void HandleExplosion(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        if (CanPush(collisionObject))
        {
            PushObject(collisionObject, contacts);
        }
    }
    private bool CanPush(GameObject collisionObject)
    {
        IPushable iPushable = collisionObject.GetComponent<IPushable>();
        bool objectCanBePushed = iPushable != null;
        if (!objectCanBePushed)
        {
            return false;
        }
        bool alreadyPushedThisObject = dealtDamageTo.Contains(collisionObject);
        if (alreadyPushedThisObject)
        {
            return false;
        }
        return true;
    }
    private void PushObject(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        IPushable iPushable = collisionObject.GetComponent<IPushable>();
        Vector2 pushingForce = CountForce(collisionObject, contacts);

        iPushable.Push(pushingForce);
    }
    #region Helper methods
    private Vector2 CountForce(GameObject collisionObject, ContactPoint2D[] contacts)
    {
        Vector2 collisionPosition = FindTheClosestCollisionPoint(contacts);
        Vector2 explosionPosition = transform.position;
        Vector2 obstaclePosition = collisionObject.transform.position;
        Vector2 deltaPosition = HelperMethods.VectorUtils.DeltaPosition(explosionPosition, obstaclePosition);
        float distance = deltaPosition.magnitude;
        float maxRadius = startingRadius * expandRate;

        float maxForcePercentage = Mathf.Clamp((maxRadius - distance), 0, maxRadius) / maxRadius;
        //Debug.Log("Max radius: " + maxRadius + " percentage: " + maxForcePercentage);
        float forceStrength = maxPushingForce * maxForcePercentage;
        Vector2 force = deltaPosition.normalized * forceStrength;

        return force;
    }
    private Vector2 FindTheClosestCollisionPoint(ContactPoint2D[] contacts)
    {
        ContactPoint2D closestPoint = contacts[0];
        Vector2 explosionPosition = transform.position;
        foreach (ContactPoint2D contactPoint in contacts)
        {
            bool foundCloserPoint = HelperMethods.VectorUtils.Distance(explosionPosition, contactPoint.point) < HelperMethods.VectorUtils.Distance(explosionPosition, closestPoint.point);
            if (foundCloserPoint)
            {
                closestPoint = contactPoint;
            }
        }
        return closestPoint.point;
    }
    #endregion
    #endregion
}



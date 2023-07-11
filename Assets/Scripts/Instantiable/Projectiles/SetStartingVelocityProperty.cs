using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStartingVelocityProperty : MonoBehaviour, IModifiableStartingSpeed
{
    [SerializeField] bool isOn = true;
    [SerializeField][Tooltip("Forward velocity only adds velocity in the direction the object is facing. Otherwise, the original delta velocity is added")] VelocityMode velocityMode;
    [SerializeField][Range(0,10)] float maxRandomSpeedForward;
    [SerializeField][Range(0,360)] float randomSpeedCone;

    private Rigidbody2D rb2D;
    private enum VelocityMode
    {
        ForwardVelocity,
        AnyDirection
    }

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        ApplyRandomStartingSpeed();
    }
    private void ApplyRandomStartingSpeed()
    {
        float randomDirection = Random.Range(-(randomSpeedCone/2), randomSpeedCone/2);
        float randomSpeed = Random.Range(0, maxRandomSpeedForward);

        if (rb2D == null)
        {
            return;
        }
        rb2D.velocity += (Vector2)HelperMethods.VectorUtils.DirectionVector(randomSpeed, randomDirection + transform.rotation.eulerAngles.z);
    }
    public void IncreaseStartingSpeed(Vector2 deltaVelocity)
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            return;
        }
        if(velocityMode == VelocityMode.ForwardVelocity)
        {
            Vector2 summonDirection = HelperMethods.VectorUtils.DirectionVectorNormalized(transform.rotation.eulerAngles.z);
            rb2D.velocity = summonDirection.normalized * deltaVelocity.magnitude;
        }
        else
        {
            rb2D.velocity += deltaVelocity;
        }
    }

    public bool ShouldModifyVelocity()
    {
        return isOn;
    }
}

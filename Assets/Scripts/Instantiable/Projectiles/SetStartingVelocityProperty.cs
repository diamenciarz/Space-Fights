using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStartingVelocityProperty : MonoBehaviour, IModifiableStartingSpeed
{
    [SerializeField] bool isOn = true;
    
    public void IncreaseStartingSpeed(float deltaSpeed)
    {
        Debug.Log("Modified speed by: " + deltaSpeed);
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        if (rb2D)
        {
            Vector2 summonDirection = HelperMethods.VectorUtils.DirectionVectorNormalized(transform.rotation.eulerAngles.z);
            rb2D.velocity = summonDirection.normalized * deltaSpeed;
        }
    }

    public bool ShouldModifyVelocity()
    {
        return isOn;
    }
}

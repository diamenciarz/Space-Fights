using UnityEngine;
using static EntityInput;

[CreateAssetMenu(fileName = "ConstantForce", menuName = "Moves/ConstantMove")]

public class ConstantMoveAction : MoveAction
{
    [SerializeField] Vector2 force;
    public override void applyAction(ActionData actionData)
    {
        Vector2 forceToApply = GetRelativeForce(actionData);
        actionData.rigidbody2D.AddRelativeForce(forceToApply, ForceMode2D.Force);
        KeepVelocityInBounds(actionData, forceToApply);
    }

    private Vector2 GetRelativeForce(ActionData actionData)
    {
        Vector2 forceInTime = force * Time.fixedDeltaTime;
        float forceMultiplier = GetForceMultiplier(actionData, forceInTime);
        Vector2 clampedForce = forceInTime * forceMultiplier;

        //Debug.Log(" multiplier: " + forceMultiplier);
        return clampedForce / Time.fixedDeltaTime;
    }
    private float GetForceMultiplier(ActionData actionData, Vector2 force)
    {
        Vector2 forceInWorldspace = actionData.rigidbody2D.transform.TransformVector(force);
        Vector2 velocityInForceDirection = HelperMethods.ProjectVector(actionData.rigidbody2D.velocity, forceInWorldspace);
        float dot = Vector2.Dot(actionData.rigidbody2D.velocity, forceInWorldspace);

        bool forceInFlightDirection = dot > 0;
        if (forceInFlightDirection)
        {
            float maxVelocity = actionData.entityMover.GetMaxSpeed();
            float deltaSpeedToMax = maxVelocity - velocityInForceDirection.magnitude;
            return deltaSpeedToMax / maxVelocity;
        }
        return 1;
    }
    private void KeepVelocityInBounds(ActionData actionData, Vector2 appliedForce)
    {
        Vector2 velocity = actionData.rigidbody2D.velocity;
        Vector2 forceInWorldspace = actionData.rigidbody2D.transform.TransformVector(appliedForce);
        Vector2 velocityInForceDirection = HelperMethods.ProjectVector(velocity, forceInWorldspace);
        Vector2 perpendicularVelocity = velocity - velocityInForceDirection;

        float maxSpeed = actionData.entityMover.GetMaxSpeed();
        float allowedPerpendicularSpeed = Mathf.Sqrt(maxSpeed * maxSpeed - velocityInForceDirection.magnitude * velocityInForceDirection.magnitude);
        Vector2 allowedPerpendicularVelocity = Vector2.ClampMagnitude(perpendicularVelocity, allowedPerpendicularSpeed);
        actionData.rigidbody2D.velocity = allowedPerpendicularVelocity + velocityInForceDirection;
    }
}

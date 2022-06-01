using UnityEngine;
using static EntityInput;

[CreateAssetMenu(fileName = "ConstantForce", menuName = "Moves/ConstantMove")]

public class ConstantMoveAction : MoveAction
{
    [SerializeField] Vector2 force;
    [SerializeField] bool isForceGlobal;
    public override void ApplyAction(ActionData actionData)
    {
        Vector2 forceToApply = GetRelativeForce(actionData);
        ApplyForce(actionData, forceToApply);
        KeepVelocityInBounds(actionData, forceToApply);
    }
    private Vector2 GetRelativeForce(ActionData actionData)
    {
        Vector2 forceInTime = force * Time.fixedDeltaTime;
        float forceMultiplier = CalculateForceMultiplier(actionData, forceInTime);
        Vector2 clampedForce = forceInTime * forceMultiplier;

        //Debug.Log(" multiplier: " + forceMultiplier);
        return clampedForce / Time.fixedDeltaTime;
    }
    private float CalculateForceMultiplier(ActionData actionData, Vector2 appliedForce)
    {
        Vector2 forceInWorldspace = CalculateAppliedForce(actionData,appliedForce);
        Vector2 velocityInForceDirection = HelperMethods.VectorUtils.ProjectVector(actionData.rigidbody2D.velocity, forceInWorldspace);
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
    private void ApplyForce(ActionData actionData, Vector2 force)
    {
        if (isForceGlobal)
        {
            actionData.rigidbody2D.AddForce(force, ForceMode2D.Force);
            return;
        }
        actionData.rigidbody2D.AddRelativeForce(force, ForceMode2D.Force);
    }
    private void KeepVelocityInBounds(ActionData actionData, Vector2 appliedForce)
    {
        Vector2 velocity = actionData.rigidbody2D.velocity;
        Vector2 forceInWorldspace = CalculateAppliedForce(actionData, appliedForce);
        Vector2 velocityInForceDirection = HelperMethods.VectorUtils.ProjectVector(velocity, forceInWorldspace);
        Vector2 perpendicularVelocity = velocity - velocityInForceDirection;

        float maxSpeed = actionData.entityMover.GetMaxSpeed();
        float maxPerpendicularSpeed = Mathf.Sqrt(maxSpeed * maxSpeed - velocityInForceDirection.magnitude * velocityInForceDirection.magnitude);
        Vector2 allowedPerpendicularVelocity = Vector2.ClampMagnitude(perpendicularVelocity, maxPerpendicularSpeed);
        actionData.rigidbody2D.velocity = allowedPerpendicularVelocity + velocityInForceDirection;
    }
    private Vector2 CalculateAppliedForce(ActionData actionData, Vector2 appliedForce)
    {
        if (isForceGlobal)
        {
            return appliedForce;
        }
        return actionData.rigidbody2D.transform.TransformVector(appliedForce);
    }
}

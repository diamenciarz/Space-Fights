using UnityEngine;
using static EntityInput;

[CreateAssetMenu(fileName = "ConstantForce", menuName = "Moves/ConstantMove")]

public class ConstantMoveAction : MoveAction
{
    [SerializeField] Vector2 force;
    public override void applyAction(ActionData actionData)
    {
        actionData.rigidbody2D.AddRelativeForce(GetRelativeForce(actionData), ForceMode2D.Force);
    }

    private Vector2 GetRelativeForce(ActionData actionData)
    {
        Vector2 velocityInForceDirection = ProjectVector(actionData.rigidbody2D.velocity, force);
        Vector2 forceInTime = force * Time.fixedDeltaTime;
        float forceMultiplier = GetForceMultiplier(actionData, forceInTime);
        Vector2 clampedForce = forceInTime * forceMultiplier;

        //Debug.Log("Clamped force: " + clampedForce + " multiplier: " + forceMultiplier);
        return clampedForce / Time.fixedDeltaTime;
    }
    private float GetForceMultiplier(ActionData actionData, Vector2 force)
    {
        Vector2 velocityInWorldspace = actionData.rigidbody2D.transform.TransformVector(actionData.rigidbody2D.velocity);
        Vector2 forceInWorldspace = actionData.rigidbody2D.transform.TransformVector(force);
        Vector2 velocityInForceDirection = ProjectVector(actionData.rigidbody2D.velocity, forceInWorldspace);
        float dot = Vector2.Dot(velocityInForceDirection, forceInWorldspace);

        bool forceInFlightDirection = dot > 0;
        if (forceInFlightDirection)
        {
            float maxVelocity = actionData.entityMover.GetMaxVelocity();
            float deltaSpeedToMax = maxVelocity - Mathf.Clamp(velocityInForceDirection.magnitude, 0, maxVelocity);
            return deltaSpeedToMax / maxVelocity;
        }
        return 1;
    }
    private Vector2 ProjectVector(Vector2 project, Vector2 onto)
    {
        float length = Vector2.Dot(project, onto.normalized);
        return onto.normalized* length;
    }
}

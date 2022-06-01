using UnityEngine;

[CreateAssetMenu(fileName = "RotateInMoveDirection", menuName = "Moves/RotateInMoveDirection")]

public class RotateInMoveMentDirectionAction : MoveAction
{
    [Tooltip("Positive values rotate clockwise")]
    [SerializeField] float maxTorque;
    [SerializeField] bool affectedByVelocity;

    public override void ApplyAction(EntityInput.ActionData actionData)
    {
        actionData.entityMover.RotateByAngle(CalculateTorque(actionData), affectedByVelocity);
    }
    private float CalculateTorque(EntityInput.ActionData actionData)
    {
        float rotation = HelperMethods.AngleUtils.ClampAngle360(actionData.rigidbody2D.rotation);
        Vector2 rotationVector = HelperMethods.VectorUtils.DirectionVectorNormalized(rotation);
        Vector2 velocity = actionData.rigidbody2D.velocity;
        float rotation2 = HelperMethods.VectorUtils.VectorDirection(rotationVector);
        float velocityDirection = HelperMethods.VectorUtils.VectorDirection(velocity);

        //float deltaAngle = Vector2.SignedAngle(rotationVector, velocity);
        float deltaAngle = velocityDirection - rotation;
        //Debug.Log("Ship velocity: " + rotation2 + " direction vector: " + rotation + " deltaAngle: " + deltaAngle);
        return 0;
        //return Mathf.Clamp(deltaAngle, -maxTorque, maxTorque);
    }
}

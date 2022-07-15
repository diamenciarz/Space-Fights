using UnityEngine;
using static EntityInput;

[CreateAssetMenu(fileName = "RotateInMoveDirection", menuName = "Moves/RotateInMoveDirection")]

public class RotateInMovementDirectionAction : AbstractMoveAction
{
    [Tooltip("Positive values rotate clockwise")]
    [SerializeField] bool affectedByVelocity;
    /// <summary>
    /// The minimum percentage of max speed that the ship should move at for the action to take effect
    /// </summary>
    [SerializeField] [Range(0, 1)] float maxSpeedPercentage;

    public override void ApplyAction(ActionData actionData)
    {
        if (shipMovesTooSlowly(actionData))
        {
            return;
        }
        actionData.entityMover.RotateTowardsVector(actionData.rigidbody2D.velocity, affectedByVelocity);
    }
    private bool shipMovesTooSlowly(ActionData actionData)
    {
        float maxSpeed = actionData.entityMover.GetMaxSpeed();
        float speed = actionData.rigidbody2D.velocity.magnitude;
        return (speed / maxSpeed) < maxSpeedPercentage;
    }
}

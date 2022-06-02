using UnityEngine;
using static EntityInput;

[CreateAssetMenu(fileName = "RotateInMouseDirection", menuName = "Moves/RotateInMouseDirection")]

public class RotateInMouseDirectionAction : MoveAction
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
        actionData.entityMover.RotateTowardsVector(GetDirectionToMouseCursor(actionData), affectedByVelocity);
    }
    private bool shipMovesTooSlowly(ActionData actionData)
    {
        float maxSpeed = actionData.entityMover.GetMaxSpeed();
        float speed = actionData.rigidbody2D.velocity.magnitude;
        return (speed / maxSpeed) < maxSpeedPercentage;
    }
    private Vector2 GetDirectionToMouseCursor(ActionData actionData)
    {
        Vector2 mousePosition = HelperMethods.VectorUtils.TranslatedMousePosition();
        Vector2 shipPosition = actionData.rigidbody2D.gameObject.transform.position;
        Vector2 directionToMouseCursor = HelperMethods.VectorUtils.DeltaPosition(shipPosition, mousePosition);
        return directionToMouseCursor;
    }
}

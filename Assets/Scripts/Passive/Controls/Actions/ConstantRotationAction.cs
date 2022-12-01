using UnityEngine;

[CreateAssetMenu(fileName = "ConstantRotation", menuName = "Moves/ConstantRotation")]

public class ConstantRotationAction : AbstractMoveAction
{
    [Tooltip("Positive values rotate clockwise")]
    [SerializeField] float torque;
    [SerializeField] bool affectedByVelocity;

    public override void ApplyAction(EntityInput.ActionData actionData)
    {
        actionData.entityMover.RotateByAngle(torque * actionData.percentage, affectedByVelocity);
    }
}

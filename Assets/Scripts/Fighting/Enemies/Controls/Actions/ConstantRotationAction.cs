using UnityEngine;

[CreateAssetMenu(fileName = "ConstantRotation", menuName = "Moves/ConstantRotation")]

public class ConstantRotationAction : MoveAction
{
    [Tooltip("Positive values rotate clockwise")]
    [SerializeField] float torque;

    public override void applyAction(EntityInput.ActionData actionData)
    {
        actionData.entityMover.RotateByAngle(torque);
    }
}

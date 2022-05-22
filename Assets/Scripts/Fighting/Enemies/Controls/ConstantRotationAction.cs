using UnityEngine;

[CreateAssetMenu(fileName = "ConstantRotation", menuName = "Moves/ConstantRotation")]

public class ConstantRotationAction : MoveAction
{
    [Tooltip("Positive values rotate clockwise")]
    [SerializeField] float torque;

    [SerializeField] float maxAngularSpeed;

    public override void applyAction(Rigidbody2D rigidbody2D)
    {
        rigidbody2D.AddTorque(GetRelativeTorque(rigidbody2D), ForceMode2D.Force);
    }

    private float GetRelativeTorque(Rigidbody2D rigidbody2D)
    {
        const int TORQUE_TRANSLATION = 1000;
        float torqueInTime = -torque * Time.fixedDeltaTime * TORQUE_TRANSLATION * rigidbody2D.inertia;
        float velocityAfterTorqueIsApplied = rigidbody2D.angularVelocity + torqueInTime;
        float clampedVelocity = Mathf.Clamp(velocityAfterTorqueIsApplied, -maxAngularSpeed / Time.fixedDeltaTime, maxAngularSpeed / Time.fixedDeltaTime);
        float torqueToApply = (clampedVelocity - rigidbody2D.angularVelocity) / Time.fixedDeltaTime;
        return torqueToApply;
    }
}

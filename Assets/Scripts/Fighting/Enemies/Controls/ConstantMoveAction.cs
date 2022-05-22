using UnityEngine;

[CreateAssetMenu(fileName = "ConstantForce", menuName = "Moves/ConstantMove")]

public class ConstantMoveAction : MoveAction
{
    [SerializeField] Vector2 force;

    [SerializeField] float maxVelocity;

    public override void applyAction(Rigidbody2D rigidbody2D)
    {
        rigidbody2D.AddRelativeForce(GetRelativeForce(rigidbody2D), ForceMode2D.Force);
    }

    private Vector2 GetRelativeForce(Rigidbody2D rigidbody2D)
    {
        const int FORCE_TRANSLATION = 1000;
        Vector2 forceInTime = force * Time.fixedDeltaTime * FORCE_TRANSLATION * rigidbody2D.mass;
        Vector2 velocityAfterForceIsApplied = rigidbody2D.velocity + forceInTime;
        Vector2 clampedVelocity = Vector2.ClampMagnitude(velocityAfterForceIsApplied, maxVelocity);
        Vector2 forceToApply = (clampedVelocity - rigidbody2D.velocity) / Time.fixedDeltaTime;
        return forceToApply;
    }
}

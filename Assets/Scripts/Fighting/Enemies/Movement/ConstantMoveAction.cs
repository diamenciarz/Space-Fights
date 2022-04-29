using UnityEngine;

[CreateAssetMenu(fileName = "ConstantForce", menuName = "Moves/Constant")]

public class ConstantMoveAction : MoveAction
{
    [SerializeField] Vector2 force;

    [SerializeField] float maxSpeed;

    public override void applyAction(Rigidbody2D rigidbody2D)
    {
        rigidbody2D.AddRelativeForce(GetRelativeForce(rigidbody2D), ForceMode2D.Force);
    }

    private Vector2 GetRelativeForce(Rigidbody2D rigidbody2D)
    {
        
        Vector2 basicForce = rigidbody2D.gameObject.transform.right * force * Time.fixedDeltaTime;
        float maxForce = (maxSpeed - GetSpeed(rigidbody2D)) / Time.fixedDeltaTime;
        return Vector2.ClampMagnitude(basicForce, maxForce);
        
    }
    public float GetSpeed(Rigidbody2D rigidbody2D)
    {
        Vector2 velocityVector = GetForwardVelocity(rigidbody2D);
        float speed = velocityVector.magnitude;
        if (Vector2.Dot(rigidbody2D.velocity, rigidbody2D.gameObject.transform.right) > 0)
        {
            return speed;
        }
        else
        {
            return -speed;
        }
    }
    public Vector2 GetForwardVelocity(Rigidbody2D rigidbody2D)
    {
        return rigidbody2D.gameObject.transform.right * Vector2.Dot(rigidbody2D.velocity, rigidbody2D.gameObject.transform.right);
    }
}

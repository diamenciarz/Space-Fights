using UnityEngine;

public interface IPushable
{
    public abstract Rigidbody2D GetRigidbody2D();
    public abstract void Push(Vector2 force);
    public abstract void Push(Vector2 force, Vector2 deltaPosition);
}

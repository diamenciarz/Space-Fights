using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PushableProperty : MonoBehaviour, IPushable
{
    Rigidbody2D rigidbody2D;
    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }
    public Rigidbody2D GetRigidbody2D()
    {
        return rigidbody2D;
    }

    public void Push(Vector2 force)
    {
        Debug.Log("Pushed for: " + force);
        rigidbody2D.AddForce(force, ForceMode2D.Force);
    }
    /// <summary>
    /// Pushed the object at a delta position from the middle of the object's collider
    /// </summary>
    /// <param name="force"></param>
    /// <param name="deltaPosition"></param>
    public void Push(Vector2 force, Vector2 deltaPosition)
    {
        rigidbody2D.AddForceAtPosition(force, deltaPosition, ForceMode2D.Impulse);
    }
}

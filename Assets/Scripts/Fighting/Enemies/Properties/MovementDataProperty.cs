using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementDataProperty : MonoBehaviour
{
    private Rigidbody2D myRigidbody2D;
    private Vector2 lastVelocity;
    private Vector2 acceleration;

    #region Initialization
    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        lastVelocity = myRigidbody2D.velocity;
    }
    #endregion
    // Update is called once per frame
    void FixedUpdate()
    {
        acceleration = (myRigidbody2D.velocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = transform.position;
    }

    #region Accessor methods
    public MovementData GetMovementData()
    {
        MovementData movementData = new MovementData();
        movementData.position = transform.position;
        movementData.velocity = myRigidbody2D.velocity;
        movementData.acceleration = acceleration;
        return movementData;
    }
    #endregion
    public class MovementData
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;

        public Vector2 predictPosition(float delay)
        {
            return position + delay * velocity + (delay * delay / 2 * acceleration);
        }
    }
}

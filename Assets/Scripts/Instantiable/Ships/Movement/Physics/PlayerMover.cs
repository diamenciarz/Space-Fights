using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour, IEntityMover
{
    [SerializeField] float maxSpeed = 5f;

    private Vector2 inputVector;
    private Rigidbody2D rb2D;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb2D.velocity = inputVector * maxSpeed;
    }
    public void SetInputVector(Vector2 newVector)
    {
        inputVector = newVector;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;
using static TeamUpdater;

public abstract class MovementBehaviour : ScriptableObject
{
    public abstract Vector2 CalculateMovementVector(MovementBehaviourData newData);
}

public class MovementBehaviourData
{
    public GameObject gameObject;
    public Vector2 position;
    public Vector2 velocity;
    public Team team;
    public IParent parent;
    public float shipSize;
}
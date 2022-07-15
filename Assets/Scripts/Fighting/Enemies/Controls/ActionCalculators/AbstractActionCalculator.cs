using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractActionCalculator : MonoBehaviour
{
    public abstract ActionCallData[] CalculateActionsToCall(Vector2 globalForce);
}

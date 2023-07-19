using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourBase", menuName = "AI/Behaviours/BehaviourBase")]
public class Behaviour : ScriptableObject
{
    public MovementBehaviour movementBehaviour;
    public Condition condition;
}

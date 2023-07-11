using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateOnDeath : TriggerOnDeath
{
    [SerializeField] GameObject objToInstantiate;

    protected override void DoDestroyAction()
    {
        SummonedGameObjectData data = new SummonedGameObjectData();
        data.gameObject = objToInstantiate;
        data.summonPosition = transform.position;
        data.summonRotation = transform.rotation;
        data.startingVelocity = GetVelocity();
        EntityCreator.SummonGameObject(data);
    }
    private Vector2 GetVelocity()
    {
        Rigidbody2D rb2D = GetComponentInParent<Rigidbody2D>();
        if (rb2D == null)
        {
            return Vector2.zero;
        }
        else
        {
            return rb2D.velocity;
        }
    }
}

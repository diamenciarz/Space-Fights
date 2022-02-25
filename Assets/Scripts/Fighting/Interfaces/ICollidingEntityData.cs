using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollidingEntityData
{
    public abstract GameObject GetCreatedBy();
    public abstract Vector3 GetVelocityVector3();
    public abstract void ModifyVelocityVector3(Vector3 deltaVector);
}

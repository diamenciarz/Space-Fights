using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions for determining the team and object responsible for the creation of this object
/// </summary>
public interface IParent
{
    public abstract int GetTeam();
    public abstract GameObject GetCreatedBy();
    public abstract void SetTeam(int team);
    public abstract void SetCreatedBy(GameObject parent);
}

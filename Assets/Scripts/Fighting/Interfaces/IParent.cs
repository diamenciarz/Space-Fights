using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParent
{
    public abstract int GetTeam();
    public abstract GameObject GetParent();
    public abstract void SetTeam(int team);
    public abstract void SetCreatedBy(GameObject parent);
}

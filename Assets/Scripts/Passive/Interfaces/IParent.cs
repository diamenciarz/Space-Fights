using UnityEngine;
using static TeamUpdater;

/// <summary>
/// Functions for determining the team and object responsible for the creation of this object
/// </summary>
public interface IParent
{
    public abstract GameObject GetGameObject();
    public abstract Team GetTeam();
    public abstract GameObject GetCreatedBy();
    public abstract ObjectType GetObjectType();
    public abstract void SetTeam(Team team);
    public abstract void SetCreatedBy(GameObject parent);
}

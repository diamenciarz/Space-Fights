using System.Collections;
using UnityEngine;

public class TeamUpdater : MonoBehaviour, ITeamable
{
    /// <summary>
    /// If equal to -1 - this object is an enemy to everyone
    /// </summary>
    [HideInInspector]
    public int team = -1;
    protected GameObject createdBy;

    protected virtual void Awake()
    {
        IParent parent = GetComponentInParent<IParent>();
        if (parent == null)
        {
            return;
        }
        UpdateCreatedBy(parent);
        UpdateTeam(parent);
    }
    #region Set parent
    private void UpdateCreatedBy(IParent parent)
    {
        if (createdBy != null)
        {
            return;
        }
        SetCreatedBy(parent.GetCreatedBy());
    }
    #endregion

    #region Accessor methods
    public int GetTeam()
    {
        return team;
    }
    public GameObject GetCreatedBy()
    {
        return createdBy;
    }
    #endregion

    #region Mutator methods
    public void SetCreatedBy(GameObject parent)
    {
        if (parent)
        {
            createdBy = parent;
        }
    }
    public virtual void UpdateTeam(IParent parent)
    {
        team = parent.GetTeam();
    }
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
    #endregion
}
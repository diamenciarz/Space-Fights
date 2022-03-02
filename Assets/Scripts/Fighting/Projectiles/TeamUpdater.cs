using System.Collections;
using UnityEngine;

public class TeamUpdater : MonoBehaviour
{
    [HideInInspector]
    public int team = -1;
    protected GameObject createdBy;

    protected virtual void Awake()
    {
        UpdateCreatedBy();
        UpdateTeam();
    }
    #region Set parent
    private void UpdateCreatedBy()
    {
        IParent parent = GetComponentInParent<IParent>();
        if (parent == null || createdBy != null)
        {
            return;
        }
        SetCreatedBy(parent.GetParent());
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
    public virtual void UpdateTeam()
    {
        IParent damageReceiver = GetComponentInParent<IParent>();
        team = damageReceiver.GetTeam();
    }
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
    #endregion
}
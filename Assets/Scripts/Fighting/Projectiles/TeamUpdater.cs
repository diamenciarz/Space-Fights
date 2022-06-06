using System;
using UnityEngine;

public class TeamUpdater : MonoBehaviour, ITeamable
{
    /// <summary>
    /// If equal to -1 - this object is an enemy to everyone
    /// </summary>
    //[HideInInspector]
    public Team team;
    protected GameObject createdBy;
    public enum TeamInstance
    {
        EnemyToAll,
        Neutral,
        Team1,
        Team2,
        Team3
    }

    protected virtual void Awake()
    {
        IParent parent = GetComponentInParent<IParent>();
        if (parent == null)
        {
            return;
        }
        UpdateCreatedBy(parent);
        //Debug.Log("Parent: " + gameObject.name);
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
    public Team GetTeam()
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
    /// <summary>
    /// This is called by the parent to override the teams of all of its children
    /// </summary>
    /// <param name="parent"></param>
    public virtual void UpdateTeam(IParent parent)
    {
        team = parent.GetTeam();
    }
    /// <summary>
    /// This is called on the parent to change its team and off of its childrens team
    /// </summary>
    /// <param name="newTeam"></param>
    public virtual void SetTeam(Team newTeam)
    {
        team = new Team(newTeam);
    }
    #endregion
    [Serializable]
    public class Team
    {
        public Team(TeamInstance teamInstance)
        {
            this.teamInstance = teamInstance;
        }
        public Team(Team team)
        {
            this.teamInstance = team.teamInstance;
        }
        public TeamInstance teamInstance;

        public static Team GetNeutralTeam()
        {
            return new Team(TeamInstance.Neutral);
        }
        public static Team GetEnemyToAllTeam()
        {
            return new Team(TeamInstance.EnemyToAll);
        }
        public bool IsEnemy(Team otherTeam)
        {
            if (teamInstance == TeamInstance.Neutral || otherTeam.teamInstance == TeamInstance.Neutral)
            {
                return false;
            }
            if (teamInstance == TeamInstance.EnemyToAll || otherTeam.teamInstance == TeamInstance.EnemyToAll)
            {
                return true;
            }
            return teamInstance != otherTeam.teamInstance;
        }
        public bool IsAlly(Team otherTeam)
        {
            if (teamInstance == otherTeam.teamInstance)
            {
                return true;
            }
            return false;
        }
        public bool IsNeutral(Team otherTeam)
        {
            if (teamInstance == TeamInstance.Neutral || otherTeam.teamInstance == TeamInstance.Neutral)
            {
                return true;
            }
            return false;
        }
    }
}
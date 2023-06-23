using System;
using UnityEngine;

public class TeamUpdater : MonoBehaviour, ITeamable
{
    [HideInInspector]
    public Team team;
    protected float creationTime;
    protected GameObject createdBy;

    public enum ObjectType
    {
        PROJECTILE,
        ACTOR,
        OTHER,
        ROCKET
    }
    protected ObjectType objectType = ObjectType.OTHER;

    public enum TeamInstance
    {
        EnemyToAll,
        Neutral,
        Team1,
        Team2,
        Team3
    }

    #region Startup
    protected virtual void Awake()
    {
        IParent parent = GetComponentInParent<IParent>();
        if (parent == null)
        {
            return;
        }
        UpdateCreatedBy(parent);
        UpdateLayer(team);
        ParentUpdatesTeam(parent);
    }

    protected virtual void Start()
    {
        UpdateStartingVariables();
    }

    private void UpdateStartingVariables()
    {
        creationTime = Time.time;
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
    protected void SetObjectType(ObjectType type)
    {
        objectType = type;
    }
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
    public virtual void ParentUpdatesTeam(IParent parent)
    {
        team = parent.GetTeam();
        objectType = parent.GetObjectType();
        UpdateLayer(team);

    }
    /// <summary>
    /// This is called on the parent to change its team and off of its childrens team
    /// </summary>
    /// <param name="newTeam"></param>
    public virtual void SetTeam(Team newTeam)
    {
        team = new Team(newTeam);
        UpdateLayer(newTeam);
    }

    public void UpdateLayer(Team newTeam)
    {
        if (objectType == ObjectType.PROJECTILE)
        {
            SetProjectileLayer(newTeam);
        }
        if (objectType == ObjectType.ACTOR)
        {
            SetActorLayer(newTeam);
        }
        if (objectType == ObjectType.ROCKET)
        {
            SetRocketLayer(newTeam);
        }
    }
    private void SetProjectileLayer(Team newTeam)
    {
        switch (newTeam.teamInstance)
        {
            case TeamInstance.Team1:
                gameObject.layer = 9;
                break;
            case TeamInstance.Team2:
                gameObject.layer = 10;
                break;
            case TeamInstance.Team3:
                gameObject.layer = 11;
                break;
            case TeamInstance.EnemyToAll:
                gameObject.layer = 12;
                break;
            default:
                gameObject.layer = 13;
                break;
        }
    }
    private void SetActorLayer(Team newTeam)
    {
        switch (newTeam.teamInstance)
        {
            case TeamInstance.Team1:
                gameObject.layer = 14;
                break;
            case TeamInstance.Team2:
                gameObject.layer = 15;
                break;
            case TeamInstance.Team3:
                gameObject.layer = 16;
                break;
            case TeamInstance.EnemyToAll:
                gameObject.layer = 17;
                break;
            default:
                gameObject.layer = 18;
                break;
        }
    }
    private void SetRocketLayer(Team newTeam)
    {
        switch (newTeam.teamInstance)
        {
            case TeamInstance.Team1:
                gameObject.layer = 19;
                break;
            case TeamInstance.Team2:
                gameObject.layer = 20;
                break;
            case TeamInstance.Team3:
                gameObject.layer = 21;
                break;
            default:
                gameObject.layer = 22;
                break;
        }
    }
    #endregion
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
            if (teamInstance == TeamInstance.EnemyToAll || otherTeam.teamInstance == TeamInstance.EnemyToAll)
            {
                return false;
            }
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
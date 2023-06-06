using System.Collections.Generic;
using UnityEngine;

public class ClosestTargetFollower : MonoBehaviour
{
    [SerializeField] bool followMouseInstead;
    [SerializeField] StaticDataHolder.ObjectTypes[] targetTypesToFollow;

    private GameObject objectToFollow;
    private GameObject currentTarget;
    private TeamUpdater.Team team;

    #region Initialization
    private void Start()
    {
        FindObjectToFollow();
    }
    private void FindObjectToFollow()
    {
        if (followMouseInstead)
        {
            objectToFollow = StaticDataHolder.ListContents.Generic.GetClosestMouseCursor(transform.position);
        }
    }
    #endregion

    #region Update
    void Update()
    {
        FindTarget();
        FollowTarget();
    }
    private void FindTarget()
    {
        if (objectToFollow != null)
        {
            List<GameObject> potentialTargetsWithAllies = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypesToFollow);
            List<GameObject> potentialTargets = StaticDataHolder.ListModification.SubtractAllies(potentialTargetsWithAllies, team);
            currentTarget = StaticDataHolder.ListContents.Generic.GetClosestObject(potentialTargets, objectToFollow.transform.position);
        }
    }
    private void FollowTarget()
    {
        if (currentTarget != null)
        {
            transform.position = currentTarget.transform.position;
        }
    }
    #endregion
    
    #region Mutator Methods
    public void SetObjectToFollow(GameObject newObj)
    {
        objectToFollow = newObj;
        followMouseInstead = false;
    }
    public void SetFollowMouse(bool set)
    {
        followMouseInstead = set;
        objectToFollow = null;
        FindObjectToFollow();
    }
    public void SetTeam(TeamUpdater.Team newteam)
    {
        team = newteam;
    }
    #endregion
}

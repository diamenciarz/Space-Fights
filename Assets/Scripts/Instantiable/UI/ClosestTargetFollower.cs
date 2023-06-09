using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ClosestTargetFollower : MonoBehaviour
{
    [SerializeField] bool followMouseInstead;

    private StaticDataHolder.ObjectTypes[] targetTypesToFollow;
    private GameObject objectToFollow;
    private GameObject currentTarget;
    private TeamUpdater.Team team;
    private SpriteRenderer spriteRenderer;

    #region Initialization
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
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
            spriteRenderer.enabled = true;
            List<GameObject> potentialTargetsWithAllies = StaticDataHolder.ListContents.Generic.GetObjectList(targetTypesToFollow);
            List<GameObject> potentialTargets = StaticDataHolder.ListModification.SubtractNeutralsAndAllies(potentialTargetsWithAllies, team);
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
    public void SetTargetTypesToFollow(StaticDataHolder.ObjectTypes[] types)
    {
        targetTypesToFollow = types;
    }
    public void SetTeam(TeamUpdater.Team newteam)
    {
        team = newteam;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamUpdater : MonoBehaviour
{
    [HideInInspector]
    public int team = -1;
    protected GameObject createdBy;

    protected virtual void Awake()
    {
        SetupTeam();
        UpdateCreatedBy();
    }
    #region Set parent
    private void UpdateCreatedBy()
    {
        DamageReceiver damageReceiver = GetComponentInParent<DamageReceiver>();
        if (damageReceiver)
        {
            if (createdBy == null)
            {
                SetCreatedBy(damageReceiver.gameObject);
            }
            return;
        }
        BasicProjectileController basicProjectileController = GetComponentInParent<BasicProjectileController>();
        if (basicProjectileController)
        {
            if (createdBy == null)
            {
                SetCreatedBy(basicProjectileController.gameObject);
            }
            return;
        }
    }
    public void SetCreatedBy(GameObject newObject)
    {
        createdBy = newObject;
    }
    #endregion

    #region Set team
    /// <summary>
    /// Change team of the whole gameObject. Use ChangeTeamTo() to change team of this script
    /// </summary>
    /// <param name="newTeam"></param>
    public void SetTeam(int newTeam)
    {
        team = newTeam;
        UpdateTeamInChildren(newTeam);
    }
    private void UpdateTeamInChildren(int newTeam)
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.ChangeTeamTo(newTeam);
        }
        DamageReceiver[] damageReceivers = GetComponentsInChildren<DamageReceiver>();
        foreach (DamageReceiver item in damageReceivers)
        {
            item.ChangeTeamTo(newTeam);
        }
    }
    /// <summary>
    /// Change team of this script. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public void ChangeTeamTo(int newTeam)
    {
        team = newTeam;
    }
    private void SetupTeam()
    {
        DamageReceiver damageReceiver = GetComponentInParent<DamageReceiver>();
        if (damageReceiver)
        {
            team = damageReceiver.GetTeam();
            return;
        }
        BasicProjectileController basicProjectileController = GetComponentInParent<BasicProjectileController>();
        if (basicProjectileController)
        {
            team = basicProjectileController.GetTeam();
            return;
        }
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
}
using UnityEngine;

public abstract class TriggerOnDeath : TeamUpdater
{
    [Tooltip("Leave empty to always activate")]
    [SerializeField] DestroyCause[] activateOn;
    #region OnDestroy
    public enum DestroyCause
    {
        HealthDepleted,
        InstantBreak
    }
    public void CallDestroyAction(DestroyCause cause)
    {
        if (activateOn.Length == 0 || ListContains(cause))
        {
            DoDestroyAction();
        }
    }
    protected abstract void DoDestroyAction();
    private bool ListContains(DestroyCause cause)
    {
        foreach (var reason in activateOn)
        {
            if (reason == cause)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}

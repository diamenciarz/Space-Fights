public abstract class TriggerOnDeath : TeamUpdater
{
    protected override void Awake()
    {
        base.Awake();
    }

    #region OnDestroy
    public abstract void DoDestroyAction();
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticProgressionBarUpdater
{
    private static Dictionary<IProgressionBarCompatible, IProgressionBar> pairs = new Dictionary<IProgressionBarCompatible, IProgressionBar>();
    /// <summary>
    /// This is a list of those Progression Bar holders, which don't actually have a Progression Bar
    /// </summary>
    private static List<IProgressionBarCompatible> bans = new List<IProgressionBarCompatible>();

    #region Update progression bar
    public static void UpdateProgressionBar(IProgressionBarCompatible updater)
    {
        IProgressionBar bar;
        try
        {
            bar = TryGetBar(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.UpdateProgressionBar(updater.GetBarRatio());
    }
    private static IProgressionBar TryGetBar(IProgressionBarCompatible updater)
    {
        bool objectDoesNotHaveABar = bans.Contains(updater);
        if (objectDoesNotHaveABar)
        {
            throw new System.Exception();
        }
        IProgressionBar bar = GetBar(updater);
        bool barNotFound = bar == null;
        if (barNotFound)
        {
            throw new System.Exception();
        }
        return bar;
    }
    private static IProgressionBar GetBar(IProgressionBarCompatible updater)
    {
        IProgressionBar bar;
        pairs.TryGetValue(updater, out bar);
        bool barNotInListYet = bar == null;
        if (barNotInListYet)
        {
            HandleNotFound(bar, updater);
        }
        return bar;
    }
    private static void HandleNotFound(IProgressionBar bar, IProgressionBarCompatible updater)
    {
        GameObject barHolder = updater.GetGameObject();
        bar = barHolder.GetComponent<IProgressionBar>();
        bool barScriptDoesNotExist = bar == null;
        if (barScriptDoesNotExist)
        {
            bans.Add(updater);
            return;
        }
        pairs.Add(updater, bar);
    }
    #endregion

    #region Progression bar existence
    public static void CreateProgressionBar(IProgressionBarCompatible updater)
    {
        IProgressionBar bar;
        try
        {
            bar = TryGetBar(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.CreateProgressionBar(updater.GetGameObject());
    }
    public static void DeleteProgressionBar(IProgressionBarCompatible updater)
    {
        IProgressionBar bar;
        try
        {
            bar = TryGetBar(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.DeleteProgressionBar();
    }
    #endregion
}

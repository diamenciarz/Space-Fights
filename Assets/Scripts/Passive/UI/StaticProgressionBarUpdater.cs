using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HelperMethods.VectorUtils;

public static class StaticProgressionBarUpdater
{
    private static Dictionary<IProgressionBarCompatible, IProgressionBar> barPairs = new Dictionary<IProgressionBarCompatible, IProgressionBar>();
    private static Dictionary<IProgressionBarCompatible, IProgressionCone> conePairs = new Dictionary<IProgressionBarCompatible, IProgressionCone>();
    /// <summary>
    /// This is a list of those Progression Bar holders, which don't actually have a Progression Bar
    /// </summary>
    private static List<IProgressionBarCompatible> bans = new List<IProgressionBarCompatible>();
    public static GameObject UIParent = new GameObject("UIParent");

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
    public static void UpdateProgressionCone(IProgressionBarCompatible updater)
    {
        IProgressionCone bar;
        try
        {
            bar = TryGetCone(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.UpdateProgressionCone(updater.GetBarRatio());
    }
    public static void SetIsProgressionBarAlwaysVisible(IProgressionBarCompatible updater, bool isOn)
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
        bar.SetIsAlwaysOn(isOn);
    }
    public static void SetIsProgressionConeAlwaysVisible(IProgressionBarCompatible updater, bool isOn)
    {
        IProgressionCone bar;
        try
        {
            bar = TryGetCone(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.SetIsAlwaysOn(isOn);
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
    public static void CreateProgressionCone(IProgressionBarCompatible updater, float ratio, float radius)
    {
        IProgressionCone bar;
        try
        {
            bar = TryGetCone(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.CreateProgressionCone(updater.GetTransform(), radius, ratio);
    }
    public static void CreateProgressionCone(IProgressionBarCompatible updater, float radius, float ratio, float deltaRotationFromParent)
    {
        IProgressionCone bar;
        try
        {
            bar = TryGetCone(updater);
        }
        catch (System.Exception)
        {
            return;
        }
        bar.CreateProgressionCone(updater.GetTransform(), radius, ratio, deltaRotationFromParent);
    }
    public static void DeleteProgressionBar(IProgressionBarCompatible updater)
    {
        IProgressionBar bar;
        barPairs.TryGetValue(updater, out bar);
        if (bar == null)
        {
            return;
        }
        bar.DeleteProgressionBar();
    }
    public static void DeleteProgressionCone(IProgressionBarCompatible updater)
    {
        IProgressionCone cone;
        conePairs.TryGetValue(updater, out cone);
        if (cone == null)
        {
            return;
        }
        cone.DeleteProgressionCone();
    }
    #endregion

    #region Helper methods
    private static IProgressionBar TryGetBar(IProgressionBarCompatible updater)
    {
        bool objectDoesNotHaveABar = bans.Contains(updater);
        if (objectDoesNotHaveABar)
        {
            Debug.LogError("This object does not have a progression bar!");
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
        barPairs.TryGetValue(updater, out bar);
        bool barNotInListYet = bar == null;
        if (barNotInListYet)
        {
            HandleBarNotFound(out bar, updater);
        }
        return bar;
    }
    private static void HandleBarNotFound(out IProgressionBar bar, IProgressionBarCompatible updater)
    {
        GameObject barHolder = updater.GetGameObject();
        bar = barHolder.GetComponent<IProgressionBar>();
        bool barScriptDoesNotExist = bar == null;
        if (barScriptDoesNotExist)
        {
            bans.Add(updater);
            return;
        }
        barPairs.Add(updater, bar);
    }
    private static IProgressionCone TryGetCone(IProgressionBarCompatible updater)
    {
        bool objectDoesNotHaveABar = bans.Contains(updater);
        if (objectDoesNotHaveABar)
        {
            throw new System.Exception();
        }
        IProgressionCone cone = GetCone(updater);
        bool barNotFound = cone == null;
        if (barNotFound)
        {
            throw new System.Exception();
        }
        return cone;
    }
    private static IProgressionCone GetCone(IProgressionBarCompatible updater)
    {
        IProgressionCone cone;
        conePairs.TryGetValue(updater, out cone);
        bool barNotInListYet = cone == null;
        if (barNotInListYet)
        {
            HandleConeNotFound(out cone, updater);
        }
        return cone;
    }
    private static void HandleConeNotFound(out IProgressionCone cone, IProgressionBarCompatible updater)
    {
        GameObject coneHolder = updater.GetGameObject();
        cone = coneHolder.GetComponent<IProgressionCone>();
        bool barScriptDoesNotExist = cone == null;
        if (barScriptDoesNotExist)
        {
            bans.Add(updater);
            return;
        }
        conePairs.Add(updater, cone);
    }
    #endregion
}

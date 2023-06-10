using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RayScriptableObject : ScriptableObject
{
    public float additionalReloadTime;
    [Tooltip("True - the gun waits the full time to reload all ammo at once. False - the ammo reolads gradually")]
    public bool reloadAllAtOnce = true;
    public AnimationCurve forceOverTime;


    public float GetTotalRayDuration()
    {
        return forceOverTime.keys[forceOverTime.keys.Length - 1].time;
    }
    public float GetReloadTime() { return 0; }
    /// <summary>
    /// Sums the time for the amount of shots. Starts counting from the last index. Amount starts from 0.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    /*
    public float GetSalvoTimeSum(int amount)
    {
        amount = ClampInputIndex(amount);
        float timeSum = 0;

        for (int i = 0; i < amount; i++)
        {
            timeSum += shots[i].reloadDelay;
        }
        return timeSum;
    }
    private int ClampInputIndex(int index)
    {
        int shotAmount = shots.Length;
        if (index < 0)
        {
            index = 0;
        }
        else
        if (index >= shotAmount)
        {
            index = shotAmount - 1;
        }
        return index;
    }
    */
}
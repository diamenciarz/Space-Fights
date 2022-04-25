using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Single Shot", menuName = "Shots/Salvo")]
public class SalvoScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    public float additionalReloadTime;
    [Tooltip("True - the gun waits the full time to reload all ammo at once. False - the ammo reolads gradually")]
    public bool reloadAllAtOnce = true;

    public SingleShotScriptableObject[] shots;
    public List<float> delayAfterEachShot;
    public List<float> reloadDelays;

    #region Serialization
    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        controlShotDelayLength();
        controlReloadDelayLength();
    }
    private void controlShotDelayLength()
    {
        if (delayAfterEachShot.Count < shots.Length)
        {
            delayAfterEachShot.Add(0);
        }
        if (delayAfterEachShot.Count > shots.Length)
        {
            delayAfterEachShot.RemoveAt(delayAfterEachShot.Count - 1);
        }
    }
    private void controlReloadDelayLength()
    {
        if (reloadDelays.Count < shots.Length)
        {
            reloadDelays.Add(0);
        }
        if (reloadDelays.Count > shots.Length)
        {
            reloadDelays.RemoveAt(reloadDelays.Count - 1);
        }
    }
}
    #endregion

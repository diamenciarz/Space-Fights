using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionBarProperty : MonoBehaviour, IProgressionBar
{
    [SerializeField] EntityCreator.ProgressionBars bar;
    [SerializeField] Vector2 barDeltaPosition;
    [SerializeField] bool isAlwaysOn;
    [Tooltip("Time, after which the bar will disappear, after being shown")]
    [SerializeField] [Range(0, 100)] private float hideDelay = 1;

    public GameObject progressionBar;
    public ProgressionBarController barScript;


    public void CreateProgressionBar(GameObject parent)
    {
        GameObject barToInstantiate = EntityFactory.GetPrefab(bar);
        progressionBar = Instantiate(barToInstantiate, parent.transform.position, parent.transform.rotation);
        progressionBar.transform.SetParent(StaticProgressionBarUpdater.UIParent.transform, true);
        SetBarScriptValues();
    }
    #region Helper methods
    private void SetBarScriptValues()
    {
        barScript = progressionBar.GetComponent<ProgressionBarController>();
        barScript.SetDeltaPositionToObject(barDeltaPosition);
        barScript.SetHideDelay(hideDelay);
        barScript.SetIsAlwaysVisible(isAlwaysOn);
        barScript.SetObjectToFollow(gameObject);
    }
    #endregion

    public void DeleteProgressionBar()
    {
        Destroy(progressionBar);
        barScript = null;
    }
    public void UpdateProgressionBar(float ratio)
    {
        if (barScript)
        {
            barScript.UpdateProgressionBar(ratio);
        }
    }
    public void SetIsAlwaysOn(bool isOn)
    {
        if (barScript)
        {
            barScript.SetIsAlwaysVisible(isOn);
        }
    }
}

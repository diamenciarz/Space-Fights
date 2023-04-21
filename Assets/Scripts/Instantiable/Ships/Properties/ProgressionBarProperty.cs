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

    private GameObject progressionBar;
    private ProgressionBarController barScript;


    public void CreateProgressionBar(GameObject parent)
    {
        GameObject barToInstantiate = EntityFactory.GetPrefab(bar);
        progressionBar = Instantiate(barToInstantiate, parent.transform.position, parent.transform.rotation);
        SummonedProgressionBarData data = CreateSummonData(parent);

        progressionBar = EntityCreator.SummonProgressionBar(data);
        barScript = progressionBar.GetComponent<ProgressionBarController>();
    }
    private SummonedProgressionBarData CreateSummonData(GameObject parent)
    {
        SummonedProgressionBarData data = new SummonedProgressionBarData();
        data.progressionBarType = bar;
        data.barDeltaPosition = barDeltaPosition;
        data.summonPosition = parent.transform.position;
        data.summonRotation = parent.transform.rotation;
        data.hideDelay = hideDelay;
        data.isAlwaysOn = isAlwaysOn;
        data.objectToFollow = gameObject;

        return data;
    }

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

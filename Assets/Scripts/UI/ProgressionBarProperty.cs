using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionBarProperty : MonoBehaviour, IProgressionBar
{
    /// <summary>
    /// Add progression bar factory
    /// </summary>
    [SerializeField] GameObject progressionBarPrefab;
    [SerializeField] Vector2 barDeltaPosition;
    [SerializeField] bool isAlwaysOn;
    [Tooltip("Time, after which the bar will disappear, after being shown")]
    [SerializeField] [Range(0, 100)] private float hideDelay;

    private GameObject progressionBar;
    private ProgressionBarController barScript;

    private void Start()
    {
        CreateProgressionBar(gameObject);
    }

    public void CreateProgressionBar(GameObject parent)
    {
        if (progressionBarPrefab == null)
        {
            return;
        }
        progressionBar = Instantiate(progressionBarPrefab, parent.transform.position, parent.transform.rotation);
        barScript = progressionBar.GetComponent<ProgressionBarController>();
        barScript.SetDeltaPositionToObject(barDeltaPosition);
        barScript.SetHideDelay(hideDelay);
        barScript.SetIsAlwaysVisible(isAlwaysOn);
        barScript.SetObjectToFollow(gameObject);
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
}
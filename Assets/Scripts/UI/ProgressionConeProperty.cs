using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionConeProperty : MonoBehaviour, IProgressionCone
{
    [SerializeField] EntityCreator.ProgressionCones cone;
    [SerializeField] Vector2 coneDeltaPosition;
    [SerializeField] bool isAlwaysOn;
    [SerializeField] float offsetAngle;
    [Tooltip("Time, after which the bar will disappear, after being shown")]
    [SerializeField] [Range(0, 100)] private float hideDelay = 1;

    private GameObject progressionCone;
    private ProgressionBarController barScript;


    public void CreateProgressionCone(Transform parentTransform, float radius, float ratio, float deltaRotationFromParent)
    {
        CreateProgressionCone(parentTransform, radius, ratio);
        SetDeltaRotation(deltaRotationFromParent + offsetAngle);
    }
    public void CreateProgressionCone(Transform parentTransform, float radius, float ratio)
    {
        CreateProgressionCone(parentTransform, ratio);
        SetBarSize(radius);
    }
    public void UpdateProgressionCone(float ratio)
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
    public void DeleteProgressionCone()
    {
        Destroy(progressionCone);
        barScript = null;
    }

    #region Helper methods
    private void CreateProgressionCone(Transform parentTransform, float ratio)
    {
        GameObject barToInstantiate = EntityFactory.GetPrefab(cone);
        progressionCone = Instantiate(barToInstantiate, parentTransform.position, parentTransform.rotation);
        progressionCone.transform.SetParent(StaticProgressionBarUpdater.UIParent.transform, true);
        SetBarScriptValues();
        UpdateProgressionCone(ratio);
    }
    private void SetBarScriptValues()
    {
        barScript = progressionCone.GetComponent<ProgressionBarController>();
        barScript.SetDeltaPositionToObject(coneDeltaPosition);
        barScript.SetHideDelay(hideDelay);
        barScript.SetIsAlwaysVisible(isAlwaysOn);
        barScript.SetObjectToFollow(gameObject);
    }
    private void SetDeltaRotation(float deltaRotationFromParent)
    {
        barScript.SetRotateSameAsParent(true);
        barScript.SetDeltaRotationToObject(deltaRotationFromParent);
    }
    private void SetBarSize(float radius)
    {
        float xScale = radius / progressionCone.transform.lossyScale.x;
        float yScale = radius / progressionCone.transform.lossyScale.y;
        progressionCone.transform.localScale = new Vector3(xScale, yScale, 1);
    }
    #endregion
}

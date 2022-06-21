using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgressionCone
{
    public abstract void UpdateProgressionCone(float ratio);
    public abstract void CreateProgressionCone(GameObject parent, float radius);
    public abstract void CreateProgressionCone(GameObject parent, float radius, float deltaRotationFromParent);
    public abstract void DeleteProgressionCone();
    public abstract void SetIsAlwaysOn(bool isOn);
}

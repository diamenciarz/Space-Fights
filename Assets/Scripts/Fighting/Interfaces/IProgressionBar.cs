using System.Collections;
using UnityEngine;

public interface IProgressionBar
{
    public abstract void UpdateProgressionBar(float ratio);
    public abstract void CreateProgressionBar(GameObject parent);
    public abstract void DeleteProgressionBar();

}

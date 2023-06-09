using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgressionBarCompatible 
{
    public abstract float GetBarRatio();
    public abstract GameObject GetGameObject();
    public abstract Transform GetTransform();
}

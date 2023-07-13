using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomSize : MonoBehaviour, ISerializationCallbackReceiver, ISizeScaled
{
    [SerializeField] float minSizePercentage;
    [SerializeField] float maxSizePercentage;

    [SerializeField] bool changeMassAccordingly;

    private float originalSizePercentage;

    #region Startup
    void Start()
    {
        ChangeSize();
    }
    private void ChangeSize()
    {
        originalSizePercentage = Random.Range(minSizePercentage, maxSizePercentage) / 100;
        transform.localScale = new Vector3(originalSizePercentage * transform.localScale.x, originalSizePercentage * transform.localScale.y, originalSizePercentage * transform.localScale.z);
        if (changeMassAccordingly)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.mass *= originalSizePercentage;
            }
        }
    }
    #endregion
    
    #region Serialization
    public void OnBeforeSerialize()
    {
        FixRange();
    }

    public void OnAfterDeserialize()
    {

    }
    private void FixRange()
    {
        if (maxSizePercentage < minSizePercentage)
        {
            maxSizePercentage = maxSizePercentage + 1;
        }
        if (maxSizePercentage < minSizePercentage)
        {
            maxSizePercentage = maxSizePercentage + 1;
        }
    }
    #endregion
    public float GetOriginalSizePercentage()
    {
        return originalSizePercentage;
    }
}

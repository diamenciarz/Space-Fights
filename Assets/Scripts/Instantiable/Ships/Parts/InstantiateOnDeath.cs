using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateOnDeath : TriggerOnDeath, ISerializationCallbackReceiver
{
    [SerializeField] List<GameObject> objectsToInstantiate;
    [Tooltip("Each value is the chance for this object to be chosen")]
    [SerializeField][Range(0, 100)] List<int> objProbabilities;

    protected override void DoDestroyAction()
    {
        if (objectsToInstantiate == null || objectsToInstantiate.Count == 0)
        {
            return;
        }
        SummonedGameObjectData data = new SummonedGameObjectData();
        data.gameObject = GetRandomObject();
        data.summonPosition = transform.position;
        data.summonRotation = transform.rotation;
        data.startingVelocity = GetVelocity();
        data.originalSizeFraction = GetSize();
        EntityCreator.SummonGameObject(data);
    }
    private GameObject GetRandomObject()
    {
        int spriteIndex = HelperMethods.ListUtils.GetWeightedIndex(objProbabilities);
        return objectsToInstantiate[spriteIndex];
    }
    private Vector2 GetVelocity()
    {
        Rigidbody2D rb2D = GetComponentInParent<Rigidbody2D>();
        if (rb2D == null)
        {
            return Vector2.zero;
        }
        else
        {
            return rb2D.velocity;
        }
    }
    private float GetSize()
    {
        ISizeScaled size = GetComponent<ISizeScaled>();
        if (size != null)
        {
            return size.GetOriginalSizePercentage();
        }
        else
        {
            return 1;
        }
    }
    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public virtual void OnBeforeSerialize()
    {
        controlSpriteProbabilitiesLength();
    }

    private void controlSpriteProbabilitiesLength()
    {
        if (objProbabilities.Count < objectsToInstantiate.Count)
        {
            objProbabilities.Add(0);
        }
        if (objProbabilities.Count > objectsToInstantiate.Count)
        {
            objProbabilities.RemoveAt(objProbabilities.Count - 1);
        }
    }
    #endregion
}

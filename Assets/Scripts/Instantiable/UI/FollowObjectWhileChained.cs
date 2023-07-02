using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectWhileChained : MonoBehaviour
{
    [SerializeField] GameObject objectToFollow;
    [SerializeField] GameObject chainTo;
    [SerializeField] float chainLength;
    void Update()
    {
        FollowObject();
    }
    public void SetObjectToFollow(GameObject newObj)
    {
        objectToFollow = newObj;
        FollowObject();
    }
    public void ChainTo(GameObject newObj, float newChainLength)
    {
        chainTo = newObj;
        chainLength = newChainLength;
    }
    private void FollowObject()
    {
        if (objectToFollow == null)
        {
            return;
        }
        if (chainTo == null)
        {
            transform.position = objectToFollow.transform.position + new Vector3(0, 0, 1);
            return;
        }
        Vector2 posFromChainToFollowerObj = HelperMethods.VectorUtils.DeltaPosition(chainTo, objectToFollow);
        Vector2 chainDeltaPosition = posFromChainToFollowerObj.normalized * Mathf.Min(posFromChainToFollowerObj.magnitude, chainLength);
        transform.position = chainTo.transform.position + new Vector3(chainDeltaPosition.x, chainDeltaPosition.y, 1);
    }
}

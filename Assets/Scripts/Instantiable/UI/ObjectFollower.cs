using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    [SerializeField] GameObject objectToFollow;
    private void Start()
    {
        FollowObject();
    }
    void Update()
    {
        FollowObject();
    }
    public void SetObjectToFollow(GameObject newObj)
    {
        objectToFollow = newObj;
        FollowObject();
    }
    private void FollowObject()
    {
        if (objectToFollow == null)
        {
            return;
        }
        transform.position = objectToFollow.transform.position + new Vector3(0, 0, 1);
    }
}

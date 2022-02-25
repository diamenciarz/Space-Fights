using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionController : MonoBehaviour
{
    [SerializeField] [Range(-10, 10)] float deltaX;
    [SerializeField] [Range(-10, 10)] float deltaY;
    [SerializeField] List<GameObject> follow;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Camera virtualCamera;
    [SerializeField] bool limitView;

    private float xLimit;
    private float yLimit;

    private void Start()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        UpdateLimits();
    }
    private void UpdateLimits()
    {
        Sprite sprite = background.sprite;
        float pixelsPerUnit = background.sprite.pixelsPerUnit;
        float width = sprite.texture.width;
        float height = sprite.texture.height;

        float camSize = virtualCamera.orthographicSize;

        xLimit = (width / pixelsPerUnit) / 2 - camSize;
        yLimit = (height / pixelsPerUnit) / 2 - camSize;
    }
    void Update()
    {
        Move();
        Zoom();
    }

    #region Movement
    private void Move()
    {
        if (follow.Count > 0)
        {
            transform.position = CountPos();
        }
        else
        {
            Debug.Log("All followed objects have been destroyed");
        }
    }
    private Vector2 CountPos()
    {
        if (limitView)
        {
            return CountClampedPosition();
        }
        else
        {
            return CountMiddlePoint();
        }
    }
    private Vector2 CountMiddlePoint()
    {
        Vector2 averagedVector = GetSummedPos() / follow.Count;
        return averagedVector;
    }
    private Vector2 CountClampedPosition()
    {
        float xClamp = xLimit + deltaX;
        float yClamp = yLimit + deltaY;
        Vector2 followPos = CountMiddlePoint();

        Vector2 clampedPos = new Vector2(Mathf.Clamp(followPos.x, -xClamp, xClamp), Mathf.Clamp(followPos.y, -yClamp, yClamp));
        return clampedPos;
    }
    private Vector2 GetSummedPos()
    {
        Vector2 summedPos = Vector2.zero;
        foreach (GameObject obj in follow)
        {
            if (obj == null)
            {
                follow.Remove(obj);
                continue;
            }
            summedPos += (Vector2)obj.transform.position;
        }
        return summedPos;
    }
    #endregion

    #region Zoom
    private void Zoom()
    {

    }
    #endregion
}

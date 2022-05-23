using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionController : MonoBehaviour
{
    [SerializeField] [Range(-10, 10)] float deltaX;
    [SerializeField] [Range(-10, 10)] float deltaY;
    [SerializeField] List<GameObject> follow;
    [SerializeField] SpriteRenderer background;
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

        Vector2 cameraSize = CameraInformation.GetCameraSize();

        //Size in game units
        float backgroundWidth = width / pixelsPerUnit * background.transform.lossyScale.x;
        float backgroundHeight = height / pixelsPerUnit * background.transform.lossyScale.y;

        xLimit = backgroundWidth - cameraSize.x;
        yLimit = backgroundHeight - cameraSize.y;

        if (xLimit < 0)
        {
            xLimit = 0;
        }
        if (yLimit < 0)
        {
            yLimit = 0;
        }
    }
    void Update()
    {
        Move();
        Zoom();
    }

    #region Movement
    private void Move()
    {
        transform.position = CountPos();
    }
    private Vector2 CountPos()
    {
        if (follow.Count == 0)
        {
            return transform.position;
        }
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
        return GetSummedPos() / follow.Count;
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
        for (int i = follow.Count - 1; i >= 0; i--)
        {
            GameObject obj = follow[i];
            if (obj == null)
            {
                follow.RemoveAt(i);
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

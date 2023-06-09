using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionController : MonoBehaviour
{
    [SerializeField][Range(-10, 10)] float deltaX;
    [SerializeField][Range(-10, 10)] float deltaY;

    [SerializeField]
    [Tooltip("The sprite's dimensions will be used to make sure the camera does not fly outside the sprite")]
    SpriteRenderer backgroundSprite;

    [SerializeField]
    [Tooltip("If set to true, the camera will only fly as far from the (0,0) point in game units so that it does not see outside the background (unmovable)")]
    bool limitView;

    //The maximum distance in game units that the camera can fly
    private float xLimit;
    private float yLimit;
    private List<GameObject> follow;

    private void Start()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        UpdateLimits();
        follow = StaticCameraController.objectsToObserve;
    }
    private void UpdateLimits()
    {
        Sprite sprite = backgroundSprite.sprite;
        float pixelsPerUnit = backgroundSprite.sprite.pixelsPerUnit;
        float width = sprite.texture.width;
        float height = sprite.texture.height;

        Vector2 cameraSize = CameraInformation.GetCameraSize();

        //Size in game units
        float backgroundWidthInCameraUnits = width / pixelsPerUnit * backgroundSprite.transform.lossyScale.x;
        float backgroundHeightInCameraUnits = height / pixelsPerUnit * backgroundSprite.transform.lossyScale.y;

        xLimit = backgroundWidthInCameraUnits - cameraSize.x;
        yLimit = backgroundHeightInCameraUnits - cameraSize.y;

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
        RemoveNullElements();
        if (follow.Count == 0)
        {
            return transform.position;
        }
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
        RemoveNullElements();
        for (int i = 0; i < follow.Count; i++)
        {
            GameObject obj = follow[i];
            summedPos += (Vector2)obj.transform.position;
        }
        return summedPos;
    }
    private void RemoveNullElements()
    {
        for (int i = follow.Count - 1; i >= 0; i--)
        {
            GameObject obj = follow[i];
            if (obj == null)
            {
                follow.RemoveAt(i);
                continue;
            }
        }
    }
    #endregion

    #region Zoom
    private void Zoom()
    {

    }
    #endregion
}

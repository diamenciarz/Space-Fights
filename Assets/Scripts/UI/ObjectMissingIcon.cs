using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMissingIcon : MonoBehaviour
{
    public GameObject objectToFollow;

    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;

    [Tooltip("Delta position from the screen edge, where the object should be placed")]
    private float positionOffset = 0.3f;
    [Tooltip("Fraction of the full size")]
    [SerializeField] [Range(0.1f, 1)] float minimumSpriteSize = 0.4f;
    [SerializeField]
    [Tooltip("If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear")]
    float scaleFactor = 6f;

    [Tooltip("The full size of the sprite. This will be used in the transform of the game object")]
    [SerializeField] float spriteScale = 1;
    [Tooltip("Delta position from the screen edge to regard the icon as outside the screen space")]
    private float screenEdgeOffset = 0.5f;
    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;


    //Private variables
    Camera mainCamera;
    SpriteRenderer[] mySpriteRenderers;

    void Start()
    {
        SetupStartingVariables();
        RecountScreenEdges();
    }

    #region Initialization
    private void SetupStartingVariables()
    {
        mySpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        mainCamera = Camera.main;
    }
    public void TryFollowThisObject(GameObject followThis)
    {
        if (followThis != null)
        {
            objectToFollow = followThis;
            UpdateSpriteColor(followThis);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateSpriteColor(GameObject objectToFollow)
    {
        DamageReceiver damageReceiver;
        damageReceiver = objectToFollow.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            if (damageReceiver.GetTeam() == 1)
            {
                GetComponent<SpriteRenderer>().color = allyColor;
                return;
            }
            if (damageReceiver.GetTeam() == 2)
            {
                GetComponent<SpriteRenderer>().color = enemyColor;
                return;
            }
        }

        BasicProjectileController basicProjectileController;
        basicProjectileController = objectToFollow.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            if (basicProjectileController.GetTeam() == 1)
            {
                GetComponent<SpriteRenderer>().color = allyColor;
                return;
            }
            if (basicProjectileController.GetTeam() == 2)
            {
                GetComponent<SpriteRenderer>().color = enemyColor;
                return;
            }
        }
    }
    #endregion

    #region Update
    void Update()
    {
        if (objectToFollow != null)
        {
            RecountScreenEdges(); // this is called by every icon instance, even thouth we could only call it once per frame. Maybe that could be dont later?
            UpdateVisibility();
            if (IsVisible())
            {
                UpdateTransform();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateTransform()
    {
        UpdateRotation();
        UpdatePosition();
        UpdateScale();
    }
    private void UpdateRotation()
    {
        transform.rotation = objectToFollow.transform.rotation;
    }
    private void UpdatePosition()
    {
        transform.position = CountNewPosition();
    }
    private Vector2 CountNewPosition()
    {
        float newXPosition = objectToFollow.transform.position.x;
        float newYPosition = objectToFollow.transform.position.y;

        newXPosition = Mathf.Clamp(newXPosition, xMin - positionOffset, xMax + positionOffset);
        newYPosition = Mathf.Clamp(newYPosition, yMin - positionOffset, yMax + positionOffset);
        return new Vector2(newXPosition, newYPosition);
    }
    private void UpdateScale()
    {
        float distanceToObject = Mathf.Abs((transform.position - objectToFollow.transform.position).magnitude);
        //Sets the new scale ilamped in between <0,4;1>
        float newScale = (1 - (Mathf.Clamp((distanceToObject / scaleFactor), 0, 1f) * (1 - minimumSpriteSize))) * spriteScale;
        Vector3 newScaleVector3 = new Vector3(newScale, newScale, 0);

        transform.localScale = newScaleVector3;
    }
    private void UpdateVisibility()
    {
        bool isFollowedObjectOutsideCameraView = objectToFollow.transform.position.x > xMax + positionOffset
                || objectToFollow.transform.position.x < xMin - positionOffset
                || objectToFollow.transform.position.y > yMax + positionOffset
                || objectToFollow.transform.position.y < yMin - positionOffset;
        if (isFollowedObjectOutsideCameraView)
        {
            //If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear
            float distanceToFollowedObject = (transform.position - objectToFollow.transform.position).magnitude;
            bool isObjectTooFar = (distanceToFollowedObject / scaleFactor > 1f);
            if (isObjectTooFar)
            {
                SetVisibility(false);
            }
            else
            {
                SetVisibility(true);
            }
        }
        else
        {
            SetVisibility(false);
        }
    }
    #endregion

    private void SetVisibility(bool isVisible)
    {
        foreach (SpriteRenderer sprite in mySpriteRenderers)
        {
            sprite.enabled = isVisible;
        }
    }
    private bool IsVisible()
    {
        return mySpriteRenderers[0].enabled;
    }
    /// <summary>
    /// Update the screen edges to 
    /// </summary>
    private void RecountScreenEdges()
    {
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + screenEdgeOffset;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - screenEdgeOffset;
        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + screenEdgeOffset;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - screenEdgeOffset;
    }

    #region Accessor/mutator methods

    #endregion

}

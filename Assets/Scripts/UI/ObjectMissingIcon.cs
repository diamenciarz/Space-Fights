using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMissingIcon : MonoBehaviour
{
    public GameObject objectToFollow;

    [Tooltip("Fraction of the full size")]
    [SerializeField] [Range(0.1f, 1)] float minimumSpriteSize = 0.4f;
    [SerializeField]
    [Tooltip("If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear")]
    float scaleFactor = 6f;

    [Tooltip("The full size of the sprite. This will be used in the transform of the game object")]
    [SerializeField] float spriteScale = 1;
    [Tooltip("Delta position from the screen edge to regard the icon as outside the screen space")]
    [SerializeField] float screenEdgeOffset = 0.5f;
    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;


    //Private variables
    Camera mainCamera;
    SpriteRenderer[] mySpriteRenderers;
    private bool isVisible;

    void Start()
    {
        SetupStartingVariables();
    }

    #region Initialization
    private void SetupStartingVariables()
    {
        mySpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        mainCamera = Camera.main;
    }
    public void TryFollowThisObject(GameObject followThis)
    {
        if (followThis == null)
        {
            Destroy(gameObject);
        }

        objectToFollow = followThis;
        UpdateSpriteColor(followThis);
    }
    private void UpdateSpriteColor(GameObject objectToFollow)
    {
        ITeamable teamable = objectToFollow.GetComponent<ITeamable>();
        if (teamable == null)
        {
            return;
        }
        if (teamable.GetTeam() == 1)
        {
            GetComponent<SpriteRenderer>().color = allyColor;
            return;
        }
        if (teamable.GetTeam() == 2)
        {
            GetComponent<SpriteRenderer>().color = enemyColor;
            return;
        }
    }
    #endregion

    #region Update
    void Update()
    {
        CheckDestroy();

        UpdateVisibility();
        UpdateTransform();
    }
    private void CheckDestroy()
    {
        if (objectToFollow == null)
        {
            Destroy(gameObject);
        }
    }

    #region Transform
    private void UpdateTransform()
    {
        if (isVisible)
        {
            UpdateRotation();
            UpdatePosition();
            UpdateScale();
        }
    }
    private void UpdateRotation()
    {
        transform.rotation = objectToFollow.transform.rotation;
    }
    private void UpdatePosition()
    {
        transform.position = CameraInformation.ClampPositionOnScreen(objectToFollow.transform.position, screenEdgeOffset);
    }
    private void UpdateScale()
    {
        float distanceToObject = Mathf.Abs((transform.position - objectToFollow.transform.position).magnitude);
        //Sets the new scale ilamped in between <0,4;1>
        float newScale = (1 - (Mathf.Clamp((distanceToObject / scaleFactor), 0, 1f) * (1 - minimumSpriteSize))) * spriteScale;
        Vector3 newScaleVector3 = new Vector3(newScale, newScale, 0);

        transform.localScale = newScaleVector3;
    }
    #endregion

    #region Visibility
    private void UpdateVisibility()
    {
        if (!CameraInformation.IsPositionOnScreen(objectToFollow.transform.position, screenEdgeOffset))
        {
            SetSpriteVisibility(false);
        }
        //If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear
        float distanceToFollowedObject = (transform.position - objectToFollow.transform.position).magnitude;
        bool objectIsTooFar = (distanceToFollowedObject / scaleFactor > 1f);
        if (objectIsTooFar)
        {
            SetSpriteVisibility(false);
        }
        else
        {
            SetSpriteVisibility(true);
        }
    }

    private void SetSpriteVisibility(bool setVisibility)
    {
        if (isVisible == setVisibility)
        {
            return;
        }

        isVisible = setVisibility;
        foreach (SpriteRenderer sprite in mySpriteRenderers)
        {
            sprite.enabled = isVisible;
        }
    }
    #endregion
    #endregion
}

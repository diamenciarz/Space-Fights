using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionBarController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] Image healthBarImage;
    [SerializeField] Image[] imagesToHide;
    [SerializeField] GameObject objectToFollow;

    [Header("Display Settings")]
    [SerializeField] bool useGradient = true;
    [SerializeField] Gradient barColorGradient;
    [SerializeField] [Range(0, 1)] float originalAlfa = 1f;
    [Header("Transform Settings")]
    [SerializeField] bool rotateSameAsParent;


    private Vector3 deltaPositionToObject;
    private Quaternion deltaRotationFromParent;
    /// <summary>
    /// The current state of the bar. For internal use only
    /// </summary>
    private bool isShown;
    /// <summary>
    /// If true, the bar is always visible. If false, the bar will hide after a delay
    /// </summary>
    private bool isAlwaysVisible;
    private double lastUsedTime;
    private float visibilityChangeRate;
    private bool isDestroyed;
    /// <summary>
    /// After not receiving damage for this amount of time, the health bar will disappear
    /// </summary>
    private float hideDelay;
    private const float HIDE_OVER_TIME = 0.5f;

    #region Initialization
    void Start()
    {
        SetStartingVariables();
    }
    private void SetStartingVariables()
    {
        transform.rotation = Quaternion.Euler(0, 1, 0);
        //originalAlfa = healthBarImage.color.a;
        visibilityChangeRate = originalAlfa / HIDE_OVER_TIME;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    #endregion

    #region Update
    private void Update()
    {
        HandleParent();

        CheckHideDelay();
        ChangeBarVisibility();
    }

    #region Transform
    private void HandleParent()
    {
        if (objectToFollow != null)
        {
            FollowParent();
            RotateSameAsParent();
        }
        else
        {
            HandleDestroy();
        }
    }
    private void FollowParent()
    {
        Vector3 parentPosition = objectToFollow.transform.position;
        transform.position = parentPosition + deltaPositionToObject;
    }
    private void RotateSameAsParent()
    {
        if (rotateSameAsParent)
        {
            Quaternion parentRotation = objectToFollow.transform.rotation;
            transform.rotation = parentRotation * deltaRotationFromParent;
        }
    }
    private void HandleDestroy()
    {
        isDestroyed = true;
        Destroy(gameObject);
    }
    #endregion

    #region Change visibility
    private void ShowAllImages()
    {
        foreach (Image image in imagesToHide)
        {
            SetAlfaTo(image, originalAlfa);
        }
    }
    private void HideAllImages()
    {
        foreach (Image image in imagesToHide)
        {
            SetAlfaTo(image, 0);
        }
    }
    private void SetAlfaTo(Image image, float newAlfa)
    {
        Color newColor = new Color(image.color.r, image.color.g, image.color.b, newAlfa);
        SetColor(image, newColor);
    }
    private void ChangeBarVisibility()
    {
        if (isAlwaysVisible)
        {
            return;
        }
        foreach (Image image in imagesToHide)
        {
            ChangeImageVisibility(image);
        }
    }
    private void ChangeImageVisibility(Image image)
    {
        if (isShown)
        {
            MoveAlfaTowards(image, originalAlfa);
        }
        else
        {
            MoveAlfaTowards(image, 0);
        }
    }
    private void MoveAlfaTowards(Image image, float targetAlfa)
    {
        float colorAlfa = image.color.a;
        if (colorAlfa != targetAlfa)
        {
            Color newColor = new Color(image.color.r, image.color.g, image.color.b, CountNewAlfa(image, targetAlfa));
            SetColor(image, newColor);
        }
    }
    private float CountNewAlfa(Image image, float targetAlfa)
    {
        //In how much time it could change from 0 to max value
        float changeThisFrame = visibilityChangeRate * Time.deltaTime;
        float newColorAlfa = Mathf.MoveTowards(image.color.a, targetAlfa, changeThisFrame);
        return newColorAlfa;
    }
    private void SetColor(Image image, Color newColor)
    {
        image.color = newColor;
    }
    #endregion

    private void CheckHideDelay()
    {
        if (isAlwaysVisible)
        {
            return;
        }

        bool pastHideCooldown = Time.time >= lastUsedTime + hideDelay + HIDE_OVER_TIME;
        if (pastHideCooldown)
        {
            isShown = false;
        }
        else
        {
            isShown = true;
        }
    }
    #endregion

    #region Update Bar
    private void UpdateBarRatio(float ratio)
    {
        UpdateLastUsedTime();
        healthBarImage.fillAmount = ratio;

        UpdateGradientColor(ratio);
    }
    private void UpdateLastUsedTime()
    {
        lastUsedTime = Time.time;
    }
    private void UpdateGradientColor(float ratio)
    {
        if (useGradient)
        {
            Color newColor = barColorGradient.Evaluate(ratio);
            newColor.a = healthBarImage.color.a;
            healthBarImage.color = newColor;
        }
    }
    #endregion

    #region Mutator methods
    public void SetHideDelay(float delay)
    {
        hideDelay = delay;
    }
    public void SetObjectToFollow(GameObject followGO)
    {
        objectToFollow = followGO;
    }
    public void SetRotateSameAsParent(bool input)
    {
        rotateSameAsParent = input;
    }
    public void SetDeltaPositionToObject(Vector3 newDeltaPosition)
    {
        deltaPositionToObject = newDeltaPosition;
    }
    public void SetDeltaRotationToObject(float deltaRotation)
    {
        deltaRotationFromParent = Quaternion.Euler(0,0, deltaRotation);
    }
    public void SetIsAlwaysVisible(bool isTrue)
    {
        isAlwaysVisible = isTrue;
        if (isTrue)
        {
            lastUsedTime = Time.time;
            ShowAllImages();
        }
        else
        {
            lastUsedTime = Time.time - hideDelay - HIDE_OVER_TIME;
            HideAllImages();
        }
    }
    public void UpdateProgressionBar(float newRatio)
    {
        if (!isDestroyed)
        {
            newRatio = Mathf.Clamp(newRatio, 0, 1);
            UpdateBarRatio(newRatio);
        }
    }

    #endregion
}
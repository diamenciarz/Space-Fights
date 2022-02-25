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
    [SerializeField] Vector3 deltaPositionToObject;
    [SerializeField] bool useGradient = true;
    [SerializeField] Gradient barColorGradient;
    [SerializeField] [Range(0, 1)] float originalAlfa = 1f;
    /// <summary>
    /// After not receiving damage for this amount of time, the health bar will disappear
    /// </summary>
    [SerializeField] protected float hideOverTime = 0.5f;
    [Tooltip("Time, after which the bar will disappear, after being shown (-1 for never)")]
    [SerializeField] [Range(0, 100)] private float hideDelay = 0;

    [Header("Transform Settings")]
    [SerializeField] bool rotateSameAsParent;

    private Quaternion deltaRotationFromParent;
    private bool isDestroyed;
    Color currentColor;
    private bool isShown = true;
    private double lastUsedTime;
    private float visibilityChangeRate;

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 1, 0);
        //originalAlfa = healthBarImage.color.a;
        visibilityChangeRate = originalAlfa / hideOverTime;

        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    #region Update
    void Update()
    {
        CheckForParent();

        CheckHideDelay();
        ChangeBarVisibility();
    }

    #region Transform
    private void CheckForParent()
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
    private void ChangeBarVisibility()
    {
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
            Color newColor = currentColor;
            newColor.a = CountNewAlfa(image, targetAlfa);
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
        bool pastHideCooldown = Time.time > lastUsedTime + hideDelay;
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
            newColor.a = originalAlfa;
            healthBarImage.color = newColor;

            currentColor = healthBarImage.color;
        }
    }
    #endregion

    #region Mutator methods
    public void SetHideDelay (float delay)
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
    public void SetDeltaRotationToObject(Quaternion newDeltaRotation)
    {
        deltaRotationFromParent = newDeltaRotation;
    }
    public void SetIsVisible(bool isTrue)
    {
        isShown = isTrue;
    }
    public void UpdateProgressionBar(float newHP, float maxHP)
    {
        if (!isDestroyed)
        {
            if (maxHP == 0)
            {
                Debug.LogError("MaxHP was 0 and the ratio was NaN! Followed object: " + objectToFollow);
                return;
            }

            float newRatio = newHP / maxHP;
            newRatio = Mathf.Clamp(newRatio, 0, 1);

            UpdateBarRatio(newRatio);
        }
    }

    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    [SerializeField] Gradient colorGradient;
    [Tooltip("The x offset from the starting position. Time from 0 to 1.")]
    [SerializeField] AnimationCurve xPositionCurve;
    [Tooltip("The y offset from the starting position. Time from 0 to 1.")]
    [SerializeField] AnimationCurve yPositionCurve;
    [Tooltip("The value of angle from the beginning to the end of the animation. Time from 0 to 1.")]
    [SerializeField] AnimationCurve rotationCurve;
    [Tooltip("Given in seconds. The animation curves remain supported on [0,1].")]
    [SerializeField] float lifetime;
    [SerializeField] int minTextSize = 30;
    [SerializeField] int maxTextSize = 60;

    private int maxDamageForGradient = 1;
    private int displayedDamage = 0;
    private TMP_Text TMP;
    private float creationTime;
    private Vector2 startingPosition;
    private float startingRotation;

    #region Startup
    public void Start()
    {
        SetupStartingVariables();
        UpdateColor();
        StartCoroutine(DestroyAfterCooldown());
    }
    private void SetupStartingVariables()
    {
        creationTime = Time.time;
        startingPosition = transform.position;
        startingRotation = transform.rotation.eulerAngles.z;
        TMP = GetComponent<TMP_Text>();
    }
    private void UpdateColor()
    {
        float percentage = (float)displayedDamage / (float)maxDamageForGradient;
        Color textColor = colorGradient.Evaluate(percentage);
        SetColor(textColor);
    }
    private IEnumerator DestroyAfterCooldown()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
    #endregion

    #region Update
    void Update()
    {
        Move();
        Rotate();
    }
    private void Move()
    {
        Vector2 newPosition = startingPosition + CalculateDeltaPosition();
        transform.position = new Vector3(newPosition.x, newPosition.y, 1);
    }
    private Vector2 CalculateDeltaPosition()
    {
        float currentLifetime = Time.time - creationTime;
        float percentage = currentLifetime / lifetime;
        return new Vector2(xPositionCurve.Evaluate(percentage), yPositionCurve.Evaluate(percentage));
    }
    private void Rotate()
    {
        Quaternion newRotation = Quaternion.Euler(0, 0, startingRotation + CalculateDeltaRotation());
        transform.rotation = newRotation;
    }
    private float CalculateDeltaRotation()
    {
        float currentLifetime = Time.time - creationTime;
        float percentage = currentLifetime / lifetime;
        return rotationCurve.Evaluate(percentage);
    }
    #endregion

    #region Mutator methods
    public void SetDamageDisplayed(int damage)
    {
        displayedDamage = damage;
        TMP.text = damage.ToString();
        UpdateTextSize();
        UpdateColor();
    }
    public void SetMaxDamage(int maxDamage)
    {
        maxDamageForGradient = maxDamage;
    }
    private void SetColor(Color color)
    {
        TMP.color = color;
    }
    private void SetTextSize(int size)
    {
        TMP.fontSize = size;
    }
    private void UpdateTextSize()
    {
        float percentage = (float)displayedDamage / (float)maxDamageForGradient;
        int size = (int)Mathf.Lerp(minTextSize, maxTextSize, percentage);
        SetTextSize(size);
    }
    #endregion
}

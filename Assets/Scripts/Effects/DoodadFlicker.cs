using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoodadFlicker : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] float minDelay;
    [SerializeField] float maxDelay;
    [SerializeField] float flickerDuration;

    [SerializeField] List<Gradient> flickerGradients;
    [Tooltip("Each value is the chance for this gradient to be chosen")]
    [SerializeField] [Range(0, 100)] List<int> gradientProbabilities;

    [SerializeField] List<Sprite> alternativeLooks;
    [Tooltip("Each value is the chance for this sprite to be chosen")]
    [SerializeField] [Range(0, 100)] List<int> spriteProbabilities;

    private int chanceSum;
    private SpriteRenderer[] sprites;
    private float flickerStartTime;
    private Gradient currentGradient;
    private bool isVisible;

    
    #region Startup
    private void Start()
    {
        SetupStartingVariables();
        ChooseRandomSprite();
    }
    private void ChooseRandomSprite()
    {
        int chance = Random.Range(0, chanceSum);

        int spriteIndex = HelperMethods.GetIndexFromChance(chance, spriteProbabilities);
        sprites[0].sprite = alternativeLooks[spriteIndex];
    }
    private void SetupStartingVariables()
    {
        currentGradient = flickerGradients[0];
        flickerStartTime = -flickerDuration;
        sprites = GetComponentsInChildren<SpriteRenderer>();
        chanceSum = CountChanceSum();
        StartCoroutine(flickerRoutine());
    }
    private int CountChanceSum()
    {
        int sum = 0;
        foreach (int chance in gradientProbabilities)
        {
            sum += chance;
        }
        return sum;
    }
    #endregion

    private IEnumerator flickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            yield return doFlicker();
        }
    }

    #region Flicker call
    private IEnumerator doFlicker()
    {
        if (isVisible)
        {
            ChooseRandomGradient();
            flickerStartTime = Time.time;
        }
        yield return new WaitForSeconds(flickerDuration);
    }
    private void ChooseRandomGradient()
    {
        int chance = Random.Range(0, chanceSum);
        int gradientIndex = HelperMethods.GetIndexFromChance(chance, gradientProbabilities);
        currentGradient = flickerGradients[gradientIndex];
    }

    #endregion

    #region Update
    private void Update()
    {
        UpdateVisibility();
        UpdateColor();
    }
    private void UpdateVisibility()
    {
        if (CameraInformation.IsPositionOnScreen(transform.position))
        {
            isVisible = true;
            return;
        }
        isVisible = false;

    }
    private void UpdateColor()
    {
        if (!isVisible)
        {
            return;
        }
        SetColor(GetCurrentColor());
    }
    private Color GetCurrentColor()
    {
        float time = Mathf.Clamp01((Time.time - flickerStartTime) / flickerDuration);
        return currentGradient.Evaluate(time);
    }
    private void SetColor(Color newColor)
    {
        foreach (SpriteRenderer renderer in sprites)
        {
            renderer.color = newColor;
        }
    }
    #endregion

    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public void OnBeforeSerialize()
    {
        controlGradientProbabilitiesLength();
        controlSpriteProbabilitiesLength();
    }
    private void controlGradientProbabilitiesLength()
    {
        if (gradientProbabilities.Count < flickerGradients.Count)
        {
            gradientProbabilities.Add(0);
        }
        if (gradientProbabilities.Count > flickerGradients.Count)
        {
            gradientProbabilities.RemoveAt(gradientProbabilities.Count - 1);
        }
    }
    private void controlSpriteProbabilitiesLength()
    {
        if (spriteProbabilities.Count < alternativeLooks.Count)
        {
            spriteProbabilities.Add(0);
        }
        if (spriteProbabilities.Count > alternativeLooks.Count)
        {
            spriteProbabilities.RemoveAt(spriteProbabilities.Count - 1);
        }
    }
    #endregion
}

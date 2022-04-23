using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoodadFlicker : MonoBehaviour
{
    [SerializeField] float minDelay;
    [SerializeField] float maxDelay;
    [SerializeField] float flickerDuration;

    [SerializeField] Gradient[] flickerGradients;
    [Tooltip("Each value is the chance for this gradient to be chosen")]
    [SerializeField] [Range(0, 100)] int[] weightedChance;

    private int chanceSum;
    private SpriteRenderer[] sprites;
    private float flickerStartTime;
    private Gradient currentGradient;
    private bool isVisible;


    private void Start()
    {
        SetupStartingVariables();
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
        foreach (int chance in weightedChance)
        {
            sum += chance;
        }
        return sum;
    }
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
        int gradientIndex = GetIndexFromChance(chance);
        currentGradient = flickerGradients[gradientIndex];
    }
    private int GetIndexFromChance(int chance)
    {
        int sum = 0;
        for (int i = 0; i < weightedChance.Length; i++)
        {
            sum += weightedChance[i];
            if (chance < sum)
            {
                return i;
            }
        }
        return weightedChance.Length - 1;
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
}

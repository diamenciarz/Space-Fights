using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SetRandomTexture : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Texture settings")]
    [SerializeField] List<Sprite> alternativeLooks;
    [Tooltip("Each value is the chance for this sprite to be chosen")]
    [SerializeField] [Range(0, 100)] List<int> spriteProbabilities;

    private SpriteRenderer spriteRenderer;


    #region Startup
    private void Start()
    {
        SetupStartingVariables();
        SetRandomSprite();
    }
    private void SetupStartingVariables()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetRandomSprite()
    {
        int spriteIndex = HelperMethods.ListUtils.GetWeightedIndex(spriteProbabilities);
        if (spriteRenderer)
        {
            spriteRenderer.sprite = alternativeLooks[spriteIndex];
        }
    }
    #endregion

    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public virtual void OnBeforeSerialize()
    {
        controlSpriteProbabilitiesLength();
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

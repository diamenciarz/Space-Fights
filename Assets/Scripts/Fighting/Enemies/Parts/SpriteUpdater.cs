using UnityEngine;

public class SpriteUpdater : TeamUpdater
{
    [Tooltip("Specify the sprites that this part should change according to its team. Starting from team 1, onwards")]
    [SerializeField] Sprite[] sprites;

    //Private variables
    protected SpriteRenderer mySpriteRenderer;

    #region Startup
    private void Start()
    {
        SetupStartingVariables();
    }
    private void SetupStartingVariables()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }
    #endregion

    #region Mutator methods
    public override void UpdateTeam()
    {
        base.UpdateTeam();
        UpdateSprite();
    }
    #endregion
    private void UpdateSprite()
    {
        if (mySpriteRenderer == null)
        {
            return;
        }

        int spriteCount = sprites.Length;
        bool isInBounds = spriteCount > 0 && team > 0 && team <= spriteCount;
        if (isInBounds)
        {
            mySpriteRenderer.sprite = sprites[team - 1];
        }
    }
}

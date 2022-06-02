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
        if (mySpriteRenderer == null)
        {
            mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
    #endregion

    #region Mutator methods
    public override void UpdateTeam(IParent parent)
    {
        base.UpdateTeam(parent);
        UpdateSprite();
    }
    #endregion
    private void UpdateSprite()
    {
        if (mySpriteRenderer == null)
        {
            return;
        }
        if (team.teamInstance == TeamInstance.Team1)
        {
            if (sprites.Length >= 1)
            {
                mySpriteRenderer.sprite = sprites[0];
            }
        }
        if (team.teamInstance == TeamInstance.Team2)
        {
            if (sprites.Length >= 2)
            {
                mySpriteRenderer.sprite = sprites[1];
            }
        }
        if (team.teamInstance == TeamInstance.Team3)
        {
            if (sprites.Length >= 3)
            {
                mySpriteRenderer.sprite = sprites[2];
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPart : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] IOnDestroyed iOnDestroyed;
    [SerializeField] int health;
    [Tooltip("The collision velocity, above which this ship part will start taking damage")]
    [SerializeField] float collisionVelocityForDamage = 10;

    SpriteRenderer mySpriteRenderer;
    private int team;


    void Start()
    {
        SetupStartingVariables();
        UpdateTeam();
    }
    private void SetupStartingVariables()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }
    private void HandleCollision(Collision2D collision)
    {
        ICollision iCollision = collision.gameObject.GetComponent<ICollision>();
        if (iCollision == null || !iCollision.DealsDamageOnCollision())
        {
            return;
        }

        int damage = CountCollisionDamage(iCollision);
        ReceiveDamage(damage);
    }
    private int CountCollisionDamage(ICollision iCollision)
    {
        if (IsVelocityHighEnough(iCollision))
        {
            float speed = iCollision.GetVelocity().magnitude;
            float mass = iCollision.GetMass();
            float damage = (speed / collisionVelocityForDamage) * mass;

            return (int)damage;
        }
        return 0;
    }
    private bool IsVelocityHighEnough(ICollision iCollision)
    {
        return iCollision.GetVelocity().magnitude > collisionVelocityForDamage;
    }

    #region Mutator methods
    public void ReceiveDamage(int damage)
    {
        if (damage == 0)
        {
            return;
        }

        health -= damage;
        CheckHP();
    }
    
    public void SetTeam(int team)
    {
        this.team = team;
        UpdateSprite();
    }
    #endregion

    #region HP
    private void CheckHP()
    {
        if (health <= 0)
        {
            DestroyShipPart();
        }
    }
    private void DestroyShipPart()
    {
        iOnDestroyed.Destroy();
    }
    #endregion

    #region Teams
    private void UpdateTeam()
    {
        team = FindTeam();
        UpdateSprite();
    }
    private int FindTeam()
    {
        DamageReceiver parentScript = GetComponentInParent<DamageReceiver>();
        return parentScript.GetTeam();
    }
    private void UpdateSprite()
    {
        if (mySpriteRenderer == null)
        {
            return;
        }
        if (team == 2)
        {
            mySpriteRenderer.sprite = sprites[1];
        }
        else
        {
            mySpriteRenderer.sprite = sprites[0];
        }
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPart : MonoBehaviour
{
    [Header("Startup")]
    [SerializeField] Sprite[] sprites;
    [Tooltip("Actions to call on death")]
    [SerializeField] IOnDestroyed[] iOnDestroyed;

    [Header("Properties")]
    [SerializeField] int maxHealth;
    [Tooltip("The collision velocity, above which this ship part will start taking damage")]
    [SerializeField] float collisionDeltaVelocity = 10;
    [SerializeField] OnCollisionDamage.TypeOfDamage[] immuneTo;

    SpriteRenderer mySpriteRenderer;
    private Rigidbody2D myRigidbody2D;

    private int team;
    private int health;

    void Start()
    {
        SetupStartingVariables();
        UpdateTeam();
    }
    private void SetupStartingVariables()
    {
        ResetDamage();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    #region Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleDamage(collision.gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
        HandleDamage(collision.gameObject);
    }
    private void HandleCollision(Collision2D collision)
    {
        ICollision iCollision = collision.gameObject.GetComponent<ICollision>();
        if (iCollision == null || !iCollision.DealsCollisionDamage())
        {
            return;
        }

        int damage = CountCollisionDamage(iCollision);
        ReceiveDamage(damage);
    }
    private int CountCollisionDamage(ICollision iCollision)
    {
        Vector2 deltaVelocity = iCollision.GetVelocity() - myRigidbody2D.velocity;
        bool shouldDealDamage = deltaVelocity.magnitude > collisionDeltaVelocity;
        if (!shouldDealDamage)
        {
            return 0;
        }

        float speed = deltaVelocity.magnitude;
        float mass = iCollision.GetMass();
        float damage = (speed * speed / collisionDeltaVelocity) * mass;

        return (int)damage;
    }

    private void HandleDamage(GameObject collidedWithGO)
    {
        IDamageReceived iDamageReceived = collidedWithGO.GetComponent<IDamageReceived>();
        if (iDamageReceived == null || iDamageReceived.GetTeam() != team || IsImmune(iDamageReceived))
        {
            return;
        }

        ReceiveDamage(iDamageReceived.GetDamage());
    }

    private bool IsImmune(IDamageReceived iDamageReceived)
    {
        foreach (var damageType in immuneTo)
        {
            if (iDamageReceived.DamageTypeContains(damageType))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

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
    public void ResetDamage()
    {
        health = maxHealth;
    }
    #endregion

    #region HP
    private void CheckHP()
    {
        if (health <= 0)
        {
            DestroyUnitPart();
        }
    }
    private void DestroyUnitPart()
    {
        foreach (IOnDestroyed item in iOnDestroyed)
        {
            item.DestroyObject();
        }
    }
    #endregion

    #region Teams
    public int GetTeam()
    {
        return team;
    }
    private void UpdateTeam()
    {
        team = GetTeamFromParent();
        UpdateSprite();
    }
    private int GetTeamFromParent()
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPart : MonoBehaviour
{
    [Header("Startup")]
    [Tooltip("Specify the sprites that this part should change according to its team. Starting from team 1, onwards")]
    [SerializeField] Sprite[] sprites;
    [Tooltip("Actions to call on death")]
    [SerializeField] IOnDestroyed[] iOnDestroyed;

    [Header("Properties")]
    [SerializeField] int maxHealth;
    [Tooltip("The collision velocity, above which this ship part will start taking damage")]
    [SerializeField] float collisionDeltaVelocity = 10;
    [SerializeField] OnCollisionDamage.TypeOfDamage[] immuneTo;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;


    SpriteRenderer mySpriteRenderer;
    private Rigidbody2D myRigidbody2D;

    private int team;
    private int health;
    private bool isDestroyed;

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
        HandleDamage(collision.gameObject.GetComponent<IDamageReceived>());
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
        HandleDamage(collision.gameObject.GetComponent<IDamageReceived>());
    }
    private void HandleCollision(Collision2D collision)
    {
        Rigidbody2D colliderRB2D = collision.gameObject.GetComponent<Rigidbody2D>();
        if (colliderRB2D == null)
        {
            return;
        }

        int damage = CountCollisionDamage(colliderRB2D);
        ReceiveDamage(damage);
    }
    private int CountCollisionDamage(Rigidbody2D colliderRB2D)
    {
        Vector2 deltaVelocity = colliderRB2D.velocity - myRigidbody2D.velocity;
        bool shouldDealDamage = deltaVelocity.magnitude > collisionDeltaVelocity;
        if (!shouldDealDamage)
        {
            return 0;
        }

        float speed = deltaVelocity.magnitude;
        float mass = colliderRB2D.mass;
        float damage = (speed * speed / collisionDeltaVelocity) * mass;

        return (int)damage;
    }

    private void HandleDamage(IDamageReceived iDamageReceived)
    {
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
    private void ReceiveDamage(int damage)
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
            HandleBreak();
        }
        else
        {
            HandleHit();
        }
    }
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            StaticDataHolder.PlaySound(GetBreakSound(), transform.position, breakingSoundVolume);
            DestroyObject();
        }
    }
    protected void HandleHit()
    {
        StaticDataHolder.PlaySound(GetHitSound(), transform.position, hitSoundVolume);
    }
    private void DestroyObject()
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

        int spriteCount = sprites.Length;
        bool isInBounds = spriteCount > 0 && team > 0 && team <= spriteCount;
        if (isInBounds)
        {
            mySpriteRenderer.sprite = sprites[team - 1];
        }
    }
    #endregion

    #region Sounds
    protected AudioClip GetHitSound()
    {
        int soundIndex = Random.Range(0, hitSounds.Count);
        if (hitSounds.Count > soundIndex)
        {
            return hitSounds[soundIndex];
        }
        return null;
    }
    protected AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }
    #endregion
}

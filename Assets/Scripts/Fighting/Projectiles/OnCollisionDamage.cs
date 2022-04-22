using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : BreakOnCollision, IDamageDealer
{
    [Header("Basic Stats")]
    [SerializeField] int damage;
    [SerializeField] protected bool hurtsAllies;
    [SerializeField] bool isPiercing;

    [Header("Damage type")]
    public List<TypeOfDamage> damageTypes = new List<TypeOfDamage>();

    public List<GameObject> dealtDamageTo = new List<GameObject>();

    public enum TypeOfDamage
    {
        Projectile,
        Explosion,
        Rocket
    }

    private int currentDamageLeft;


    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        currentDamageLeft = damage;
    }

    #region Collision
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    private void HandleCollision(GameObject collisionObject)
    {
        if (CanDealDamage(collisionObject))
        {
            DealDamageToObject(collisionObject);
        }
    }
    private bool CanDealDamage(GameObject collisionObject)
    {
        if (dealtDamageTo.Contains(collisionObject))
        {
            Debug.Log("Dealt damage already!");
            return false;
        }
        bool isInvulnerable = IsInvulnerableTo(collisionObject);
        if (isInvulnerable)
        {
            return false;
        }
        IDamageable iDamageReceiver = collisionObject.GetComponent<IDamageable>();
        bool objectCanReceiveDamage = iDamageReceiver != null;
        if (!objectCanReceiveDamage)
        {
            return false;
        }
        bool shouldDealDamage = hurtsAllies || iDamageReceiver.GetTeam() != team;
        if (!shouldDealDamage)
        {
            return false;
        }
        return true;
    }

    #region Deal damage
    private void DealDamageToObject(GameObject collisionObject)
    {
        dealtDamageTo.Add(collisionObject);
        IDamageable iDamageReceiver = collisionObject.GetComponent<IDamageable>();
        if (iDamageReceiver.HandleDamage(this))
        {
            HandlePiercing(iDamageReceiver);
        }
    }
    private void HandlePiercing(IDamageable iDamageReceiver)
    {
        if (isPiercing)
        {
            //EmitPiercingParticle();
            LowerMyDamage(iDamageReceiver.GetHealth());
        }
    }
    private void LowerMyDamage(int change)
    {
        currentDamageLeft -= change;
        CheckDamageLeft();
    }
    private void CheckDamageLeft()
    {
        if (currentDamageLeft < 0)
        {
            currentDamageLeft = 0;
            DestroyObject();
        }
    }
    #endregion
    #endregion

    #region Accessor methods
    public int GetDamage()
    {
        return currentDamageLeft;
    }
    public List<TypeOfDamage> GetDamageTypes()
    {
        return damageTypes;
    }
    public bool DamageTypeContains(TypeOfDamage damageType)
    {
        if (damageTypes.Contains(damageType))
        {
            return true;
        }
        return false;
    }
    public bool IsAProjectile()
    {
        return damageTypes.Count != 0;
    }
    public GameObject CreatedBy()
    {
        return createdBy;
    }
    #endregion
}

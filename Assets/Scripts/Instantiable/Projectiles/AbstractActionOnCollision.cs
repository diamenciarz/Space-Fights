using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractActionOnCollision : TeamUpdater
{
    public bool affectObjectOnce = true;
    protected List<GameObject> actedOnObjects = new List<GameObject>();

    #region Collisions
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!actedOnObjects.Contains(collision.gameObject))
        {
            actedOnObjects.Add(collision.gameObject);
        }
        HandleCollision(collision);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!actedOnObjects.Contains(collision.gameObject))
        {
            actedOnObjects.Add(collision.gameObject);
        }
        HandleTriggerEnter(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!affectObjectOnce)
        {
            actedOnObjects.Remove(collision.gameObject);
            HandleExit(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!affectObjectOnce)
        {
            actedOnObjects.Remove(collision.gameObject);
            HandleExit(collision.gameObject);
        }
    }
    protected abstract void HandleCollision(Collision2D collision);
    protected abstract void HandleTriggerEnter(Collider2D trigger);
    protected abstract void HandleExit(GameObject obj);
    #endregion
}

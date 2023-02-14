using System.Collections.Generic;
using UnityEngine;
using static StaticDataHolder;

public class ListUpdater : MonoBehaviour
{
    [Header("Add to lists")]
    [SerializeField] List<ObjectTypes> putInLists = new List<ObjectTypes>();

    #region Activations
    protected void OnEnable()
    {
        AddObjectToLists();
    }
    protected void OnDisable()
    {
        RemoveObjectFromLists();
    }
    protected void OnDestroy()
    {
        RemoveObjectFromLists();
    }
    #endregion

    #region Destroy
    public void DestroyObject()
    {
        HelperMethods.CollisionUtils.DoDestroyActions(gameObject, TriggerOnDeath.DestroyCause.HealthDepleted);
        Destroy(gameObject);
    }
    #endregion

    #region Modify lists
    private void AddObjectToLists()
    {
        foreach (var list in putInLists)
        {
            ListModification.AddObject(list, gameObject);
        }
    }
    private void RemoveObjectFromLists()
    {
        foreach (var list in putInLists)
        {
            ListModification.RemoveObject(list, gameObject);
        }
    }
    public bool ListContains(ObjectTypes element)
    {
        return putInLists.Contains(element);
    }
    public void SetList(List<ObjectTypes> newList)
    {
        RemoveObjectFromLists();
        putInLists = newList;
        AddObjectToLists();
    }
    #endregion
    public List<ObjectTypes> GetList()
    {
        return putInLists;
    }
}

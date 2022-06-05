using System.Collections.Generic;
using UnityEngine;

public class ListUpdater : MonoBehaviour
{
    [Header("Add to lists")]
    [SerializeField] List<StaticDataHolder.ObjectTypes> putInLists = new List<StaticDataHolder.ObjectTypes>();

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
        RemoveObjectFromLists();
        HelperMethods.CollisionUtils.DoDestroyActions(gameObject);
        Destroy(gameObject);
    }
    #endregion

    #region Modify lists
    private void AddObjectToLists()
    {
        foreach (var list in putInLists)
        {
            StaticDataHolder.ListModification.AddObject(list, gameObject);
        }
    }
    private void RemoveObjectFromLists()
    {
        foreach (var list in putInLists)
        {
            StaticDataHolder.ListModification.RemoveObject(list, gameObject);
        }
    }
    public bool ListContains(StaticDataHolder.ObjectTypes element)
    {
        return putInLists.Contains(element);
    }
    #endregion
}

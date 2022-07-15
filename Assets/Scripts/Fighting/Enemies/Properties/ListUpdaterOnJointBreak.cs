using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ListUpdater))]
public class ListUpdaterOnJointBreak : MonoBehaviour
{
    [Header("Add to lists")]
    [SerializeField] List<StaticDataHolder.ObjectTypes> putInLists = new List<StaticDataHolder.ObjectTypes>();

    private bool wasActivated = false;

    #region Activations
    protected void OnJointBreak2D(Joint2D joint)
    {
        SwitchObjectInLists();
    }
    protected void OnEnable()
    {
        if (wasActivated)
        {
            AddObjectToLists();
        }
    }
    protected void OnDisable()
    {
        if (wasActivated)
        {
            RemoveObjectFromLists(putInLists);
        }
    }
    protected void OnDestroy()
    {
        RemoveObjectFromLists(putInLists);
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
    public void SwitchObjectInLists()
    {
        ListUpdater listUpdater = GetComponent<ListUpdater>();
        RemoveObjectFromLists(listUpdater.GetList());
        listUpdater.SetList(putInLists);

        AddObjectToLists();
        wasActivated = true;
    }
    private void AddObjectToLists()
    {
        foreach (var list in putInLists)
        {
            StaticDataHolder.ListModification.AddObject(list, gameObject);
        }
    }
    private void RemoveObjectFromLists(List<StaticDataHolder.ObjectTypes> lists)
    {
        foreach (var list in lists)
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

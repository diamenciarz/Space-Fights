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
            RemoveObjectFromList(putInLists);
        }
    }
    protected void OnDestroy()
    {
        RemoveObjectFromList(putInLists);
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
        listUpdater.SetList(putInLists);

        wasActivated = true;
    }
    private void AddObjectToLists()
    {
        foreach (var list in putInLists)
        {
            StaticDataHolder.ListModification.AddObject(list, gameObject);
        }
    }
    private void RemoveObjectFromList(List<StaticDataHolder.ObjectTypes> lists)
    {
        foreach (var list in lists)
        {
            StaticDataHolder.ListModification.RemoveObject(list, gameObject);
        }
    }
    #endregion
}

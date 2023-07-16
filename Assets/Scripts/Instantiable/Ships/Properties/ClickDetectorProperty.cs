using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClickDetectorProperty : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        List<GameObject> me = new List<GameObject>{gameObject};
        StaticUnitSelector.SetCurrentSelection(me);
    }
}

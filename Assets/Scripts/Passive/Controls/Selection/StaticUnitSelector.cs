using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticUnitSelector
{
    public static List<GameObject> currentSelection;
    public static Dictionary<KeyCode, List<GameObject>> savedSelections = new Dictionary<KeyCode, List<GameObject>>();

    public  static void SetCurrentSelection(List<GameObject> selection)
    {
        currentSelection = selection;
    }
    public static void AddKeySelection(KeyCode key, List<GameObject> selection)
    {
        if (savedSelections.ContainsKey(key))
        {
            savedSelections[key] = selection;
        }
        else
        {
            savedSelections.Add(key, selection);
        }
    }
    public static void DeleteKeySelection(KeyCode key)
    {
        savedSelections.Remove(key);
    }
}

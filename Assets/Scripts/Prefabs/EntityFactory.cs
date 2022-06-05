using UnityEngine;
using System;
using System.Collections.Generic;

public static class EntityFactory
{
    private static Dictionary<string, GameObject> entitiesByName = new Dictionary<string, GameObject>();
    private static Dictionary<string, GameObject> UIByName = new Dictionary<string, GameObject>();

    private static bool initialized = false;
    private static string projectilePath = "Prefabs/Projectiles";
    private static string entityPath = "Prefabs/Ships";
    private static string obstaclePath = "Prefabs/Obstacles";
    private static string UIPath = "Prefabs/UI";

    #region Initialization
    public static void InitializeFactory()
    {
        if (!initialized)
        {
            initialized = true;
            FillDictionaryWithEntities(entitiesByName, projectilePath);
            FillDictionaryWithEntities(entitiesByName, entityPath);
            FillDictionaryWithEntities(entitiesByName, obstaclePath);
            FillDictionaryWithEntities(UIByName, UIPath);
        }
    }
    private static void FillDictionaryWithEntities(Dictionary<string, GameObject> dictionary, string path)
    {
        //error
        GameObject[] foundProjectiles = Resources.LoadAll<GameObject>(path);

        foreach (GameObject projectile in foundProjectiles)
        {
            dictionary.Add(projectile.name, projectile);
        }
    }
    #endregion

    #region Factory methods
    public static GameObject GetPrefab(EntityCreator.EntityTypes enumName)
    {
        string name = enumName.ToString();
        if (entitiesByName.ContainsKey(name))
        {
            return entitiesByName[name];
        }
        return null;
    }
    public static GameObject GetPrefab(EntityCreator.UIComponents enumName)
    {
        string name = enumName.ToString();
        if (UIByName.ContainsKey(name))
        {
            return UIByName[name];
        }
        return null;
    }
    #endregion
}

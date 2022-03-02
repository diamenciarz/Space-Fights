using UnityEngine;
using System;
using System.Collections.Generic;

public static class EntityFactory
{
    private static Dictionary<String, GameObject> entitiesByName = new Dictionary<string, GameObject>();

    private static bool initialized = false;
    private static string projectilePath = "Prefabs/Projectiles";
    private static string entityPath = "Prefabs/Ships";

    #region Initialization
    public static void InitializeFactory()
    {
        if (!initialized)
        {
            initialized = true;
            entitiesByName = new Dictionary<string, GameObject>();
            FillDictionaryWithEntities(projectilePath);
            FillDictionaryWithEntities(entityPath);
        }
    }
    private static void FillDictionaryWithEntities(string path)
    {
        //error
        GameObject[] foundProjectiles = Resources.LoadAll<GameObject>(path);

        foreach (GameObject projectile in foundProjectiles)
        {
            entitiesByName.Add(projectile.name, projectile);
        }
    }
    #endregion

    public static GameObject GetPrefab(EntityCreator.EntityTypes enumName)
    {
        string name = enumName.ToString();
        if (entitiesByName.ContainsKey(name))
        {
            return entitiesByName[name];
        }
        return null;
    }
}


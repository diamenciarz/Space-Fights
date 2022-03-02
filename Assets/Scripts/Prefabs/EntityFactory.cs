using UnityEngine;
using System;
using System.Collections.Generic;

public static class EntityFactory
{
    private static Dictionary<String, GameObject> entitiesByName = new Dictionary<string, GameObject>();

    private static bool initialized = false;
    private static string projectilePath = "Assets/Prefabs/Projectiles/";
    private static string entityPath = "Assets/Prefabs/Ships/";

    #region Initialization
    public static void InitializeFactory()
    {
        if (!initialized)
        {
            initialized = true;
            FillDictionaryWithEntities(projectilePath);
            FillDictionaryWithEntities(entityPath);
        }
    }
    private static void FillDictionaryWithEntities(string path)
    {
        entitiesByName = new Dictionary<string, GameObject>();
        GameObject[] foundProjectiles = Resources.LoadAll(path, typeof(GameObject)) as GameObject[];
        foreach (GameObject projectile in foundProjectiles)
        {
            entitiesByName.Add(projectile.name, projectile);
        }
    }
    #endregion

    public static GameObject GetPrefab(EntityCreator.EntityTypes enumName)
    {
        string name = enumName.ToString();
        InitializeFactory();

        if (entitiesByName.ContainsKey(name))
        {
            return entitiesByName[name];
        }
        return null;
    }
}


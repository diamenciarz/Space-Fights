using UnityEngine;
using System.Collections.Generic;

public static class EntityFactory
{
    private static Dictionary<string, GameObject> projectiles = new Dictionary<string, GameObject>(); // Projectiles and destructible obstacles
    private static Dictionary<string, GameObject> entities = new Dictionary<string, GameObject>(); // Ships and indestructible walls
    private static Dictionary<string, GameObject> UIs = new Dictionary<string, GameObject>(); // Non physical items

    private static bool initialized = false;
    private static string projectilePath = "Prefabs/Projectiles";
    private static string entityPath = "Prefabs/Ships";
    private static string obstaclePath = "Prefabs/Obstacles";
    private static string indestructiblesPath = "Prefabs/Indestructibles";
    private static string UIPath = "Prefabs/UI";

    #region Initialization
    public static void InitializeFactory()
    {
        if (!initialized)
        {
            initialized = true;
            FillDictionaryWithEntities(projectiles, projectilePath);
            FillDictionaryWithEntities(projectiles, obstaclePath);
            FillDictionaryWithEntities(entities, entityPath);
            FillDictionaryWithEntities(entities, indestructiblesPath);
            FillDictionaryWithEntities(UIs, UIPath);
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
    public static GameObject GetPrefab(EntityCreator.Projectiles enumName)
    {
        return searchFor(enumName.ToString(), projectiles);
    }
    public static GameObject GetPrefab(EntityCreator.Entities enumName)
    {
        return searchFor(enumName.ToString(), entities);
    }
    public static GameObject GetPrefab(EntityCreator.ProgressionBars enumName)
    {
        return searchFor(enumName.ToString(), UIs);
    }
    public static GameObject GetPrefab(EntityCreator.ProgressionCones enumName)
    {
        return searchFor(enumName.ToString(), UIs);
    }
    public static GameObject GetPrefab(EntityCreator.ObjectMissingIcons enumName)
    {
        return searchFor(enumName.ToString(), UIs);
    }

    private static GameObject searchFor(string name, Dictionary<string, GameObject> dictionary)
    {
        if (dictionary.ContainsKey(name))
        {
            return dictionary[name];
        }
        return null;
    }
    #endregion
}

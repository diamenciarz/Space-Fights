using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    public List<GameObject> doodads;
    public List<int> spawnCount;

    void Start()
    {
        generateDoodads();
    }
    private void generateDoodads()
    {
        for (int i = 0; i < doodads.Count; i++)
        {
            GameObject doodad = doodads[i];
            spawnNCopies(doodad, spawnCount[i]);
        }
    }
    private void spawnNCopies(GameObject doodad, int count)
    {
        for (int j = 0; j < count; j++)
        {
            //Makes itself the parent of the doodads for organization purposes.
            //The doodads can move freely by themselves.
            Instantiate(doodad, transform);
        }
    }

    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public void OnBeforeSerialize()
    {
        controlSpawnCountLength();
    }
    private void controlSpawnCountLength()
    {
        if (spawnCount.Count < doodads.Count)
        {
            spawnCount.Add(0);
        }
        if (spawnCount.Count > doodads.Count)
        {
            spawnCount.RemoveAt(spawnCount.Count - 1);
        }
    }
    #endregion
}

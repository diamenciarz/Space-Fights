using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    public List<GameObject> doodads;
    public List<int> spawnCount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

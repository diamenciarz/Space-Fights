using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Instances")]
    [SerializeField] List<GameObject> objectsToGenerate;
    [SerializeField] List<int> probabilities;
    [SerializeField] List<float> maxVelocities;

    [Header("Generation settings")]
    [SerializeField] float delay = 0;
    [SerializeField][Range(0, 10)] int minGeneratedObjects = 2;
    [SerializeField][Range(1, 20)] int maxGeneratedObjects = 5;

    private Coroutine generationCoroutine;
    private void Start()
    {
        generationCoroutine = StartCoroutine(GenerationLoop());
    }
    private IEnumerator GenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);

            int n = Random.Range(minGeneratedObjects, maxGeneratedObjects);
            for (int i = 0; i < n; i++)
            {
                GenerateRandomObj();
            }
        }
    }
    private void GenerateRandomObj()
    {
        if (StaticDataHolder.listDictionary[StaticDataHolder.ObjectTypes.Obstacle].Count > StaticDataHolder.OBSTACLE_LIMIT)
        {
            return;
        }
        SummonedGameObjectData data = new SummonedGameObjectData();
        int objectIndex = HelperMethods.ListUtils.GetWeightedIndex(probabilities);

        data.gameObject = objectsToGenerate[objectIndex];
        SetPositionAndRotation(data);
        data.startingVelocity = GetVelocity(data, objectIndex);
        EntityCreator.SummonGameObject(data);
    }
    private void SetPositionAndRotation(SummonedGameObjectData data)
    {
        float sum = StaticMapInformation.mapWidth + StaticMapInformation.mapHeight;
        bool generateFromCeil = Random.Range(0f, sum) > StaticMapInformation.mapWidth;
        if (generateFromCeil)
        {
            if (Random.Range(0, 2) == 0)
            {
                data.summonPosition = StaticMapInformation.GetMapPercentagePosition(Random.Range(0, 1f), 1);
                data.summonRotation = Quaternion.Euler(0, 0,Random.Range(120,240));
            }
            else
            {
                data.summonPosition = StaticMapInformation.GetMapPercentagePosition(Random.Range(0, 1f), 0);
                data.summonRotation = Quaternion.Euler(0, 0,Random.Range(-60,60));
            }
        }
        else
        {
            if (Random.Range(0, 2) == 0)
            {
                data.summonPosition = StaticMapInformation.GetMapPercentagePosition(1, Random.Range(0, 1f));
                data.summonRotation = Quaternion.Euler(0, 0,Random.Range(60,120));
            }
            else
            {
                data.summonPosition = StaticMapInformation.GetMapPercentagePosition(0, Random.Range(0, 1f));
                data.summonRotation = Quaternion.Euler(0, 0,Random.Range(240,300));
            }
        }
    }
    private Vector2 GetVelocity(SummonedGameObjectData data, int index)
    {
        float velocity = maxVelocities[index];
        return HelperMethods.VectorUtils.DirectionVector(velocity, data.summonRotation.eulerAngles.z);
    }
    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public virtual void OnBeforeSerialize()
    {
        controlListLengths();
    }
    private void controlListLengths()
    {
        if (probabilities.Count < objectsToGenerate.Count)
        {
            probabilities.Add(0);
        }
        if (probabilities.Count > objectsToGenerate.Count)
        {
            probabilities.RemoveAt(probabilities.Count - 1);
        }
        if (maxVelocities.Count < objectsToGenerate.Count)
        {
            maxVelocities.Add(0);
        }
        if (maxVelocities.Count > objectsToGenerate.Count)
        {
            maxVelocities.RemoveAt(maxVelocities.Count - 1);
        }
        if (minGeneratedObjects > maxGeneratedObjects)
        {
            minGeneratedObjects = maxGeneratedObjects;
        }
    }
    #endregion
}

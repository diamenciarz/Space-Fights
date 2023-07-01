using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour, ISerializationCallbackReceiver
{
    public List<GameObject> doodads;
    public List<int> doodadSpawnCount;


    public List<GameObject> regularBackground;
    public List<int> regularBGSpawnDistance;

    private List<Vector2> spawnedBGCoords = new List<Vector2>();
    /// <summary>
    /// Parallax scroller camera distance factor
    /// </summary>\
    float CAM_DIST_FACT = ParallaxScroller.CAMERA_DISTANCE_FACTOR + 0.5f;

    void Start()
    {
        GenerateDoodads();
        GenerateRegularBackground();
        /*
        List<Vector2> coords = GetSpawnCoordinates(8);
        Vector2[] diagonalCameraPoints = CameraInformation.GetDiagonalCameraPoints();
        Debug.Log("Diagonal camera BL: " + diagonalCameraPoints[0]);
        Debug.Log("Diagonal camera TR: " + diagonalCameraPoints[1]);

        foreach (var item in coords)
        {
            Debug.Log(item);
        }
        */
    }

    #region Doodads
    private void GenerateDoodads()
    {
        for (int i = 0; i < doodads.Count; i++)
        {
            GameObject doodad = doodads[i];
            spawnNCopies(doodad, doodadSpawnCount[i]);
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
    #endregion

    #region Regular background
    private void GenerateRegularBackground()
    {
        for (int i = 0; i < regularBackground.Count; i++)
        {
            GameObject backgroundItem = regularBackground[i];
            SpawnRegularCopies(backgroundItem, regularBGSpawnDistance[i]);
        }
    }
    private void SpawnRegularCopies(GameObject doodad, float distance)
    {
        List<Vector2> spawnCoordinates = GetSpawnCoordinates(distance);
        foreach (Vector2 coords in spawnCoordinates)
        {
            if (spawnedBGCoords.Contains(coords))
            {
                continue;
            }
            //Makes itself the parent of the doodads for organization purposes.
            //The doodads can move freely by themselves.
            GameObject spawnedObj = Instantiate(doodad, transform);
            ParallaxScroller parallaxScroller = spawnedObj.GetComponent<ParallaxScroller>();
            if (parallaxScroller != null)
            {
                parallaxScroller.GoToPosition(coords);
                spawnedBGCoords.Add(coords);
            }
        }
    }
    private List<Vector2> GetSpawnCoordinates(float gridDistance)
    {
        Vector2[] coordinateRange = GetCoordinateRange(gridDistance);
        // xMax - xMin
        int xRepetitions = (int)((coordinateRange[1].x - coordinateRange[0].x) / gridDistance);
        int yRepetitions = (int)((coordinateRange[1].y - coordinateRange[0].y) / gridDistance);

        List<Vector2> spawnCoordinates = new List<Vector2>();
        for (int x = 0; x <= xRepetitions; x++)
        {
            for (int y = 0; y <= yRepetitions; y++)
            {
                float xCoord = coordinateRange[0].x + gridDistance * (x + Random.Range(0, 0.5f));
                float yCoord = coordinateRange[0].y + gridDistance * (y + Random.Range(0, 0.5f));
                spawnCoordinates.Add(new Vector2(xCoord, yCoord));
            }
        }
        return spawnCoordinates;
    }

    private Vector2[] GetCoordinateRange(float gridDistance)
    {
        Vector2[] diagonalCameraPoints = CameraInformation.GetDiagonalCameraPoints();
        float xMin = Mathf.Floor(CAM_DIST_FACT * diagonalCameraPoints[0].x / gridDistance) * gridDistance;
        float yMin = Mathf.Floor(CAM_DIST_FACT * diagonalCameraPoints[0].y / gridDistance) * gridDistance;
        float xMax = Mathf.Ceil(CAM_DIST_FACT * diagonalCameraPoints[1].x / gridDistance) * gridDistance;
        float yMax = Mathf.Ceil(CAM_DIST_FACT * diagonalCameraPoints[1].y / gridDistance) * gridDistance;
        Vector2 bottomLeftPoint = new Vector2(xMin, yMin);
        Vector2 topRightPoint = new Vector2(xMax, yMax);

        return new Vector2[] {bottomLeftPoint, topRightPoint};
    }
    #endregion

    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public void OnBeforeSerialize()
    {
        ControlDoodadSpawnCountLength();
        ControlRegularBGSpawnCountLength();
    }
    private void ControlDoodadSpawnCountLength()
    {
        if (doodadSpawnCount.Count < doodads.Count)
        {
            doodadSpawnCount.Add(0);
        }
        if (doodadSpawnCount.Count > doodads.Count)
        {
            doodadSpawnCount.RemoveAt(doodadSpawnCount.Count - 1);
        }
    }
    private void ControlRegularBGSpawnCountLength()
    {
        if (regularBGSpawnDistance.Count < regularBackground.Count)
        {
            regularBGSpawnDistance.Add(0);
        }
        if (regularBGSpawnDistance.Count > regularBackground.Count)
        {
            regularBGSpawnDistance.RemoveAt(regularBGSpawnDistance.Count - 1);
        }
    }
    #endregion
}

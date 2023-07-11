using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInformation : MonoBehaviour
{
    [SerializeField] float mapHeight;
    [SerializeField] float mapWidth;

    private void Start()
    {
        float x = mapWidth / 2;
        float y = mapHeight / 2;
        StaticMapInformation.topRightCorner = new Vector2(x, y);
        StaticMapInformation.bottomLeftCorner = new Vector2(-x, -y);
    }
}

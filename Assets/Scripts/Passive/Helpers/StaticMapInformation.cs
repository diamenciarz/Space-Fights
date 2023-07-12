using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticMapInformation
{
    public static Vector2 bottomLeftCorner;
    public static Vector2 topRightCorner;

    public static float mapWidth;
    public static float mapHeight;

    public static void SetMapCorners(Vector2 topRight, Vector2 botLeft)
    {
        bottomLeftCorner = botLeft;
        topRightCorner = topRight;

        mapWidth = topRight.x - botLeft.x;
        mapHeight = topRight.y - botLeft.y;
    }
}

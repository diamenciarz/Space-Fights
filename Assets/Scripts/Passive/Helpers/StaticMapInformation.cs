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
    public static Vector2 GetMapPercentagePosition(float xPercentage, float yPercentage)
    {
        return new Vector2(bottomLeftCorner.x + mapWidth * Mathf.Clamp01(xPercentage), bottomLeftCorner.y + mapHeight * Mathf.Clamp01(yPercentage));
    }
}

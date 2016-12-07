// Author: Matteo Bevan
using UnityEngine;
using System.Collections;

public class Utilities
{
    public static float MapFromHeightMapToWorld(float value, bool wantZOffset, bool isX, int currentMatrix = -1)
    {
        if (currentMatrix == -1)
            currentMatrix = CyberspaceManager.GetInstance().currentMatrix;

        Vector3 bottomLeft = CyberspaceManager.GetInstance().startBottomLeft;
        Vector3 topRight = CyberspaceManager.GetInstance().startTopRight;
        int heightMapSize = CyberspaceManager.GetInstance().heightMapSize;
        
        float min;
        float max;
        float mapWorldLength;

        if (isX)
        {
            min = bottomLeft.x;
            max = topRight.x;
            mapWorldLength = CyberspaceManager.GetInstance().mapWorldLengthX;
        }
        else
        {
            min = bottomLeft.z;
            max = topRight.z;
            mapWorldLength = CyberspaceManager.GetInstance().mapWorldLengthZ;
        }

        float offset;

        if (!wantZOffset)
            offset = 0.0f;
        else
            offset = (max - min) * currentMatrix;

        return offset + min + (mapWorldLength * value / (heightMapSize - 1));
    }

    public static int MapFromWorldToHeightMap(float value, int heightMapSize, float max, float min, int currentMatrix = -1)
    {
        if (currentMatrix == -1)
            currentMatrix = CyberspaceManager.GetInstance().currentMatrix;

        float offset = (max - min) * currentMatrix;

        float mapWorldLength = Mathf.Abs(max) + Mathf.Abs(min);

        return Mathf.RoundToInt((value - min - offset) * (heightMapSize - 1) / mapWorldLength);   
    }
}

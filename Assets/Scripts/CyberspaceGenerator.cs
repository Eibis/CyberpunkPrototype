// Author: Matteo Bevan
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CyberspaceGenerator : MonoBehaviour 
{
    //BEZIER PARAMETERS
    int numControlPoints = 16;
    float[] binoms;
    List<Vector3[,]> controlPoints;

    int numSegments = 150;
    float radius = 1f;
    int numPointsForCircle = 25;

    //GAME PARAMETERS
    public GameObject tube;

    //MAP PARAMETERS
    float[,] heightMap;
    int heightMapSize;

    float minY = -50f;
    float maxY = 50f;
    float maxDelta = 10f;

    int numMaximum = 3;
    int numMinimum = 3;

    int[,] maximum;
    int[,] minimum;

    public float GetMaxY()
    {
        return maxY;
    }

    public float GetMinY()
    {
        return minY;
    }

    //init function
    public void PrivateStart (Vector3 bottomLeft, Vector3 topRight, int heightMapSize)
    {
        controlPoints = new List<Vector3[,]>();

        this.heightMapSize = heightMapSize;

        generateHeightMap();

        generateBezierControlPoints();

        binoms = new float[numControlPoints];

        for (int i = 0; i < numControlPoints; i++)
            binoms[i] = binomialCoefficent(i);

        produceTubes();
        
        CyberspaceManager.GetInstance().Setup(heightMap);
	}

    public void Generate(Vector3 bottomLeft, Vector3 topRight, int heightMapSize)
    {
        this.heightMapSize = heightMapSize;

        generateHeightMap();

        CyberspaceManager.GetInstance().AddHeightMap(heightMap);
        
        generateBezierControlPoints();

        produceTubes();

        CyberspaceManager.GetInstance().FinishTubes();
    }

    public IEnumerator GenerateCoroutine(Vector3 bottomLeft, Vector3 topRight, int heightMapSize, int howMany)
    {
        this.heightMapSize = heightMapSize;

        for (int i = 0; i < howMany; i++)
        { 
            generateHeightMap();

            yield return null;

            CyberspaceManager.GetInstance().AddHeightMap(heightMap);

            generateBezierControlPoints();

            yield return null;

            produceTubes();

            CyberspaceManager.GetInstance().FinishTubes();

            CyberspaceManager.GetInstance().DeleteOldMatrix();
        }
    }

    void generateHeightMap()
    {
        heightMap = new float[heightMapSize, heightMapSize];

        maximum = new int[numMaximum, 2];

        for (int i = 0; i < numMaximum; i++)
        {
            maximum[i, 0] = Random.Range(0, heightMapSize);
            maximum[i, 1] = Random.Range(0, heightMapSize);

            heightMap[maximum[i, 0], maximum[i, 1]] = Random.Range(maxY - maxY * 0.25f, maxY) + 1;
        }

        minimum = new int[numMinimum, 2];

        for (int i = 0; i < numMinimum; i++)
        {
            minimum[i, 0] = Random.Range(0, heightMapSize);
            minimum[i, 1] = Random.Range(0, heightMapSize);

            heightMap[minimum[i, 0], minimum[i, 1]] = Random.Range(minY, minY + minY * 0.25f) - 1;
        }

        //setup for the maxs
        for (int i = 0; i < numMaximum; i++)
            levelDown(heightMap[maximum[i, 0], maximum[i, 1]], maximum[i, 0], maximum[i, 1]);
        

        for (int i = 0; i < numMinimum; i++)
            levelUp(heightMap[minimum[i, 0], minimum[i, 1]], minimum[i, 0], minimum[i, 1]);
    }

    void generateBezierControlPoints()
    {
        //setup bezier control points
        controlPoints.Add(new Vector3[numControlPoints, heightMapSize]);

        int currentControlPoints = controlPoints.Count - 1;

        for (int i = 0; i < heightMapSize; i++)
        {
            for (int j = 0; j < numControlPoints; j++)
            {
                int zIndexOnHeightMap = j * heightMapSize / numControlPoints;

                controlPoints[currentControlPoints][j, i] = new Vector3(
                                                0.0f,
                                                heightMap[zIndexOnHeightMap, i],
                                                Utilities.MapFromHeightMapToWorld(zIndexOnHeightMap, false, false));
            }
        }
    }

    void levelDown(float currentValue, int x, int y)
    {
        if (currentValue <= (maxY - maxDelta))
            return;

        if(x >= heightMapSize || y >= heightMapSize || x < 0 || y < 0)
            return;
        
        if (heightMap[x, y] != 0 && heightMap[x, y] != currentValue)
            return;

        heightMap[x, y] = currentValue;

        levelDown(currentValue - 1, x, y + 1);
        levelDown(currentValue - 1, x + 1, y);
        levelDown(currentValue - 1, x, y - 1);
        levelDown(currentValue - 1, x - 1, y);

        //diagonals
        levelDown(currentValue - 2, x + 1, y + 1);
        levelDown(currentValue - 2, x - 1, y + 1);
        levelDown(currentValue - 2, x - 1, y - 1);
        levelDown(currentValue - 2, x + 1, y - 1);
    }

    void levelUp(float currentValue, int x, int y)
    {
        if (currentValue >= (minY + maxDelta))
            return;

        if(x >= heightMapSize || y >= heightMapSize || x < 0 || y < 0)
            return;

        if (heightMap[x, y] != 0 && heightMap[x, y] != currentValue)
            return;

        heightMap[x, y] = currentValue;

        levelUp(currentValue + 1, x, y + 1);
        levelUp(currentValue + 1, x + 1, y);
        levelUp(currentValue + 1, x, y - 1);
        levelUp(currentValue + 1, x - 1, y);

        //diagonals

        levelUp(currentValue + 2, x + 1, y + 1);
        levelUp(currentValue + 2, x - 1, y + 1);
        levelUp(currentValue + 2, x - 1, y - 1);
        levelUp(currentValue + 2, x + 1, y - 1);
    }

    void produceTubes()
    {
        CyberspaceManager.GetInstance().tubes.Add(new GameObject[heightMapSize]);

        int currentMatrix = CyberspaceManager.GetInstance().tubes.Count - 1;

        for (int w = 0; w < heightMapSize; w++)
        {
            CyberspaceManager.GetInstance().tubes[currentMatrix][w] = Instantiate(tube) as GameObject;

            CyberspaceManager.GetInstance().tubes[currentMatrix][w].transform.position = new Vector3(Utilities.MapFromHeightMapToWorld(w, false, true), 0.0f, Utilities.MapFromHeightMapToWorld(heightMapSize / 2, true, false, currentMatrix));

            float fraz = 0.0f;

            Vector3[,] data = new Vector3[numSegments + 1, 3]; //position, normal and tangent

            for (int i = 0; i < numSegments + 1; i++)
            {
                Vector3 b1 = bezierPos(fraz, w, controlPoints.Count - 1);
                Vector3 b2 = bezierPos(fraz + (1.0f / numSegments), w, controlPoints.Count - 1);

                data[i, 0] = b1;
                data[i, 2] = (b2 - b1).normalized; //tangent
                data[i, 1] = Vector3.Cross(data[i, 2], Vector3.up).normalized; //normal

                fraz += (1.0f / numSegments);
            }
    
            Mesh tempMesh = new Mesh();
            CyberspaceManager.GetInstance().tubes[currentMatrix][w].GetComponent<MeshFilter>().sharedMesh = tempMesh;
            tempMesh.name = "Tube1";

            tempMesh.Clear();
        
            Vector3[] vertices;
            int[] indices;

            vertices = new Vector3[(numSegments + 1) * numPointsForCircle];
            indices = new int[((numSegments) * (numPointsForCircle)) * 6];

            for (int i = 0; i < numSegments + 1; i++)
            {
                for (int j = 0; j < numPointsForCircle; j++)
                {
                    float rotAngle = 360.0f / numPointsForCircle * j;

                    Quaternion anglesT = Quaternion.AngleAxis(rotAngle, data[i, 2]);

                    vertices[i * numPointsForCircle + j] = RotatePointAroundPivot(((radius * data[i, 1]) + data[i, 0]), data[i, 0], anglesT);
                }
            }

            for (int i = 0; i < numSegments; i++)
            {
                for (int j = 0; j < numPointsForCircle; j++)
                {

                    if (j == numPointsForCircle - 1)
                    {
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 0] = (i + 1) * numPointsForCircle + 0;
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 1] = (i + 1) * numPointsForCircle + j;
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 2] = i * numPointsForCircle + j;

                        indices[i * (numPointsForCircle) * 6 + j * 6 + 3] = i * numPointsForCircle + 0;
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 4] = (i + 1) * numPointsForCircle + 0;
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 5] = i * numPointsForCircle + j;
                    }
                    else
                    { 
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 0] = (i + 1) * numPointsForCircle + (j + 1);
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 1] = (i + 1) * numPointsForCircle + j;
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 2] = i * numPointsForCircle + j;

                        indices[i * (numPointsForCircle) * 6 + j * 6 + 3] = i * numPointsForCircle + (j + 1);
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 4] = (i + 1) * numPointsForCircle + (j + 1);
                        indices[i * (numPointsForCircle) * 6 + j * 6 + 5] = i * numPointsForCircle + j;
                    }
                }
            }

            tempMesh.vertices = vertices;
            tempMesh.triangles = indices;
            
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            ;
        }
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion angles)
    {
        Vector3 dir = point - pivot;
        dir = angles * dir;
        point = dir + pivot;
        return point; 
    }

    #region GizmosBezier
    /*
    void OnDrawGizmos()
    {
        if (binoms == null)
            return;

        Gizmos.color = Color.yellow;
        
        float fraz = 0.0f;

        fraz = 0.0f;

        Vector3[,] data = new Vector3[numSegments, 3]; //position, normal and tangent

        for (int i = 0; i < numSegments - 1; i++)
        {
            Vector3 b1 = bezierPos(fraz);
            Vector3 b2 = bezierPos(fraz + (1.0f / numSegments));

            data[i, 0] = b1;
            data[i, 2] = (b2 - b1).normalized; //tangent
            data[i, 1] = Vector3.Cross(data[i, 2], Vector3.up).normalized; //normal

            fraz += (1.0f / numSegments);
        }

        for (int i = 0; i < numSegments; i++)
        {
            for (int j = 0; j < numPointsForCircle; j++)
            {
                Gizmos.color = Color.red;

                float rotAngle = 360.0f / numPointsForCircle * j;

                Quaternion anglesT = Quaternion.AngleAxis(rotAngle, data[i, 2]);

                Gizmos.DrawSphere(RotatePointAroundPivot(((radius * data[i, 1]) + data[i, 0]), data[i, 0], anglesT), 0.25f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(data[i, 0], (radius * data[i, 1]) + data[i, 0]);

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(data[i, 0], (radius * data[i, 2]) + data[i, 0]);
            }

            break;
        }

    }
    */

#endregion

    public Vector3 bezierPos(float t, int curveIndex, int nMatrix)
    {
        float ti = 1.0f - t;
        
        Vector3 res = Vector3.zero;

        for (int i = 0; i < numControlPoints; i++)
        {
            float binom = binoms[i];
            float level = Mathf.Pow(t, i) * Mathf.Pow(ti, numControlPoints - 1 - i);
            res += controlPoints[nMatrix][i, curveIndex] * binom * level;
        }

        return res;
    }

    float binomialCoefficent(int index)
    {
        return factorial(numControlPoints - 1) / (factorial(index) * factorial(numControlPoints - 1 - index));
    }
    
    float factorial(float n)
    {
        if (n < 0)
        {
            Debug.Log("ouh cerchi di spaccarmi il pc?");
            return -1;
        }

        if (n == 0)
            return 1;

        return n * factorial(n - 1);
    }

    public void produceBuilding()
    { 
    //TODO basta un piano, due vettor ibottomright e topleft e height e si producono due cose di tubi di traverso - metti un gameobject padre cosi da cancellare tutto assieme
    }


}

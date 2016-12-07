// Author: Matteo Bevan
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//main class everything starts here
public class CyberspaceManager : MonoBehaviour 
{
    //singleton
    static CyberspaceManager _this;

    public static CyberspaceManager GetInstance()
    {
        return _this;
    }

    //dynamic list of the tubes
    public List<GameObject[]> tubes;
    
    //parameters to switch tube
    public int currentTube = 0;
    public int currentMatrix = 0;

    //height map to create the map
    public List<float[,]> heightMap;
    public int heightMapSize;

    //parameters for world generation
    public Vector3 startBottomLeft;
    public Vector3 startTopRight;
    public float mapWorldLengthZ;
    public float mapWorldLengthX;

    //references
    public CyberspaceGenerator generator;
    public CyberspacePlayer player;

    //pickable icons
    List<GameObject[]> icons;
    int nIconPerMatrix = 3;

    //BACKGROUND ELEMENTS
    GameObject sphere;
    GameObject cylinder;
    GameObject background;
    List<GameObject[]> backgroundElements;
    int nElementsPerMatrix = 50;

    //UI PARAMETERS
    public UIProgressBar loading;
    public GameObject cyberspace_element;

	IEnumerator Start () 
    {
        _this = this;

        tubes = new List<GameObject[]>();
        icons = new List<GameObject[]>();
        heightMap = new List<float[,]>();
        generator = GetComponent<CyberspaceGenerator>();
        
        background = new GameObject();
        background.name = "Background";
        
        backgroundElements = new List<GameObject[]>();
        sphere = Resources.Load("Sphere") as GameObject;
        cylinder = Resources.Load("Cylinder") as GameObject;

        mapWorldLengthZ = Mathf.Abs(startBottomLeft.z) + Mathf.Abs(startTopRight.z);
        mapWorldLengthX = Mathf.Abs(startBottomLeft.x) + Mathf.Abs(startTopRight.x);

        //generator init
        generator.PrivateStart(startBottomLeft, startTopRight, heightMapSize);

        int nStartingHeightmap = 6;

        for (int i = 0; i < nStartingHeightmap; i++)
        {
            loading.value = ((float) i + 1) / nStartingHeightmap;
            
            yield return null;
        
            generator.Generate(startBottomLeft, startTopRight, heightMapSize);
        }

        yield return new WaitForSeconds(0.5f);
        NGUITools.SetActive(loading.transform.parent.gameObject, false);
	}

    public void AddIcons(int indexMatrix)
    {
        icons.Add(new GameObject[nIconPerMatrix]);

        for (int index = 0; index < nIconPerMatrix; index++)
        { 
            int i = Random.Range(0, heightMapSize);
            int j = Random.Range(0, heightMapSize);

            float x = Utilities.MapFromHeightMapToWorld(i, false, true);

            float z = Utilities.MapFromHeightMapToWorld(j, true, false, indexMatrix);

            float percPerc = (z + (mapWorldLengthZ / 2) - mapWorldLengthZ * indexMatrix) / mapWorldLengthZ;

            float y = generator.bezierPos(percPerc, i, indexMatrix).y + 10.0f;

            icons[indexMatrix][index] = Instantiate(cyberspace_element, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }
    }

    public void AddHeightMap(float [,] newHeightMap)
    {
        heightMap.Add(newHeightMap);

        for (int i = 0; i < heightMapSize; i++)
            heightMap[heightMap.Count - 1][0, i] = heightMap[heightMap.Count - 2][heightMapSize - 1, i];

     //   CreateBackgroundElements(heightMap.Count - 1); NEURONI
    }

    public void Setup(float[,] heightMap, int matrix = 0)
    {
        this.heightMap.Add(heightMap);

        currentTube = tubes[currentMatrix].Length / 2;

        for (int i = currentMatrix; i < this.heightMap.Count; i++)
            tubes[i][currentTube].GetComponent<MeshRenderer>().material.color = Color.red;

        AddIcons(currentMatrix);

        player.SetupPlayer();
    }

    public void FinishTubes()
    {
        tubes[heightMap.Count - 1][currentTube].GetComponent<MeshRenderer>().material.color = Color.red;

        AddIcons(heightMap.Count - 1);
    }

    public bool SwitchTube(int offset)
    {
        if (currentTube + offset < 0 || currentTube + offset >= tubes[currentMatrix].Length)
            return false;

        for (int i = currentMatrix; i < heightMap.Count; i++ )
            tubes[i][currentTube].GetComponent<MeshRenderer>().material.color = new Color32(0, 181, 255, 255);

        currentTube += offset;

        for (int i = currentMatrix; i < heightMap.Count; i++)
            tubes[i][currentTube].GetComponent<MeshRenderer>().material.color = Color.red;
        
        return true;
    }

    public void SwitchMatrix()
    {
        currentMatrix++;

        if (currentMatrix > tubes.Count - 4)
            StartCoroutine(generator.GenerateCoroutine(startBottomLeft, startTopRight, heightMapSize, 3));
    }

    public void DeleteOldMatrix()
    {
        for (int i = 0; i < currentMatrix; i++)
        {
            if (heightMap[i] != null)
            {
                heightMap[i] = null;

                for (int j = 0; j < heightMapSize; j++)
                    GameObject.Destroy(tubes[i][j]);

                tubes[i] = null;

                for (int j = 0; j < nIconPerMatrix; j++)
                    GameObject.Destroy(icons[i][j]);

                icons[i] = null;

                for (int j = 0; j < nElementsPerMatrix; j++)
                    GameObject.Destroy(backgroundElements[i][j]);

                backgroundElements[i] = null;
            }
        }
    }

    public void CreateBackgroundElements(float offset)
    {
        GameObject[] temp = new GameObject[nElementsPerMatrix];

        float x = Random.Range(5 * startBottomLeft.x, 5 * startTopRight.x);
        float y = Random.Range(10 * generator.GetMinY(), 5 * generator.GetMinY());
        float z = mapWorldLengthZ * offset + Random.Range(5 * startBottomLeft.z, 5 * startTopRight.z);

        GameObject sphereTemp = Instantiate(sphere, new Vector3(x, y, z), Quaternion.identity) as GameObject;
        sphereTemp.transform.parent = background.transform;

        temp[0] = sphereTemp;

        Vector3 precPos = temp[0].transform.position;
        
        for (int i = 1; i < nElementsPerMatrix / 2; i++)
        {
            x = Random.Range(5 * startBottomLeft.x, 5 * startTopRight.x);
            y = Random.Range(10 * generator.GetMinY(), 5 * generator.GetMinY());
            z = mapWorldLengthZ * offset + Random.Range(5 * startBottomLeft.z, 5 * startTopRight.z);

            sphereTemp = Instantiate(sphere, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            sphereTemp.transform.parent = background.transform;

            temp[i * 2 - 1] = sphereTemp;

            Vector3 tempPos = sphereTemp.transform.position - precPos;

            GameObject cylinderTemp = Instantiate(cylinder) as GameObject;
            cylinderTemp.transform.parent = background.transform;

            cylinderTemp.transform.position = tempPos / 2.0f + precPos;

            var v3T = cylinderTemp.transform.localScale;

            v3T.y = tempPos.magnitude / 2.0f;
            cylinderTemp.transform.localScale = v3T;

            cylinderTemp.transform.rotation = Quaternion.FromToRotation(Vector3.up, tempPos);

            temp[i * 2] = cylinderTemp;

            precPos = temp[i * 2 - 1].transform.position;
        }

        backgroundElements.Add(temp);

    }

    //UI METHODS

}

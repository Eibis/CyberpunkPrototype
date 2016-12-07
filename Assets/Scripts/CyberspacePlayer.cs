// Author: Matteo Bevan
using UnityEngine;
using System.Collections;

public class CyberspacePlayer : MonoBehaviour 
{
    float speed = 60.0f;
    float mediumSpeed = 60.0f;

    bool switching = false;

	void Start () 
    {
	
	}
	
	void Update () 
    {
        if (switching)
            return;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveRight();
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeft();
        else
            Move();
    }

    public void MoveLeft()
    {
        if (CyberspaceManager.GetInstance().SwitchTube(-1))
            Switch();
    }

    public void MoveRight()
    {
        if (CyberspaceManager.GetInstance().SwitchTube(1))
            Switch();
    }

    void Switch()
    { 
        switching = true;

        float newZ = transform.position.z + speed;
        
        float percPerc = (newZ + (CyberspaceManager.GetInstance().mapWorldLengthZ / 2) - CyberspaceManager.GetInstance().mapWorldLengthZ * CyberspaceManager.GetInstance().currentMatrix) / CyberspaceManager.GetInstance().mapWorldLengthZ;

        if (percPerc >= 1.0f)
        {
            CyberspaceManager.GetInstance().SwitchMatrix();
            percPerc = (newZ + (CyberspaceManager.GetInstance().mapWorldLengthZ / 2) - CyberspaceManager.GetInstance().mapWorldLengthZ * CyberspaceManager.GetInstance().currentMatrix) / CyberspaceManager.GetInstance().mapWorldLengthZ;
        }

        float newY = CyberspaceManager.GetInstance().generator.bezierPos(percPerc, CyberspaceManager.GetInstance().currentTube, CyberspaceManager.GetInstance().currentMatrix).y;

        float newX = Utilities.MapFromHeightMapToWorld(CyberspaceManager.GetInstance().currentTube, false, true);

        Vector3 res = new Vector3(  newX,
                                    newY,
                                    newZ);

        iTween.MoveTo(gameObject, iTween.Hash("position", res, 
                                                "time", 0.75f,
                                                "oncomplete", "EndTween",
                                                "easetype", iTween.EaseType.easeInSine
                                                ));
    }

    public void EndTween()
    {
        switching = false;
    }

    public void Move()
    {
        float newZ = transform.position.z + speed * Time.deltaTime;

        float percPerc = (newZ + (CyberspaceManager.GetInstance().mapWorldLengthZ / 2) - CyberspaceManager.GetInstance().mapWorldLengthZ * CyberspaceManager.GetInstance().currentMatrix) / CyberspaceManager.GetInstance().mapWorldLengthZ;

        if (percPerc >= 1.0f)
        {
            CyberspaceManager.GetInstance().SwitchMatrix();
            percPerc = (newZ + (CyberspaceManager.GetInstance().mapWorldLengthZ / 2) - CyberspaceManager.GetInstance().mapWorldLengthZ * CyberspaceManager.GetInstance().currentMatrix) / CyberspaceManager.GetInstance().mapWorldLengthZ;
        }

        float newY = CyberspaceManager.GetInstance().generator.bezierPos(percPerc, CyberspaceManager.GetInstance().currentTube, CyberspaceManager.GetInstance().currentMatrix).y;

        float impedance = Mathf.Atan2(newY - transform.position.y, (newZ - transform.position.z));

        speed = mediumSpeed - impedance * 20f;
        
        transform.position = new Vector3(transform.position.x,
                                         newY,
                                         newZ);
	
    }

    public void SetupPlayer()
    {
        float x = Utilities.MapFromHeightMapToWorld(CyberspaceManager.GetInstance().currentTube, false, true);

        transform.position = new Vector3(x,
                                         CyberspaceManager.GetInstance().heightMap[CyberspaceManager.GetInstance().currentMatrix][0, CyberspaceManager.GetInstance().currentTube], 
                                        CyberspaceManager.GetInstance().startBottomLeft.z);
    }
}

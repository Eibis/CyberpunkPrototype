// Author: Matteo Bevan
using UnityEngine;
using System.Collections;

public class CyberspaceInteractable : MonoBehaviour 
{
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            Debug.Log("entered");
    }
    	
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fach : MonoBehaviour
{
    private GameObject manager;
    private Manager managerScript;
    private int fachIdx = 0; // welches fach das ist
    private int regalIdx = 0; // in welchem regal  
    private GameObject item = null; // item in dem fach 
    Renderer objRenderer;
    
    private void Start()
    {
        objRenderer = GetComponent<Renderer>();
        manager = GameObject.FindGameObjectWithTag("Manager");
        managerScript = manager.GetComponent<Manager>();
        objRenderer.material.color = managerScript.fachLeer;
    }

    public void auslagern()
    {
        item = null;
        objRenderer.material.color = managerScript.fachLeer;

    }

    public bool istFrei()
    {
        return item == null;
    }

    public void setItem(GameObject item_)
    {
        objRenderer.material.color = managerScript.fachVoll;
        item = item_;
    }

    public void init(int regalIdx_, int fachIdx_)
    {
        regalIdx = regalIdx_;
        fachIdx = fachIdx_;
    }
           

    public GameObject getItemObj() { return item; }
}

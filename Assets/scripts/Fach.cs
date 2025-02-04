using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fach : MonoBehaviour
{
    private GameObject manager;
    private Manager managerScript;
    private int fachIdx = 0; // welches fach das ist
    private int etage = 0;
    private int regalIdx = 0; // in welchem regal  
    private String item = null; // item in dem fach 
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

    public int getFachIdx() { return fachIdx; }
    public int getRegalIdx() { return regalIdx; }
    public int get_etage() { return etage; }

    public bool istFrei()
    {
        return item == null;
    }

    public void setItem(String name)
    {
        objRenderer.material.color = managerScript.fachVoll;
        item = name;
    }

    public void init(int regalIdx_, int etage_, int fachIdx_)
    {
        regalIdx = regalIdx_;
        fachIdx = fachIdx_;
        etage = etage_;
    }
           

    public String getItemName() { return item; }
}

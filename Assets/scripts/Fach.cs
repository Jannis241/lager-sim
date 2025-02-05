using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fach : MonoBehaviour
{
    private GameObject manager;
    private Manager managerScript;
    public int fachIdx = 0; // welches fach das ist
    public int etage = 0;
    public int regalIdx = 0; // in welchem regal  
    public String item;
    private Color itemFarbe;
    private Renderer objRenderer;
    public void init(int regalIdx_, int etage_, int fachIdx_)
    {
        manager = GameObject.FindGameObjectWithTag("Manager");
        managerScript = manager.GetComponent<Manager>();

        item = null;
        itemFarbe = managerScript.fachLeer;

        objRenderer = GetComponent<Renderer>();
        objRenderer.material.color = managerScript.fachLeer;

        regalIdx = regalIdx_;
        fachIdx = fachIdx_;
        etage = etage_;
    }


    public void auslagern()
    {
        item = null;
        itemFarbe = managerScript.fachLeer;
        objRenderer.material.color = managerScript.fachLeer;

    }
    public int getFachIdx() { return fachIdx; }
    public int getRegalIdx() { return regalIdx; }
    public int get_etage() { return etage; }

    public bool istFrei()
    {
        return item == null;
    }

    public void setItem(String name, Color farbe)
    {
        objRenderer.material.color = farbe;
        itemFarbe = farbe;
        item = name;
    }


    public String getItemName() { return item; }
    public Color getItemFarbe() { return itemFarbe; }
    
}

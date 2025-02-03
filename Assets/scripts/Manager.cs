using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Manager : MonoBehaviour
{

    private int faecher_count = 0;

    [Header("Roboter")]
    public GameObject roboterPrefab;

    public float roboterProzentAnzahl = .33f;


    [Header("Roboter Boden")]
    public GameObject roboterBodenPrefab;
    [Range(0.001f, 1)]
    public float dicke;
    public Color farbe;

    [Header("Kamera")]
    public GameObject cam;
    [Range(5, 100)]
    public float camHeightSpawnOffset = 10;
    [Range(5, 100)]
    public float camDepthSpawnOffset = 10;

    [Range(0.5f, 5)]
    public float lookSpeed = 2f;
    
    [Range(3, 50)]
    public float moveSpeed = 10f;

    [Range(1, 20)]
    public float verticalSpeed = 5f;
   

    [Header("Boden")]

    public GameObject bodenPrefab;

    [Range(10, 500)]
    public int bodenPufferX = 100;
    [Range(10, 500)]
    public int bodenPufferZ = 100;


    [Header("Regal Einstellungen")]

    public GameObject regalPrefab;
    public Color regalFarbe;
    [Range(2, 500)]

    public int regalAnzahl = 3;

    [Range(0.1f, 10.0f)]
    public float regalBreite = 2.0f;

    [Range(1f, 1000.0f)]
    public float regalHoehe = 6.0f;

    [Range(1f, 1000.0f)]
    public float regalLaenge = 10.0f;

    [Range(0.1f, 20.0f)]
    public float regalAbstand = 6.0f;
    [Header("Fächer Einstellungen")]
    public GameObject fachPrefab;
    [Range(0.001f, 10.0f)]
    public float fach_size = 1f;
    [Range(0.001f, 0.5f)]
    public float fachTiefe = 0.1f;

    [Range(0.001f, 5.0f)] 
    public float fachAbstand = 2.0f;

    public Color fachVoll = Color.red;
    public Color fachLeer = Color.green;

    private List<GameObject> regale = new List<GameObject>();
       void spawn_boden()
    {
        float platz_x = (regalBreite * regalAnzahl) + (regalAbstand * (regalAnzahl - 1));


        // set cam to middle 
        float cam_x = platz_x / 2f;
        float cam_y = regalHoehe + camHeightSpawnOffset;
        float cam_z = camDepthSpawnOffset; 

        cam.gameObject.transform.position = new Vector3(cam_x, cam_y, cam_z);
        cam.gameObject.transform.rotation = Quaternion.identity;


        Vector3 pos = new Vector3(Mathf.Floor(platz_x / 2f), -.5f, Mathf.Floor(regalLaenge / 2f));  // durch 0.5scale ist es perfekt in der mitte da der boden 1 groß ist und die koord immer die mitte ist
        
        spawn_roboter_boden(pos); // die position des normalen bodens kann man auch für die robotor boden benutzen 

        GameObject boden = Instantiate(bodenPrefab, pos, Quaternion.identity);

        boden.transform.localScale = new Vector3(platz_x + bodenPufferX, 1f, regalLaenge +bodenPufferZ);
    }

    void Start()
    {
        roboterProzentAnzahl = Mathf.Clamp(roboterProzentAnzahl, 0f, 1f); // Wert bleibt zwischen 0 und 100

        cam.GetComponent<Camera>().init(lookSpeed, moveSpeed, verticalSpeed);

        spawn_regale();
        spawn_boden(); // cam wird hier auch richtig positioniert 

    }
    
    void spawn_roboter_boden(Vector3 pos)
    {
        float platz_x = (regalBreite * regalAnzahl) + (regalAbstand * (regalAnzahl - 1));

        int faecher_y = Mathf.FloorToInt(regale[0].transform.localScale.y / (fachAbstand + fach_size));

        GameObject regal_boden = Instantiate(roboterBodenPrefab, new Vector3(pos.x - regalBreite / 2 - 0.0001f, 0.0001f,pos.z -0.0001f), Quaternion.identity);
        regal_boden.transform.localScale = new Vector3(platz_x,  dicke, regalLaenge);
        Renderer r2 = regal_boden.GetComponent<Renderer>();
        r2.material.color = regalFarbe;
        for (int i = 0; i <= faecher_y; i++)
        {
            float y = (fach_size + fachAbstand)  * i + fachAbstand / 2;
            GameObject spawnedObject = Instantiate(roboterBodenPrefab, new Vector3(pos.x - regalBreite / 2 - 0.0001f, y+0.0001f,pos.z -0.0001f), Quaternion.identity);
            Renderer r = spawnedObject.GetComponent<Renderer>();
            farbe.a = 0.1f;
            r.material.color = farbe;
            if (i == faecher_y)
            {
                r.material.color = regalFarbe;
            }
            spawnedObject.transform.localScale = new Vector3(platz_x,  dicke, regalLaenge);
            spawnedObject.name = "Roboter Boden";

        }


    }

    void spawn_regale()
    {
        
        for (int i = 0; i < regalAnzahl; i++)
        {
            float xPosition = i * (regalAbstand + regalBreite);  // der abstand UND die regal breite muss berücksichtigt werden
            float yPosition = regalHoehe / 2f;
            float zPosition = regalLaenge / 2f;

            Vector3 pos = new Vector3(xPosition, yPosition, zPosition);

            GameObject spawnedObject = Instantiate(regalPrefab, pos, Quaternion.identity);
            spawnedObject.transform.localScale = new Vector3(regalBreite, regalHoehe, regalLaenge);

            Renderer r = spawnedObject.GetComponent<Renderer>();

            r.material.color = regalFarbe;

            float untereYKoordinate = spawnedObject.transform.position.y - spawnedObject.transform.localScale.y / 2f;

            Regal regalScript = spawnedObject.GetComponent<Regal>();
            regalScript.set_idx(i);
            regalScript.spawn_faecher();

            spawnedObject.name = "Regal " + i;
            faecher_count += regalScript.get_faecher_count();
            regale.Add(spawnedObject);
            
        }

        Debug.Log("Spawned Fächer: " + faecher_count);
    }



}

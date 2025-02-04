using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{


    private int faecher_count = 0;



    [Header("Roboter")]
    public GameObject roboterPrefab;
    public float roboterProzentAnzahl = .33f;

    [Range(0.0f, 3f)] private float roboterSize;
    [Range(0.5f, 3f)] public float roboterSpeed = 1f;
    [Header("Roboter Boden")]
    public GameObject roboterBodenPrefab;
    [Range(0.001f, 1)] public float dicke;
    public Color farbe;

    [Header("Kamera")]
    public GameObject cam;
    [Range(5, 100)] public float camHeightSpawnOffset = 10;
    [Range(-20, 20)] public float camDepthSpawnOffset = -10;
    [Range(0.5f, 5)] public float lookSpeed = 2f;
    [Range(3, 50)] public float moveSpeed = 10f;
    [Range(1, 20)] public float verticalSpeed = 5f;

    [Header("Boden")]
    public GameObject bodenPrefab;
    [Range(10, 500)] public int bodenPufferX = 100;
    [Range(10, 500)] public int bodenPufferZ = 100;

    [Header("Regal Einstellungen")]
    public GameObject regalPrefab;
    public Color regalFarbe;
    [Range(2, 500)] public int regalAnzahl = 3;
    [Range(0.1f, 10.0f)] public float regalBreite = 2.0f;
    [Range(1f, 1000.0f)] public float regalHoehe = 6.0f;
    [Range(1f, 1000.0f)] public float regalLaenge = 10.0f;
    [Range(0.1f, 20.0f)] public float regalAbstand = 6.0f;

    [Header("Fächer Einstellungen")]
    public GameObject fachPrefab;
    [Range(0.001f, 10.0f)] public float fach_size = 1f;
    [Range(0.001f, 0.5f)] public float fachTiefe = 0.1f;
    [Range(0.001f, 5.0f)] public float fachAbstand = 2.0f;
    public Color fachVoll = Color.red;
    public Color fachLeer = Color.green;

    private List<GameObject> regale = new List<GameObject>();


    private GameObject roboterParent;
    private GameObject bodenParent;
    private GameObject regalParent;
    private GameObject roboterBodenParent;

    private List<GameObject> alle_roboter = new List<GameObject>();

    public void random_einsortieren(List<Ware> items)
    {
        int ware_anzahl = 0;
        for (int i = 0; i < items.Count; i++) {
            ware_anzahl += items[i].anzahl; 
        }
        if (ware_anzahl > faecher_count)
        {
            Debug.LogError("ACHTUNG: LAGER IST ZU KLEIN FÜR DIE MENGE AN WARE");

        }
        else
        {
            foreach (Ware item in items)
            {
                for (int i = 0; i < item.anzahl; i++)
                {


                    List<Fach> alle_faecher = new List<Fach>();

                    for (int j = 0; j < regalAnzahl; j++)
                    {
                        List<Fach> faecher = regale[j].GetComponent<Regal>().get_all_empty_faecher(); 

                        for (int k = 0; k < faecher.Count; k++)
                        {
                            alle_faecher.Add(faecher[k]);
                        }

                    }

                    Fach fach;

                    fach = alle_faecher[UnityEngine.Random.Range(0, alle_faecher.Count)];
                    fach.setItem(item.name);

                }

            }
        }

        Debug.Log(ware_anzahl + " Objekte eingelagert.");

        float capactiy = (float)ware_anzahl / faecher_count * 100; 

        Debug.Log("Nutzung der möglichen Kapazität: " + capactiy + "%.");

    }
    public void smart_einsortieren(List<Ware> items)
    {

    }


    private void robot_test()
    {
        GameObject robot_object = alle_roboter[UnityEngine.Random.Range(0, alle_roboter.Count)];

        Roboter robot = robot_object.GetComponent<Roboter>();

        robot.go_to_fach(1, 2);
    }

    void Start()
    {
        roboterParent = new GameObject("Roboter");
        bodenParent = new GameObject("Boden");
        regalParent = new GameObject("Regale");
        roboterBodenParent = new GameObject("Roboter Boden");

        roboterProzentAnzahl = Mathf.Clamp(roboterProzentAnzahl, 0f, 1f);
        cam.GetComponent<Camera>().init(lookSpeed, moveSpeed, verticalSpeed);


        roboterSize = fach_size / 4 + fachAbstand / 4;


        spawn_regale();
        spawn_boden();
        spawn_roboter();

        Debug.Log("Lager Kapazität: " + faecher_count);
        


        robot_test();
    }
    public float getRoboSize()
    {
        return roboterSize;
    }

    void spawn_roboter()
    {
        int roboter_to_spawn = regalAnzahl - 1;
        int faecher_y = Mathf.FloorToInt(regale[0].transform.localScale.y / (fachAbstand + fach_size));
        // Loop für jede Reihe (zwischen den Regalen)
        for (int i = 0; i < roboter_to_spawn; i++)
        {
            int roboter_spawn = (int)Mathf.Floor(regalHoehe * roboterProzentAnzahl);

            // Loop für die Etagen
            for (int j = 0; j < faecher_y * roboterProzentAnzahl; j++)
            {
                float x = i * (regalAbstand + regalBreite) + regalBreite / 2 + regalAbstand / 2 + roboterSize / 2;
                float y = j * (fach_size + fachAbstand) + roboterSize * 2 + dicke * 2;
                float z = fach_size + getRoboSize() / 2; // ganz vorne einfach

                Vector3 pos = new Vector3(x, y, z);
                GameObject instance = Instantiate(roboterPrefab, pos, Quaternion.identity, roboterParent.transform);

                alle_roboter.Add(instance);

                instance.transform.localScale = new Vector3(roboterSize, roboterSize, roboterSize);



                Roboter roboter = instance.GetComponent<Roboter>();
                roboter.init(i, j);
            }
        }
    }


    void spawn_boden()
    {
        float platz_x = (regalBreite * regalAnzahl) + (regalAbstand * (regalAnzahl - 1));
        float cam_x = platz_x / 2f;
        float cam_y = regalHoehe + camHeightSpawnOffset;
        float cam_z = camDepthSpawnOffset;
        cam.gameObject.transform.position = new Vector3(cam_x, cam_y, cam_z);
        cam.gameObject.transform.rotation = Quaternion.identity;

        Vector3 pos = new Vector3(platz_x / 2f, -0.5f, regalLaenge / 2f);
        spawn_roboter_boden(pos);
        GameObject boden = Instantiate(bodenPrefab, pos, Quaternion.identity, bodenParent.transform);
        boden.transform.localScale = new Vector3(platz_x + bodenPufferX, 1f, regalLaenge + bodenPufferZ);
    }

    void spawn_roboter_boden(Vector3 pos)
    {
        float platz_x = (regalBreite * regalAnzahl) + (regalAbstand * (regalAnzahl - 1));
        int faecher_y = Mathf.FloorToInt(regale[0].transform.localScale.y / (fachAbstand + fach_size));


        for (int i = 0; i <= faecher_y; i++)
        {
            float y = (fach_size + fachAbstand) * i + 1; 
            GameObject spawnedObject = Instantiate(roboterBodenPrefab, new Vector3(pos.x-regalBreite / 2f + 0.0001f, y, pos.z + 0.0001f), Quaternion.identity, roboterBodenParent.transform);
            spawnedObject.transform.localScale = new Vector3(platz_x - 0.0005f, dicke, regalLaenge - 0.0005f);
            spawnedObject.name = "Roboter Boden" + i;
            Renderer r = spawnedObject.GetComponent<Renderer>();

            r.material.color = farbe;
        }
    }

    void spawn_regale()
    {
        for (int i = 0; i < regalAnzahl; i++)
        {
            float xPosition = i * (regalAbstand + regalBreite);
            float yPosition = regalHoehe / 2f;
            float zPosition = regalLaenge / 2f;

            Vector3 pos = new Vector3(xPosition, yPosition, zPosition);
            GameObject spawnedObject = Instantiate(regalPrefab, pos, Quaternion.identity, regalParent.transform);
            spawnedObject.transform.localScale = new Vector3(regalBreite, regalHoehe, regalLaenge);
            spawnedObject.GetComponent<Renderer>().material.color = regalFarbe;

            Regal regalScript = spawnedObject.GetComponent<Regal>();
            regalScript.set_idx(i);
            regalScript.spawn_faecher();
            regale.Add(spawnedObject);

            faecher_count += regalScript.get_faecher_count();
        }
    }
}

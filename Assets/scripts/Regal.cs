using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;

public class Regal : MonoBehaviour
{
    private int regalIdx;
    private GameObject managerObj;
    private Manager manager;

    private int faecher_count = 0;

    private int faecher_x; // Anzahl der Fächer in der Breite (links/rechts)
    private int faecher_y; // Anzahl der Fächer in der Höhe (oben/unten)
    private int faecher_z; // Anzahl der Fächer in der Tiefe (vorne/hinten)


    private GameObject fachPrefab; // Prefab für ein Fach

    private List<GameObject> faecher_links;  
    private List<GameObject> faecher_rechts;  

    public List<GameObject> get_faecher_links()
    {
        return faecher_links;
    }

    public List<GameObject> get_faecher_rechts()
    {
        return faecher_rechts ;
    }
    public int get_faecher_count()
    {
        return faecher_count;
    }

    public List<Fach> get_all_empty_faecher()
    {
        if (faecher_links == null) faecher_links = new List<GameObject>();
        if (faecher_rechts == null) faecher_rechts = new List<GameObject>();
        List<GameObject> alle_faecher = faecher_links.Concat(faecher_rechts).ToList();
        
        List<Fach> alle_leeren_faecher = new List<Fach>();

        for (int i = 0; i < alle_faecher.Count; i++)
        {
            if (alle_faecher[i].GetComponent<Fach>().istFrei())
            {
                alle_leeren_faecher.Add(alle_faecher[i].gameObject.GetComponent<Fach>());
            }
        }


        return alle_leeren_faecher;

    }

    public void set_idx(int idx)
    {
        regalIdx = idx;
    }

    private List<GameObject> spawn_faecher_seite(Vector3 basePosition, Vector3 directionX, Vector3 directionY, Vector3 directionZ, bool isLeftSide)
    {
        List<GameObject> faecher = new List<GameObject>();

        // Berechnung der Mittelposition des Regals, um den Versatz zu berücksichtigen
        Vector3 regalMittelpunkt = transform.position;
        int fachIdx = 0;
        for (int x = 0; x < faecher_x; x++)
        {
            for (int y = 0; y < faecher_y; y++)
            {
                for (int z = 0; z < faecher_z; z++)
                {
                    // Position für jedes Fach berechnen
                    Vector3 fachPosition = basePosition +
                                           directionX * x * (manager.fachAbstand + manager.fach_size) +
                                           directionY * y * (manager.fachAbstand + manager.fach_size) +
                                           directionZ * z * (manager.fachAbstand + manager.fach_size);

                    // Tiefe des Fachs anpassen
                    float fach_tiefe = manager.fachTiefe; // oder eine variable Tiefe
                    GameObject fach = Instantiate(fachPrefab, fachPosition, Quaternion.identity);

                    Fach fachScript = fach.GetComponent<Fach>();
                    fachScript.init(regalIdx,y,z);

                    // Setze die Skalierung, aber berechne den Versatz für die Position
                    fach.transform.localScale = new Vector3(fach_tiefe, manager.fach_size, manager.fach_size);

                    // Berechne den Versatz basierend auf der Skalierung (verschiebt das Fach nach hinten)
                    // Bei Skalierung auf der x-Achse wird der Versatz die x-Position um halbe Fachgröße verschieben
                    Vector3 versatz = new Vector3(
                         isLeftSide ? manager.fach_size / 2 : -manager.fach_size / 2, // Versatz auf x-Achse je nach Seite
                        0, // Keine Veränderung auf y-Achse
                        0  // Keine Veränderung auf z-Achse
                    );

                    // Wende den Versatz an, sodass das Fach immer noch an der richtigen Stelle bleibt
                    fach.transform.position = fach.transform.position + versatz;
                    

                    // Setze das Fach als Kind des Regals
                    fach.transform.SetParent(transform);

                    // Füge das Fach zur Liste hinzu
                    faecher.Add(fach);
                    faecher_count++;
                    fachIdx ++;
                }
            }
        }
        return faecher;
    }

    public void spawn_rechte_faecher()
    {
        // Berechnung der Startposition für rechte Fächer
        Vector3 startPosition = transform.position +
                                transform.right * (transform.localScale.x / 2 + manager.fach_size / 2) -
                                transform.up * ((faecher_y - 1) / 2f * (manager.fachAbstand + manager.fach_size)) -
                                transform.forward * ((faecher_z - 1) / 2f * (manager.fachAbstand + manager.fach_size));

        faecher_rechts = spawn_faecher_seite(startPosition, Vector3.right, transform.up, transform.forward, false); // false für rechts
    }

    public void spawn_linke_faecher()
    {
        // Berechnung der Startposition für linke Fächer
        Vector3 startPosition = transform.position -
                                transform.right * (transform.localScale.x / 2 + manager.fach_size / 2) -
                                transform.up * ((faecher_y - 1) / 2f * (manager.fachAbstand + manager.fach_size)) -
                                transform.forward * ((faecher_z - 1) / 2f * (manager.fachAbstand + manager.fach_size));

        faecher_links = spawn_faecher_seite(startPosition, Vector3.left, transform.up, transform.forward, true); // true für links
    }


    // Wird von dem Manager gecallt
    public void spawn_faecher()
    {
        managerObj = GameObject.FindGameObjectWithTag("Manager");
        manager = managerObj.GetComponent<Manager>();

        fachPrefab = manager.fachPrefab;
        

        // Anzahl der Fächer in jeder Dimension berechnen
        faecher_x = 1; // Nur links oder rechts -> keine Bewegung in X-Richtung nötig
        faecher_y = Mathf.FloorToInt(transform.localScale.y / (manager.fachAbstand + manager.fach_size));
        faecher_z = Mathf.FloorToInt(transform.localScale.z / (manager.fachAbstand + manager.fach_size));

        // Fächer abhängig von der Regalposition spawnen
        if (regalIdx == 0)
        {
            spawn_rechte_faecher();
        }
        else if (regalIdx == manager.regalAnzahl - 1)
        {
            spawn_linke_faecher();
        }



        else
        {
            spawn_linke_faecher();
            spawn_rechte_faecher();
        }
    }
}

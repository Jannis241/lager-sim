using System.Collections.Generic;
using UnityEngine;

public class Einladung : MonoBehaviour
{
    public List<Ware> warenListe = new List<Ware>(); // Liste für die Waren
    public Mode sortierungsMode;
    private Manager manager;

    public enum Mode
    {
        Random,
        Smart,
    }
    void Start()
        {
            Invoke(nameof(LateStart), 0.5f); 
        }

    void LateStart()

    {



        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        if (sortierungsMode == Mode.Random)
        {
            manager.random_einsortieren(warenListe);
        }
        if (sortierungsMode == Mode.Smart)
        {
            manager.smart_einsortieren(warenListe);
        }
    }
}


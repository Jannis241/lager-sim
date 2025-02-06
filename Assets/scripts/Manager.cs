using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;

public class Manager : MonoBehaviour
{
    private Queue<RobotOrder> auftragsWarteschlange = new Queue<RobotOrder>();

    private struct RobotOrder
    {
        public int RegalIdx;
        public int Etage;
        public int FachIdx;
        public int Seite;

        public RobotOrder(int regalIdx, int etage, int fachIdx, int seite)
        {
            RegalIdx = regalIdx;
            Etage = etage;
            FachIdx = fachIdx;
            Seite = seite;
        }
    }

    private int faecher_count = 0;

    [Header("Bestellung")]
    public List<Ware> warenListe;
    public Mode sortierungsMode;
    private Manager manager;

    public enum Mode
    {
        Random,
        Smart,
    }

    [Header("Item")]
    public GameObject itemPrefab;
    public bool item_drop_rotation = false;
    public bool item_hitbox = true;

    [Header("Roboter")]
    public GameObject roboterPrefab;
    public float roboterProzentAnzahl = .33f;
    [Range(0.01f, 50f)]public float roboterBeladungsDelay = 10f;
    [Range(0.01f, 20f)]public float roboterSleepDelay = 2f;

    private float roboterSize;
    [Range(0.001f, 1000f)] public float roboterSpeed = 1f;
    [Header("Roboter Boden")]
    public GameObject roboterBodenPrefab;
    [Range(0.001f, .5f)] public float dicke;
    public Color farbe;

    [Header("Kamera")]
    public GameObject cam;
    [Range(-20, 20)] public float camHeightSpawnOffset = 10;
    [Range(-50, 50)] public float camDepthSpawnOffset = -10;
    [Range(0.5f, 5)] public float lookSpeed = 2f;
    [Range(3, 25)] public float moveSpeed = 10f;
    [Range(1, 10)] public float verticalSpeed = 5f;

    [Header("Boden")]
    public GameObject bodenPrefab;
    [Range(50, 1000)] public int bodenPufferX = 100;
    [Range(50, 1000)] public int bodenPufferZ = 100;

    [Header("Regal Einstellungen")]
    public GameObject regalPrefab;
    public Color regalFarbe;
    [Range(2, 50)] public int regalAnzahl = 3;
    [Range(0.1f, 5.0f)] public float regalBreite = 2.0f;
    [Range(1f, 100.0f)] public float regalHoehe = 6.0f;
    [Range(1f, 100.0f)] public float regalLaenge = 10.0f;
    [Range(0.1f, 10.0f)] public float regalAbstand = 6.0f;

    [Header("Fächer Einstellungen")]
    public GameObject fachPrefab;
    [Range(0.001f, 2.0f)] public float fach_size = 1f;
    [Range(0.001f, 0.5f)] public float fachTiefe = 0.1f;
    [Range(0.001f, 4.0f)] public float fachAbstand = 2.0f;
    public Color fachLeer = Color.green;

    private List<GameObject> regale = new List<GameObject>();


    private GameObject roboterParent;
    private GameObject bodenParent;
    private GameObject regalParent;
    private GameObject roboterBodenParent;

    private List<GameObject> alle_roboter = new List<GameObject>();


    // Ganz oben in der Manager-Klasse (als neues Feld)
    private Dictionary<int, bool> aufzugLocks = new Dictionary<int, bool>();

    // In Start() oder Awake() initialisieren (hier in Awake() vor dem Spawn)
    void Awake()
    {
        // Für jede Gasse (hier nehmen wir an, dass die Gassen von 0 bis regalAnzahl-1 gehen)
        for (int i = 0; i < regalAnzahl; i++)
        {
            aufzugLocks[i] = false;
        }
    }

    // Reserviert den Aufzug in der gegebenen Gasse.
    // Liefert true, wenn reserviert werden konnte, andernfalls false.
    public bool ReserveAufzug(int gasse)
    {
        if (!aufzugLocks[gasse])
        {
            aufzugLocks[gasse] = true;
            return true;
        }
        return false;
    }

    // Gibt den Aufzug in der gegebenen Gasse wieder frei.
    public void ReleaseAufzug(int gasse)
    {
        aufzugLocks[gasse] = false;
    }


    public List<GameObject> get_regale() {  return regale; }
    public void random_einsortieren(List<Ware> items)
    {
        int ware_anzahl = 0;
        for (int i = 0; i < items.Count; i++) {
            ware_anzahl += items[i].anzahl; 
        }
        if (ware_anzahl > faecher_count)
        {
            Debug.LogError("ACHTUNG: LAGER IST ZU KLEIN FÜR DIE MENGE AN WARE!");
            Debug.LogError("Ware anzahl:" + ware_anzahl);
            Debug.LogError("fächer anzahl:" + faecher_count);

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
                    Fach fach = alle_faecher[UnityEngine.Random.Range(0, alle_faecher.Count)];

                    fach.setItem(item.name, item.farbe);
                    

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


    private IEnumerator robot_test()
    {
            int etage = UnityEngine.Random.Range(0, Mathf.FloorToInt(regale[0].gameObject.GetComponent<Regal>().get_faecher_y()));
            int fach = UnityEngine.Random.Range(0, Mathf.FloorToInt(regale[0].gameObject.GetComponent<Regal>().get_faecher_z()));
            int regal = UnityEngine.Random.Range(0, regalAnzahl);
            int seite = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            

            yield return StartCoroutine(send_robot(regal, etage, fach, seite));
        
    }
    public bool ist_etage_besetzt(int gasse, int etage)
    {
        foreach(GameObject roboter in alle_roboter)
        {
            Roboter robo = roboter.GetComponent<Roboter>();
            if (robo.get_etage() == etage && robo.get_gasse() == gasse && !robo.ist_frei())
            {
                return true;
            }
        }
        return false;
    }

    public bool ist_aufzug_besetzt(int gasse)
    {
        foreach (GameObject roboter in alle_roboter)
        {
            Roboter robo = roboter.GetComponent<Roboter>();
            if (robo.is_currently_changing_etage && robo.get_gasse() == gasse)
            {
                return true;
            }
        }
        return false;
    }


    public IEnumerator send_robot(int regalIdx, int etage, int fachIdx, int seite)
    {
        int gasse = (regalIdx == 0) ? 0 : (regalAnzahl - 1 == regalIdx ? regalIdx - 1 : (seite == -1 ? regalIdx - 1 : regalIdx));

        List<Roboter> possible_roboter = new List<Roboter>();
        foreach (GameObject roboter in alle_roboter)
        {
            Roboter robo = roboter.GetComponent<Roboter>();
            if (robo.get_gasse() == gasse && robo.ist_frei())
            {
                possible_roboter.Add(robo);
            }
        }

        if (possible_roboter.Count > 0 && !ist_etage_besetzt(gasse, etage))
        {
            int bestDiff = 100000;
            Roboter bestRobo = possible_roboter[0];
            foreach (Roboter roboter in possible_roboter)
            {
                int robo_etage = Mathf.RoundToInt(roboter.get_etage());
                int diff = Mathf.Abs(etage - robo_etage);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestRobo = roboter;
                }
            }

            // Falls der Auftrag einen Etagenwechsel braucht
            if (Mathf.RoundToInt(bestRobo.get_etage()) != etage)
            {
                // versuche zu reservieren, falls nicht mögich => warteschlange
                if (!ReserveAufzug(gasse))

                {
                    auftragsWarteschlange.Enqueue(new RobotOrder(regalIdx, etage, fachIdx, seite));
                    yield break;
                }
                else
                {
                    // reservierung nur als test - der roboter reserviert in change etage selber, aufzug also wieder freigeben
                    ReleaseAufzug(gasse);
                }
            }

            if (!bestRobo.AssignTask())
            {
                auftragsWarteschlange.Enqueue(new RobotOrder(regalIdx, etage, fachIdx, seite));
                yield break;
            }
            yield return StartCoroutine(bestRobo.go_to_fach(etage, fachIdx, seite));
        }
        else
        {
            auftragsWarteschlange.Enqueue(new RobotOrder(regalIdx, etage, fachIdx, seite));
            yield break;
        }

        yield return StartCoroutine(probiere_warteschlange_abzuarbeiten());
    }



    private IEnumerator probiere_warteschlange_abzuarbeiten()
    {
        while (auftragsWarteschlange.Count > 0)
        {
            RobotOrder order = auftragsWarteschlange.Dequeue();
            yield return StartCoroutine(send_robot(order.RegalIdx, order.Etage, order.FachIdx, order.Seite));
            

            Debug.Log("Wartende Aufträge: " + auftragsWarteschlange.Count);
        }
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
        


        if (sortierungsMode == Mode.Random)
        {
            random_einsortieren(warenListe);
        }
        if (sortierungsMode == Mode.Smart)
        {
            smart_einsortieren(warenListe);
        }

        Debug.Log("Lager Kapazität: " + faecher_count);
        for (int i = 0; i < 100000; i++)
        { 
            StartCoroutine(robot_test());
        }
    }


    public IEnumerator make_item_drop(Color color, Vector3 pos)
    {
        GameObject instance = Instantiate(itemPrefab, pos, Quaternion.identity);
        Renderer r = instance.GetComponent<Renderer>();
        r.material.color = color;
        

        BoxCollider collider = instance.GetComponent<BoxCollider>();
        collider.enabled = item_hitbox;
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, -roboterSpeed / 4, -2); // -2 damit der block bisschen weg fliegt und nicht wieder ins regal fällt
        if (item_drop_rotation)
        {
            rb.angularVelocity = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f)); // Zufällige Rotation
        }



        while (instance.transform.position.y > -1)
        {
            yield return null;
        }
        Destroy(instance);
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


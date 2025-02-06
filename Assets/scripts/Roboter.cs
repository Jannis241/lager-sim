using System.Collections;
using UnityEngine;

public class Roboter : MonoBehaviour
{
    private Manager manager;
    private int regal_reihe;
    private float eine_fach_unit_size;

    public Vector2 current_fach_position;
    public Vector3 target_fach_position; // x: etage, y: fach index, z: seite (-1,1)
    public bool is_currently_changing_etage = false;

    private bool hat_auftrag;

    public float speed;
    private bool isMoving = false;

    public bool ist_frei()
    {
        return !hat_auftrag;
    }

    public int get_gasse() { return regal_reihe; }
    public float get_etage()
    {
        return isMoving ? target_fach_position.x : current_fach_position.x;
    }
    public void init(int regal_reihe_, int roboter_etage_)
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        speed = manager.roboterSpeed;
        current_fach_position = new Vector2(roboter_etage_, 0);
        regal_reihe = regal_reihe_;
        target_fach_position = Vector3.zero;
        eine_fach_unit_size = manager.fach_size + manager.fachAbstand;
        hat_auftrag = false;
    }


    public IEnumerator go_to_front()
    {
        target_fach_position = new Vector3(current_fach_position.x, - 1, 0);
        yield return StartCoroutine(MoveToTarget());
    }

    public IEnumerator change_etage(int etage_)
    {
        if (!manager.ReserveAufzug(regal_reihe))
        {
            Debug.LogWarning("Aufzug in Gasse " + regal_reihe + " belegt. Abbruch des Etagenwechsels.");
            yield break;
        }

        GameObject aufzug = transform.Find("Aufzug").gameObject;

        aufzug.SetActive(true);

        is_currently_changing_etage = true;
        target_fach_position = new Vector3(etage_, current_fach_position.y, 0);
        yield return StartCoroutine(MoveToTarget());


        // aktuelle position direkt updaten damit es nur einen roboter in der ziel gasse geben kann
        current_fach_position.x = etage_;
        is_currently_changing_etage = false;

        // Aufzug wieder freigeben
        manager.ReleaseAufzug(regal_reihe);
        aufzug.SetActive(false);
    }



    public IEnumerator go_to_fach_idx(int fachIdx)
    {
        target_fach_position = new Vector3(current_fach_position.x, fachIdx,0);
        yield return StartCoroutine(MoveToTarget());
    }

    public IEnumerator go_to_seite(int seite)
    {
        target_fach_position = new Vector3(current_fach_position.x, current_fach_position.y,-seite);
        yield return StartCoroutine(MoveToTarget());
    }


    public IEnumerator go_to_fach(int etage, int fach_idx, int seite)
    {
        // Der Auftrag wurde bereits vergeben (hat_auftrag ist true)
        if (etage != current_fach_position.x)
        {
            yield return StartCoroutine(go_to_front()); // Erst nach vorne fahren
            yield return StartCoroutine(change_etage(etage)); // Dann Etage wechseln
        }

        yield return StartCoroutine(go_to_fach_idx(fach_idx)); // Dann zum Fach fahren
        int regalIdx;
        int faecher_in_einer_reihe;
        Fach fach;
        if (seite == -1)
        {
            regalIdx = regal_reihe + 1;
            Regal regal = manager.get_regale()[regalIdx].GetComponent<Regal>();
            faecher_in_einer_reihe = regal.get_faecher_z();
            fach = regal.get_faecher_links()[fach_idx + etage * faecher_in_einer_reihe].GetComponent<Fach>();
        }
        else
        {
            regalIdx = regal_reihe;
            Regal regal = manager.get_regale()[regalIdx].GetComponent<Regal>();
            faecher_in_einer_reihe = regal.get_faecher_z();
            fach = regal.get_faecher_rechts()[fach_idx + etage * faecher_in_einer_reihe].GetComponent<Fach>();
        }

        yield return new WaitForSeconds(manager.roboterSleepDelay / manager.roboterSpeed);
        yield return StartCoroutine(go_to_seite(seite));
        yield return new WaitForSeconds(manager.roboterBeladungsDelay / manager.roboterSpeed);

        GameObject carried_item = transform.Find("Carried_item").gameObject;
        Renderer r = carried_item.GetComponent<MeshRenderer>();
        r.material.color = fach.getItemFarbe();

        bool hat_item = false;
        if (!fach.istFrei())
        {
            hat_item = true;
            carried_item.SetActive(true);
            fach.auslagern();
        }

        yield return StartCoroutine(go_to_seite(-seite));
        yield return new WaitForSeconds(manager.roboterSleepDelay / manager.roboterSpeed);
        yield return StartCoroutine(go_to_front()); // Gehe nach vorne, um zu entladen
        yield return new WaitForSeconds(manager.roboterBeladungsDelay / manager.roboterSpeed);
        carried_item.SetActive(false);
        if (hat_item)
        {
            StartCoroutine(manager.make_item_drop(r.material.color, transform.position));
        }
        yield return StartCoroutine(go_to_fach_idx(0)); // Dann zurück ins Regal

        hat_auftrag = false;
    }

    public bool AssignTask()
    {
        if (hat_auftrag) return false;
        hat_auftrag = true;
        return true;
    }



    private IEnumerator MoveToTarget()
    {
        isMoving = true;
        float target_x = target_fach_position.z + transform.position.x;
        Vector2 target_fach_pos_vec2 = new Vector2(target_fach_position.x, target_fach_position.y);   
        while (current_fach_position != target_fach_pos_vec2 || Mathf.Abs(transform.position.x - target_x) > 0.01f)
        {
            Vector3 targetWorldPosition = new Vector3(target_x,
                                                      (target_fach_position.x - current_fach_position.x) * eine_fach_unit_size + transform.position.y,
                                                      (target_fach_position.y - current_fach_position.y) * eine_fach_unit_size + transform.position.z);

            while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPosition, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPosition;
            current_fach_position = target_fach_position;

            yield return new WaitForSeconds(manager.roboterSleepDelay / manager.roboterSpeed);
        }

        isMoving = false;
    }









}


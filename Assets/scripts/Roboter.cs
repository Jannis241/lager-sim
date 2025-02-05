using System.Collections;
using UnityEngine;

public class Roboter : MonoBehaviour
{
    private Manager manager;
    private int regal_reihe;
    private float eine_fach_unit_size;

    public Vector2 current_fach_position;
    public Vector3 target_fach_position; // x: etage, y: fach index, z: seite (-1,1)

    private bool hat_auftrag;

    public float speed;
    private bool isMoving = false;

    public bool ist_frei()
    {
        return !hat_auftrag;
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

    public bool hat_schon_auftrag() { return hat_auftrag; }

    public IEnumerator go_to_front()
    {
        target_fach_position = new Vector3(current_fach_position.x, - 1, 0);
        yield return StartCoroutine(MoveToTarget());
    }

    public IEnumerator change_etage(int etage_)
    {
        target_fach_position = new Vector3(etage_, current_fach_position.y, 0);
        yield return StartCoroutine(MoveToTarget());
    }

    public IEnumerator go_to_fach_idx(int fachIdx)
    {
        target_fach_position = new Vector3(current_fach_position.x, fachIdx,0);
        yield return StartCoroutine(MoveToTarget());
    }

    public IEnumerator go_to_seite(int seite)
    {
        target_fach_position = new Vector3(current_fach_position.x, current_fach_position.y,seite);
        yield return StartCoroutine(MoveToTarget());
    }


    public IEnumerator go_to_fach(int etage, int fach_idx, int seite)
    {
        if (!hat_auftrag)
        {
            hat_auftrag = true;

            // die gewollte etage ist nicht die aktuelle etage => erst nach vorne fahren, dann noch oben um die etage zu wechesln
            if (etage != current_fach_position.x)
            {
                yield return StartCoroutine(go_to_front()); // Erst nach vorne fahren
                yield return StartCoroutine(change_etage(etage)); // Dann Etage wechseln
            }

            yield return StartCoroutine(go_to_fach_idx(fach_idx)); // Dann zum Fach fahren
            int regalIdx;

            int faecher_in_einer_reihe;

            Fach fach;
            if (seite == 1)
            {
                regalIdx = regal_reihe + 1; 
                Regal regal = manager.get_regale()[regalIdx].GetComponent<Regal>();
                faecher_in_einer_reihe = regal.get_faecher_z();
                fach = regal.get_faecher_links()[fach_idx + etage * faecher_in_einer_reihe].GetComponent<Fach>();
                Debug.Log("Das Fach in Etage " + etage + " an dem fach Index " + fach_idx + " aus dem " + regalIdx +". regal in der linken Regal Hälfte (umgerechnet zu " + (fach_idx + etage * faecher_in_einer_reihe) + ") hat den Namen " + fach.getItemName() + "und den RGBA code " + fach.getItemFarbe() + ". Eine Reihe besitzt " + faecher_in_einer_reihe + " Fächer.");
                Debug.Log("Koordinaten des angeblich gefundenen Fachs: " + fach.getRegalIdx() + " | " + fach.getFachIdx() + " | " + fach.get_etage());
            }

            else
            {
                regalIdx = regal_reihe;
                Regal regal = manager.get_regale()[regalIdx].GetComponent<Regal>();
                faecher_in_einer_reihe = regal.get_faecher_z();
                fach = regal.get_faecher_rechts()[fach_idx + etage * faecher_in_einer_reihe].GetComponent<Fach>();
                Debug.Log("Das Fach in Etage " + etage + " an dem fach Index " + fach_idx + " aus dem " + regalIdx +". regal in der rechten Regal Hälfte (umgerechnet zu " + (fach_idx + etage * faecher_in_einer_reihe) + ") hat den Namen " + fach.getItemName() + "und den RGBA code " + fach.getItemFarbe() + ". Eine Reihe besitzt " + faecher_in_einer_reihe + " Fächer.");
                Debug.Log("Koordinaten des angeblich gefundenen Fachs: " + fach.getRegalIdx() + " | " + fach.getFachIdx() + " | " + fach.get_etage());
            }


            Debug.Log(fach.getItemName() + " | " + fach.getItemFarbe());



            //fach.auslagern();

            yield return new WaitForSeconds(manager.roboterSleepDelay / manager.roboterSpeed);
            yield return StartCoroutine(go_to_seite(seite)); // Dann zum Fach fahren
            yield return new WaitForSeconds(manager.roboterBeladungsDelay / manager.roboterSpeed);










            yield return StartCoroutine(go_to_seite(-seite)); // Dann zum Fach fahren
            yield return new WaitForSeconds(manager.roboterSleepDelay / manager.roboterSpeed);

            hat_auftrag = false;

        }

        else
        {
            Debug.LogWarning("Roboter ist bereits beauftragt.");
        }
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


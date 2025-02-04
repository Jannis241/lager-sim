using System.Collections;
using UnityEngine;

public class Roboter : MonoBehaviour
{
    private Manager manager;
    private bool hat_ware;
    private int regal_reihe;
    private float eine_fach_unit_size;

    public Vector2 current_fach_position;
    public Vector2 target_fach_position;

    public float speed;
    private bool isMoving = false;

    public bool ist_frei()
    {
        return !hat_ware;
    }

    public void init(int regal_reihe_, int roboter_etage_)
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        speed = manager.roboterSpeed;
        hat_ware = false;
        current_fach_position = new Vector2(roboter_etage_, 0);
        regal_reihe = regal_reihe_;
        target_fach_position = Vector2.zero;
        eine_fach_unit_size = manager.fach_size + manager.fachAbstand; 
    }

    public void go_to_fach(int etage, int fach_idx)
    {
        target_fach_position = new Vector2(etage, fach_idx);
        if (!isMoving) StartCoroutine(MoveToTarget());
    }
    private IEnumerator MoveToTarget()
    {
        isMoving = true;

        while (current_fach_position != target_fach_position)
        {
            Vector3 targetWorldPosition = new Vector3(transform.position.x, // eventuell später noch ändern dass der nach rechts oder lins
                                                                            // fährt jenachdem welche reihe er hat
                                                      (target_fach_position.x - current_fach_position.x) * (manager.fach_size + manager.fachAbstand) + transform.position.y, // y
                                                      (target_fach_position.y - current_fach_position.y) * (manager.fach_size + manager.fachAbstand) + transform.position.z);

            while (Vector3.Distance(transform.position, targetWorldPosition) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, targetWorldPosition, speed * Time.deltaTime);
                yield return null;
            }

            current_fach_position = target_fach_position; 
        }

        isMoving = false;
    }
}

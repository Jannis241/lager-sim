using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    private float lookSpeed;
    private float moveSpeed;
    private float verticalSpeed;

    private float pitch = 0.0f; // Vertikale Rotation
    private float yaw = 0.0f;   // Horizontale Rotation

    public void init(float ls, float ms, float vs)
    {
        lookSpeed = ls;
        moveSpeed = ms;
        verticalSpeed = vs;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // Mausbewegung erfassen und horizontale/vertikale Rotation anpassen
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        yaw += mouseX;  // Horizontale Rotation (Rechts/Links)
        pitch -= mouseY; // Vertikale Rotation (Oben/Unten)

        // Begrenzung der vertikalen Rotation, um Überschläge zu vermeiden
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Anwenden der neuen Rotation auf die Kamera
        transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);

        // Bewegung (WASD + Leertaste/Strg)
        MoveCamera();
    }

    // Bewegung der Kamera (WASD und vertikal mit Leertaste/Strg)
    private void MoveCamera()
    {
        // WASD Bewegungsachsen
        float moveX = Input.GetAxis("Horizontal");  // A/D
        float moveZ = Input.GetAxis("Vertical");    // W/S

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Vertikale Bewegung (Leertaste nach oben, Strg nach unten)
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection += Vector3.down;
        }

        // Position der Kamera aktualisieren (ohne Rotation)
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}

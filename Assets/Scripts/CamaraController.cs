using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public GameObject jugador;

    public float sensibilidad = 3f;
    public float distancia = 8f;          // distancia al jugador
    public float alturaMirada = 1f;       // punto al que mira (un poco arriba del jugador)
    public float anguloMinY = -20f;       // límite al mirar hacia abajo
    public float anguloMaxY = 70f;        // límite al mirar hacia arriba

    private float rotacionX = 0f;         // giro horizontal (360°)
    private float rotacionY = 20f;        // giro vertical

    void Start()
    {
        // Oculta y bloquea el cursor en el centro (Esc para liberarlo)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Toma el ángulo inicial a partir de la posición actual de la cámara
        Vector3 angulos = transform.eulerAngles;
        rotacionX = angulos.y;
        rotacionY = angulos.x;
        if (rotacionY > 180f) rotacionY -= 360f;
        distancia = Vector3.Distance(transform.position, jugador.transform.position);
    }

    void LateUpdate()
    {
        rotacionX += Input.GetAxis("Mouse X") * sensibilidad;
        rotacionY -= Input.GetAxis("Mouse Y") * sensibilidad;
        rotacionY = Mathf.Clamp(rotacionY, anguloMinY, anguloMaxY);

        Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0f);
        Vector3 objetivo = jugador.transform.position + Vector3.up * alturaMirada;

        transform.position = objetivo - rotacion * Vector3.forward * distancia;
        transform.rotation = rotacion;
    }
}
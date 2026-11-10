using UnityEngine;
using UnityEngine.UI;
public class movimiento : MonoBehaviour
{
private Rigidbody rb; // Creamos el objeto Rigidbody
private float velocidad = 10; // Variable para controlar la velocidad (Paso 30)
// Start is called before the first frame update
private int contador_coleccionables = 0; // Variable para contar los objetos coleccionables
public Text puntaje;
void Start()
{
rb = GetComponent<Rigidbody>();
}
// Update is called once per frame
void Update()
{
}
// Para sincronizar los frames del motor de la física
private void FixedUpdate()
{
// Variables para capturar el movimiento horizontal y vertical en nuestro teclado
float movimientoH = Input.GetAxis("Horizontal");
float movimientoV = Input.GetAxis("Vertical");
// Un Vector3 es un trio de posiciones en X, Y, Z (0.0f en Y porque no seeleva)
Vector3 dirMovimiento = new Vector3(movimientoH, 0.0f, movimientoV);
// Asignamos el movimiento al rigidbody multiplicado por la velocidad
rb.AddForce(dirMovimiento * velocidad);
}
private void OnTriggerEnter(Collider other)
{
    
    if (other.gameObject.CompareTag("Coleccionable"))
    {
        other.gameObject.SetActive(false); // Desactivamos el objeto coleccionable
        contador_coleccionables++; // Incrementamos el contador de coleccionables
        Debug.Log("Coleccionables recogidos: " + contador_coleccionables); // Mostramos el contador en la consola
        puntaje.text = contador_coleccionables + "";
    }
}
}
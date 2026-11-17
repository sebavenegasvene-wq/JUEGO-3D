using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class movimiento : MonoBehaviour
{
    private Rigidbody rb;
    private int contador_coleccionables = 0;
    public Text puntaje;

    // Movimiento con aceleración
    public float velocidadCaminar = 8f;
    public float velocidadCorrer = 14f;
    public float aceleracion = 30f;
    public float frenado = 15f;
    public float controlEnAire = 0.5f;

    // Impulso de velocidad (Coleccionable2)
    public float multiplicadorBoost = 1.5f;   // 1.5 = 50% más rápido
    public float duracionBoost = 5f;          // segundos que dura
    public GameObject contenedorBarra;        // fondo de la barra
    public Image barraRelleno;                // la barra que se vacía (Image en modo Filled)
    private float tiempoBoost = 0f;

    // Salto
    public float fuerzaSalto = 8f;
    public int saltosMaximos = 2;
    private int saltosRestantes;
    private bool enSuelo = false;

    // Sonidos
    public AudioClip sonidoDoh;
    [Range(0f, 1f)] public float volumen = 1f;
    public AudioClip sonidoColeccionable2;
    [Range(0f, 1f)] public float volumen2 = 1f;

    // Victoria
    public GameObject cilindroWin;
    public GameObject textoGanaste;
    public float segundosParaReiniciar = 5f;   // NUEVO: cuenta invisible tras ganar
    private int totalMonedas;
    private bool juegoGanado = false;

    // tp
    public GameObject portalTP;
    public int monedasParaTP = 5;
    private Vector3 destinoTP = new Vector3(-168.9687f, -1105.54f, 17.68513f);

    private bool reiniciando = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        saltosRestantes = saltosMaximos;

        foreach (GameObject zona in GameObject.FindGameObjectsWithTag("kill"))
        {
            Renderer r = zona.GetComponent<Renderer>();
            if (r != null) r.enabled = false;
        }

        totalMonedas = GameObject.FindGameObjectsWithTag("Coleccionable").Length
                     + GameObject.FindGameObjectsWithTag("Coleccionable2").Length;
        Debug.Log("Monedas totales en el mapa: " + totalMonedas);

        if (cilindroWin != null) cilindroWin.SetActive(false);
        else Debug.LogWarning("Falta asignar Cilindro Win en el Jugador");

        if (textoGanaste != null) textoGanaste.SetActive(false);
        else Debug.LogWarning("Falta asignar Texto Ganaste en el Jugador");

        if (portalTP != null) portalTP.SetActive(false);
        else Debug.LogWarning("Falta asignar Portal TP en el Jugador");

        if (contenedorBarra != null) contenedorBarra.SetActive(false);
        else Debug.LogWarning("Falta asignar Contenedor Barra en el Jugador");
    }

    void Update()
    {
        // Salto y doble salto
        if (Input.GetKeyDown(KeyCode.Space) && saltosRestantes > 0)
        {
            Vector3 v = rb.linearVelocity;
            v.y = fuerzaSalto;
            rb.linearVelocity = v;
            saltosRestantes--;
            enSuelo = false;
        }

        ActualizarBoost();
    }

    // Descuenta el tiempo y vacía la barra
    private void ActualizarBoost()
    {
        if (tiempoBoost <= 0f) return;

        tiempoBoost -= Time.deltaTime;

        if (barraRelleno != null)
        {
            barraRelleno.fillAmount = Mathf.Clamp01(tiempoBoost / duracionBoost);
        }

        if (tiempoBoost <= 0f && contenedorBarra != null)
        {
            contenedorBarra.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 adelante = cam.forward;
        Vector3 derecha = cam.right;
        adelante.y = 0f;
        derecha.y = 0f;
        adelante.Normalize();
        derecha.Normalize();

        Vector3 dir = Vector3.ClampMagnitude(adelante * v + derecha * h, 1f);

        float velocidadMax = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        // Con el impulso activo sube la velocidad máxima y la aceleración
        float multiplicador = (tiempoBoost > 0f) ? multiplicadorBoost : 1f;
        velocidadMax *= multiplicador;

        Vector3 velActual = rb.linearVelocity;
        velActual.y = 0f;

        Vector3 velObjetivo = dir * velocidadMax;

        float cambio = (dir.sqrMagnitude > 0.01f) ? aceleracion * multiplicador : frenado;
        if (!enSuelo) cambio *= controlEnAire;

        Vector3 velNueva = Vector3.MoveTowards(velActual, velObjetivo, cambio * Time.fixedDeltaTime);

        velNueva.y = rb.linearVelocity.y;
        rb.linearVelocity = velNueva;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("pizo")) return;

        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.5f)
            {
                enSuelo = true;
                if (rb.linearVelocity.y <= 0.1f) saltosRestantes = saltosMaximos;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("pizo"))
        {
            if (enSuelo && saltosRestantes == saltosMaximos && rb.linearVelocity.y <= 0.1f)
            {
                saltosRestantes = saltosMaximos - 1;
            }
            enSuelo = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("win")) Ganar();
        else if (collision.gameObject.CompareTag("kill"))
        {
            Debug.Log("Reinicio por kill: " + collision.gameObject.name);
            ReiniciarJuego();
        }
        else if (collision.gameObject.CompareTag("tp"))
        {
            Debug.Log("Teletransporte por tp: " + collision.gameObject.name);
            Teletransportar();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coleccionable"))
        {
            AudioSource.PlayClipAtPoint(sonidoDoh, transform.position, volumen);
            RecogerMoneda(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Coleccionable2"))
        {
            AudioSource.PlayClipAtPoint(sonidoColeccionable2, transform.position, volumen2);
            RecogerMoneda(other.gameObject);
            ActivarBoost();
        }
        else if (other.gameObject.CompareTag("win")) Ganar();
        else if (other.gameObject.CompareTag("kill"))
        {
            Debug.Log("Reinicio por kill: " + other.gameObject.name);
            ReiniciarJuego();
        }
        else if (other.gameObject.CompareTag("tp"))
        {
            Debug.Log("Teletransporte por tp: " + other.gameObject.name);
            Teletransportar();
        }
    }

    // Activa (o renueva) el impulso y muestra la barra llena
    private void ActivarBoost()
    {
        tiempoBoost = duracionBoost;

        if (contenedorBarra != null) contenedorBarra.SetActive(true);
        if (barraRelleno != null) barraRelleno.fillAmount = 1f;
    }

    private void RecogerMoneda(GameObject moneda)
    {
        moneda.SetActive(false);
        contador_coleccionables++;
        Debug.Log("Coleccionables recogidos: " + contador_coleccionables + " de " + totalMonedas);
        puntaje.text = contador_coleccionables + "";

        if (contador_coleccionables >= monedasParaTP && portalTP != null)
        {
            portalTP.SetActive(true);
        }

        if (contador_coleccionables >= totalMonedas && cilindroWin != null)
        {
            cilindroWin.SetActive(true);
        }
    }

    private void ReiniciarJuego()
    {
        if (reiniciando) return;
        reiniciando = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Teletransportar()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = destinoTP;
    }

    private void Ganar()
    {
        if (juegoGanado) return;
        juegoGanado = true;

        if (textoGanaste != null) textoGanaste.SetActive(true);
        Debug.Log("¡Ganaste!");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // NUEVO: cuenta invisible, al terminar reinicia todo el juego
        Invoke(nameof(ReiniciarJuego), segundosParaReiniciar);
    }
}
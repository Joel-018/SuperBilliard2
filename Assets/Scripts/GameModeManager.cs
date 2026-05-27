using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum ModoPruebaEvento 
{ 
    Aleatorio, 
    Tornado, 
    Villano, 
    Gravedad 
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    [Header("Configuración Principal")]
    public float timeRemaining = 30f;
    public int totalBalls;      
    private bool gameEnded = false;

    // --- NUEVA VARIABLE PARA EL TOPE DE TIEMPO ---
    private float tiempoMaximo; 

    [Header("Pruebas y Debug")]
    [Tooltip("Elige qué evento saldrá. Déjalo en 'Aleatorio' para jugar una partida normal.")]
    public ModoPruebaEvento forzarEvento = ModoPruebaEvento.Aleatorio;

    private int bolasIniciales;
    private bool eventoYaLanzado = false;
    
    [Header("Configuración de Eventos")]
    public GameObject[] prefabsDeBolas; 
    public GameObject manoVillanoPrefab; 
    public Transform centroDelTablero; 
    
    public float fuerzaDelTornado = 30f; 
    public float duracionTornado = 5f;
    public float separacionBolasVillano = 3f;

    [Header("Audios de Eventos")]
    public AudioClip sonidoTornado;
    public AudioClip sonidoVillano;
    public AudioClip sonidoGravedad;
    private AudioSource altavoz;

    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultMessageText;
    public TextMeshProUGUI eventMessageText; 

    [Header("Barra Visual")]
    public Slider timerBar;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalBalls = GameObject.FindGameObjectsWithTag("Ball").Length;
        bolasIniciales = totalBalls; 
        
        // GUARDAMOS EL TIEMPO INICIAL COMO EL LÍMITE MÁXIMO
        tiempoMaximo = timeRemaining; 
        
        altavoz = GetComponent<AudioSource>();
        if (altavoz == null) altavoz = gameObject.AddComponent<AudioSource>();

        if (resultMessageText != null) resultMessageText.gameObject.SetActive(false);
        if (eventMessageText != null) eventMessageText.gameObject.SetActive(false);

        if (timerBar != null)
        {
            timerBar.maxValue = tiempoMaximo;
            timerBar.value = timeRemaining;
        }
    }

    void Update()
    {
        if (gameEnded) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Tiempo: " + Mathf.Ceil(timeRemaining).ToString() + "s";
            if (timerBar != null) timerBar.value = timeRemaining;
        }
        else
        {
            timeRemaining = 0;
            timerText.text = "Tiempo: 0s";
            if (timerBar != null) timerBar.value = 0;
            LoseGame();
        }
    }

    public void OnBallPotted()
    {
        if (gameEnded) return;

        // Sumamos 10 segundos por meter la bola
        timeRemaining += 10f;

        // --- LA MAGIA DEL TOPE DE TIEMPO ---
        // Si al sumar nos hemos pasado del tiempo inicial, lo recortamos
        if (timeRemaining > tiempoMaximo)
        {
            timeRemaining = tiempoMaximo;
        }

        totalBalls--;

        // Comprobación de evento a mitad de partida
        int bolasMetidas = bolasIniciales - totalBalls;
        
        if (!eventoYaLanzado && bolasMetidas >= (bolasIniciales / 2))
        {
            LanzarEventoAleatorio();
        }

        if (totalBalls <= 0)
        {
            GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
            if (bola8 != null)
            {
                bola8.GetComponent<BallMovement>().HacerSolidaParaJugador();
            }
        }
    }

    // ==========================================
    //          SISTEMA DE EVENTOS
    // ==========================================

    private void LanzarEventoAleatorio()
    {
        eventoYaLanzado = true;
        
        int eventoElegido;
        if (forzarEvento == ModoPruebaEvento.Aleatorio) eventoElegido = Random.Range(0, 3);
        else eventoElegido = ((int)forzarEvento) - 1; 

        switch (eventoElegido)
        {
            case 0:
                StartCoroutine(EventoTornado());
                break;
            case 1:
                StartCoroutine(EventoVillano());
                break;
            case 2:
                EventoGravedad();
                break;
        }
    }

    private IEnumerator MostrarMensajeEvento(string mensaje)
    {
        TextMeshProUGUI textoAMostrar = eventMessageText != null ? eventMessageText : resultMessageText;
        textoAMostrar.text = mensaje;
        textoAMostrar.color = Color.yellow;
        textoAMostrar.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f); 
        textoAMostrar.gameObject.SetActive(false);
    }

    private IEnumerator EventoTornado()
    {
        StartCoroutine(MostrarMensajeEvento("¡CUIDADO!\n¡Un Tornado Salvaje!"));
        if (sonidoTornado != null) altavoz.PlayOneShot(sonidoTornado);

        GameObject[] agujeros = GameObject.FindGameObjectsWithTag("Hole");
        foreach (GameObject h in agujeros)
        {
            Collider c = h.GetComponent<Collider>();
            if (c != null) c.enabled = false; 
        }

        float timer = 0f;
        while (timer < duracionTornado)
        {
            GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Ball");
            GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
            
            List<GameObject> bolasEnMesa = new List<GameObject>(todasLasBolas);
            if(bola8 != null) bolasEnMesa.Add(bola8);

            foreach (GameObject bola in bolasEnMesa)
            {
                Rigidbody rb = bola.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 fuerzaTornado = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(fuerzaTornado * fuerzaDelTornado * rb.mass, ForceMode.Impulse); 
                }
            }
            
            timer += 0.3f; 
            yield return new WaitForSeconds(0.3f);
        }

        foreach (GameObject h in agujeros)
        {
            Collider c = h.GetComponent<Collider>();
            if (c != null) c.enabled = true;
        }
    }

    private IEnumerator EventoVillano()
    {
        StartCoroutine(MostrarMensajeEvento("¡EL VILLANO HACE TRAMPAS!\nAñade 3 bolas nuevas"));
        if (sonidoVillano != null) altavoz.PlayOneShot(sonidoVillano);

        if (manoVillanoPrefab != null && centroDelTablero != null)
        {
            GameObject mano = Instantiate(manoVillanoPrefab, centroDelTablero.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(mano, 3f); 
        }

        yield return new WaitForSeconds(1.0f); 

        if (prefabsDeBolas != null && prefabsDeBolas.Length > 0 && centroDelTablero != null)
        {
            float dist = separacionBolasVillano;

            Vector3[] posicionesTriangulo = new Vector3[3] {
                new Vector3(0, 0, dist),               
                new Vector3(-dist, 0, -dist),          
                new Vector3(dist, 0, -dist)            
            };

            for (int i = 0; i < 3; i++)
            {
                int indiceAleatorio = Random.Range(0, prefabsDeBolas.Length);
                GameObject bolaAleatoria = prefabsDeBolas[indiceAleatorio];

                Vector3 posFinal = centroDelTablero.position + posicionesTriangulo[i];
                Instantiate(bolaAleatoria, posFinal, Quaternion.identity);
                totalBalls++; 
            }
        }
    }

    private void EventoGravedad()
    {
        StartCoroutine(MostrarMensajeEvento("¡ALERTA!\nLas bolas ahora son muy pesadas"));
        if (sonidoGravedad != null) altavoz.PlayOneShot(sonidoGravedad);

        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Ball");
        GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
        
        List<GameObject> bolasEnMesa = new List<GameObject>(todasLasBolas);
        if(bola8 != null) bolasEnMesa.Add(bola8);

        foreach (GameObject bola in bolasEnMesa)
        {
            Rigidbody rb = bola.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass *= 5f;         
                rb.linearDamping *= 3f;         
                rb.angularDamping *= 3f;  
            }
        }
    }

    public void WinGame()
    {
        gameEnded = true;
        resultMessageText.text = "¡GANASTE!";
        resultMessageText.color = Color.green;
        resultMessageText.gameObject.SetActive(true);
    }

    public void LoseGame(string mensaje = "¡PERDISTE!\nSe acabó el tiempo.")
    {
        gameEnded = true;
        resultMessageText.text = mensaje;
        resultMessageText.color = Color.red;
        resultMessageText.gameObject.SetActive(true);
    }
}
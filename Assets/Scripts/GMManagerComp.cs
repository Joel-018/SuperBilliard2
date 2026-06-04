using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GMManagerComp : MonoBehaviour
{
    public static GMManagerComp Instance;

    [Header("Configuración Principal")]
    public float timeRemaining = 30f;
    public int totalBalls;
    private bool gameEnded = false;
    private float tiempoMaximo;

    [Header("Puntuación Competitiva")]
    public int puntosP1 = 0;
    public int puntosP2 = 0;
    public bool faseMuerteSubita = false;

    [Header("Estado de Jugadores")]
    public bool isPlayer1Stunned = false;
    public bool isPlayer2Stunned = false;

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
    public int bolasVillano = 3;
    public float multiplicadorGravedad = 5f;
    public float duracionGravedad = 8f;

    [Header("Audios de Eventos")]
    public AudioClip sonidoTornado;
    public AudioClip sonidoVillano;
    public AudioClip sonidoGravedad;
    public AudioClip stunElectricSound;
    private AudioSource altavoz;

    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultMessageText; // se puede dejar vacío, ya no se usará
    public TextMeshProUGUI eventMessageText;

    [Header("Barra Visual")]
    public Slider timerBar;

    [Header("Audio Victoria/Derrota")]
    public AudioClip sonidoVictoria;
    public AudioClip sonidoEmpate;

    // ── Colores elegantes ────────────────────────────────────────────────────
    private static readonly Color ColDark = new Color(0.08f, 0.05f, 0.02f, 1f);
    private static readonly Color ColGold = new Color(0.85f, 0.68f, 0.25f, 1f);
    private static readonly Color ColGoldLight = new Color(1f, 0.90f, 0.55f, 1f);
    private static readonly Color ColGreen = new Color(0.10f, 0.38f, 0.18f, 1f);
    private static readonly Color ColRed = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color ColCream = new Color(0.95f, 0.92f, 0.82f, 1f);
    private static readonly Color ColYellow = new Color(1f, 0.85f, 0.10f, 1f);
    private static readonly Color ColOrange = new Color(1f, 0.55f, 0.10f, 1f);
    private static readonly Color ColGray = new Color(0.25f, 0.25f, 0.25f, 1f);

    // ── Referencias UI de resultado (Multi-Display) ────────────────────
    private List<GameObject> _resultRoots = new List<GameObject>();
    private List<Image> _overlays = new List<Image>();
    private List<Image> _panels = new List<Image>();
    private List<TextMeshProUGUI> _titleTMPs = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _subtitleTMPs = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _scoreTMPs = new List<TextMeshProUGUI>();

    // ── Referencias UI de Puntuación en vivo ────────────────────
    private List<TextMeshProUGUI> _textosP1 = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _textosP2 = new List<TextMeshProUGUI>();

    // ── Referencias UI barra de tiempo ──────────────────────────────────────────
    private RectTransform _timerFill;
    private TextMeshProUGUI _timerSeconds;
    private static readonly Color ColTimerHigh = new Color(0.15f, 0.75f, 0.25f, 1f); // verde
    private static readonly Color ColTimerMid = new Color(0.95f, 0.80f, 0.10f, 1f); // amarillo
    private static readonly Color ColTimerLow = new Color(0.85f, 0.15f, 0.10f, 1f); // rojo

    // ── Referencias UI de evento  ───────────────────────
    private GameObject _eventRoot;
    private Image _eventPanel;
    private TextMeshProUGUI _eventIcon;
    private TextMeshProUGUI _eventTitle;
    private TextMeshProUGUI _eventDesc;

    // ────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalBalls = GameObject.FindGameObjectsWithTag("Ball").Length + 1;
        bolasIniciales = totalBalls;
        tiempoMaximo = timeRemaining;

        altavoz = GetComponent<AudioSource>();
        if (altavoz == null) altavoz = gameObject.AddComponent<AudioSource>();

        // Ocultamos el texto antiguo de resultado
        if (resultMessageText != null) resultMessageText.gameObject.SetActive(false);
        if (eventMessageText != null) eventMessageText.gameObject.SetActive(false);

        if (timerBar != null)
        {
            timerBar.maxValue = tiempoMaximo;
            timerBar.value = timeRemaining;
        }

        BuildResultUI();
        BuildEventUI();
        BuildTimerUI();
        BuildScoreUI();
        ActualizarMarcadoresUI();
    }

    void Update()
    {
        if (gameEnded) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timerBar != null) timerBar.value = timeRemaining;
            UpdateTimerBar();
        }
        else if (!faseMuerteSubita)
        {
            // El tiempo ha llegado a 0. ¡Se acabó reaparecer bolas!
            timeRemaining = 0;
            faseMuerteSubita = true;

            if (timerBar != null) timerBar.value = 0;
            UpdateTimerBar();
        }
    }

    public void OnBallPottedComp(string tagBola, string tagJugador)
    {
        if (gameEnded) return;
        Debug.Log($"¡Bola en el agujero! Bola: {tagBola} | Último toque: '{tagJugador}'");

        // Solo procesamos puntos si la bola ha sido empujada por un jugador válido
        bool esJugadorValido = (tagJugador == "Player1" || tagJugador == "Player2");

        if (tagBola == "Ball")
        {
            if (esJugadorValido)
            {
                if (tagJugador == "Player1") puntosP1 += 1;
                else puntosP2 += 1;
                Debug.Log($"¡Punto para {tagJugador}! (Bola normal)");
            }
        }
        else if (tagBola == "Ball8")
        {
            if (esJugadorValido)
            {
                // Si la mete el último de la mesa en muerte súbita, suma 2. Si no, resta 2.
                if (faseMuerteSubita && totalBalls == 1) // Solo queda la bola 8 en la mesa
                {
                    if (tagJugador == "Player1") puntosP1 += 2;
                    else puntosP2 += 2;
                    Debug.Log($"¡GOLPE MAESTRO! {tagJugador} mete la Bola 8 al final. +2 puntos.");
                }
                else
                {
                    if (tagJugador == "Player1")
                        puntosP1 = Mathf.Max(0, puntosP1 - 2);
                    else
                        puntosP2 = Mathf.Max(0, puntosP2 - 2);

                    Debug.Log($"¡PENALIZACIÓN! {tagJugador} metió la Bola 8 antes de tiempo. -2 puntos (Mínimo 0).");
                }
            }
        }

        // Si estamos en Muerte Súbita, las bolas se destruyen y el contador baja
        if (faseMuerteSubita)
        {
            totalBalls--;
            Debug.Log($"Quedan {totalBalls}");
            // Control de eventos normales en mitad de partida
            int bolasMetidas = bolasIniciales - totalBalls;
            if (!eventoYaLanzado && bolasMetidas >= (bolasIniciales / 2))
                LanzarEventoAleatorio();

            // Si ya no quedan más bolas en la mesa, evaluamos el final por puntos
            if (totalBalls <= 0)
            {
                TerminarPartidaPorPuntos();
            }
        }
        
        ActualizarMarcadoresUI();
    }

    // ==========================================
    //          VICTORIA / DERROTA
    // ==========================================

    private void TerminarPartidaPorPuntos()
    {
        if (gameEnded) return;
        gameEnded = true;

        string mensajeFinal = "";

        if (puntosP1 > puntosP2)
        {
            if (sonidoVictoria != null) altavoz.PlayOneShot(sonidoVictoria);
            mensajeFinal = $"PLAYER 1 WINS!\nScore: {puntosP1} vs {puntosP2}";
            StartCoroutine(ShowResult(true, 0, mensajeFinal));
        }
        else if (puntosP2 > puntosP1)
        {
            if (sonidoVictoria != null) altavoz.PlayOneShot(sonidoVictoria);
            mensajeFinal = $"PLAYER 2 WINS!\nScore: {puntosP2} vs {puntosP1}";
            StartCoroutine(ShowResult(true, 0, mensajeFinal));
        }
        else
        {
            if (sonidoEmpate != null) altavoz.PlayOneShot(sonidoEmpate);
            mensajeFinal = $"IT'S A TIE!\nBoth players got {puntosP1} points!";
            // Usamos false para que el panel salga en color rojo/crema de empate
            StartCoroutine(ShowResult(false, 0, mensajeFinal));
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
            case 0: StartCoroutine(EventoTornado()); break;
            case 1: StartCoroutine(EventoVillano()); break;
            case 2: StartCoroutine(EventoGravedad()); break;
        }
    }

    private IEnumerator MostrarMensajeEvento(string icono, string titulo, string desc, Color color)
    {
        _eventRoot.SetActive(true);
        _eventPanel.color = color.WithAlpha(0f);
        _eventIcon.text = icono;
        _eventTitle.text = titulo;
        _eventDesc.text = desc;
        _eventIcon.color = ColCream.WithAlpha(0f);
        _eventTitle.color = ColCream.WithAlpha(0f);
        _eventDesc.color = ColCream.WithAlpha(0f);

        // Slide desde arriba
        var rt = _eventRoot.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 120);
        yield return Parallel(
            LerpAnchoredY(rt, 120, 0, 0.45f),
            FadeGraphic(_eventPanel, 0f, 0.92f, 0.35f)
        );
        yield return FadeGraphic(_eventIcon, 0f, 1f, 0.25f);
        yield return FadeGraphic(_eventTitle, 0f, 1f, 0.25f);
        yield return FadeGraphic(_eventDesc, 0f, 1f, 0.20f);

        yield return new WaitForSeconds(2.8f);

        yield return Parallel(
            LerpAnchoredY(rt, 0, 120, 0.35f),
            FadeGraphic(_eventPanel, 0.92f, 0f, 0.35f)
        );
        _eventRoot.SetActive(false);
    }

    private IEnumerator EventoTornado()
    {
        StartCoroutine(MostrarMensajeEvento("🌪", "GIGANTIC TORNADO!!", "Everything is chaotic for " + duracionTornado + " seconds", ColOrange));
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
            if (bola8 != null) bolasEnMesa.Add(bola8);

            foreach (GameObject bola in bolasEnMesa)
            {
                Rigidbody rb = bola.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 f = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(f * fuerzaDelTornado * rb.mass, ForceMode.Impulse);
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
        StartCoroutine(MostrarMensajeEvento("😈", "THE VILLIAN HAS APPEARED!!", "It hads 3 more balls to the table", ColRed));
        if (sonidoVillano != null) altavoz.PlayOneShot(sonidoVillano);

        if (manoVillanoPrefab != null && centroDelTablero != null)
        {
            GameObject mano = Instantiate(manoVillanoPrefab, centroDelTablero.position + Vector3.up * 2f, Quaternion.identity);
            Destroy(mano, 3f);
        }

        yield return new WaitForSeconds(1.0f);

        if (prefabsDeBolas != null && prefabsDeBolas.Length > 0 && centroDelTablero != null)
        {
            for (int i = 0; i < bolasVillano; i++)
            {
                Vector2 posAleatoria = Random.insideUnitCircle * separacionBolasVillano;
                Vector3 spawnPos = centroDelTablero.position + new Vector3(posAleatoria.x, 0, posAleatoria.y);

                int idx = Random.Range(0, prefabsDeBolas.Length);
                Instantiate(prefabsDeBolas[idx], spawnPos, Quaternion.identity);
                totalBalls++;
            }
        }
    }

    private IEnumerator EventoGravedad()
    {
        StartCoroutine(MostrarMensajeEvento("⬇", "EXTREM GRAVITY!!", "Now teh balls wight 5 times more", ColYellow));
        if (sonidoGravedad != null) altavoz.PlayOneShot(sonidoGravedad);

        // 1. APLICAR GRAVEDAD EXTREMA
        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Ball");
        GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
        List<GameObject> bolasEnMesa = new List<GameObject>(todasLasBolas);
        if (bola8 != null) bolasEnMesa.Add(bola8);

        foreach (GameObject bola in bolasEnMesa)
        {
            Rigidbody rb = bola.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass *= multiplicadorGravedad;
                rb.linearDamping *= 3f;
                rb.angularDamping *= 3f;
            }
        }

        // 2. ESPERAR EL TIEMPO CONFIGURADO
        yield return new WaitForSeconds(duracionGravedad);

        // 3. VOLVER A LA NORMALIDAD
        GameObject[] bolasFin = GameObject.FindGameObjectsWithTag("Ball");
        GameObject bola8Fin = GameObject.FindGameObjectWithTag("Ball8");
        List<GameObject> mesaFin = new List<GameObject>(bolasFin);
        if (bola8Fin != null) mesaFin.Add(bola8Fin);

        foreach (GameObject bola in mesaFin)
        {
            Rigidbody rb = bola.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass /= multiplicadorGravedad;
                rb.linearDamping /= 3f;
                rb.angularDamping /= 3f;
            }
        }
    }

    // ==========================================
    //          MECÁNICA DE ROBAR Y ATURDIR
    // ==========================================

    public void RobarPuntoYAturdir(string atacante, string victima)
{
    // 1. Validaciones iniciales
    if (gameEnded) return;

    // 2. Transferencia de puntos y aplicación del estado 'Stun'
    if (victima == "Player1")
    {
        if (puntosP1 > 0)
        {
            puntosP1 -= 1;
            puntosP2 += 1;
            Debug.Log("¡ROBO! Player 2 le roba 1 punto a Player 1.");
        }
        isPlayer1Stunned = true;
    }
    else if (victima == "Player2")
    {
        if (puntosP2 > 0)
        {
            puntosP2 -= 1;
            puntosP1 += 1;
            Debug.Log("¡ROBO! Player 1 le roba 1 punto a Player 2.");
        }
        isPlayer2Stunned = true;
    }

    // 3. Efectos Audiovisuales (Interfaz y Sonido Eléctrico)
    ActualizarMarcadoresUI();
    
    if (altavoz != null && stunElectricSound != null)
    {
        altavoz.PlayOneShot(stunElectricSound);
    }

    // 4. Congelación física de la víctima
    GameObject jugadorVictima = GameObject.FindGameObjectWithTag(victima);
    if (jugadorVictima != null)
    {
        Rigidbody rbJugador = jugadorVictima.GetComponent<Rigidbody>();
        if (rbJugador != null)
        {
            rbJugador.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    // 5. Iniciar recuperación del jugador (2 segundos)
    StartCoroutine(RutinaDesaturdir(victima));
}

    private IEnumerator RutinaDesaturdir(string victima)
    {
        yield return new WaitForSeconds(2f);

        if (victima == "Player1") isPlayer1Stunned = false;
        else if (victima == "Player2") isPlayer2Stunned = false;

        // DESCONGELAR FÍSICAMENTE AL JUGADOR
        GameObject jugadorVictima = GameObject.FindGameObjectWithTag(victima);
        if (jugadorVictima != null)
        {
            Rigidbody rbJugador = jugadorVictima.GetComponent<Rigidbody>();
            if (rbJugador != null)
            {
                // IMPORTANTE: Lo habitual en los jugadores es que no se caigan hacia los lados, 
                // así que descongelamos la posición pero dejamos congelada la rotación.
                // Si en tu juego usas otra configuración, dímelo y lo ajustamos.
                rbJugador.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        Debug.Log($"¡El jugador {victima} se ha descongelado y puede moverse!");
    }

    // ==========================================
    //          BARRA DE TIEMPO VISUAL
    // ==========================================

    void BuildTimerUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Contenedor principal — lateral izquierdo centrado verticalmente
        var root = new GameObject("TimerBarVisual");
        root.transform.SetParent(canvas.transform, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0f, 0.5f);
        rootRT.anchorMax = new Vector2(0f, 0.5f);
        rootRT.pivot = new Vector2(0f, 0.5f);
        rootRT.sizeDelta = new Vector2(90f, 500f);
        rootRT.anchoredPosition = new Vector2(12f, 0f);

        // ── Sombra exterior (madera oscura, da profundidad) ──────────────────
        var shadow = MakeImage(root, "Shadow", new Color(0.04f, 0.02f, 0.01f, 0.85f));
        shadow.rectTransform.anchorMin = Vector2.zero;
        shadow.rectTransform.anchorMax = Vector2.one;
        shadow.rectTransform.offsetMin = new Vector2(-4f, -4f);
        shadow.rectTransform.offsetMax = new Vector2(4f, 4f);

        // ── Marco exterior dorado grueso (estilo borde de mesa de billar) ────
        var frameOuter = MakeImage(root, "FrameOuter", new Color(0.55f, 0.38f, 0.08f, 1f)); // madera dorada
        StretchFull(frameOuter.rectTransform);

        // ── Relleno interior madera oscura ───────────────────────────────────
        var frameInner = MakeImage(root, "FrameInner", new Color(0.18f, 0.09f, 0.03f, 1f)); // madera oscura
        frameInner.rectTransform.anchorMin = Vector2.zero;
        frameInner.rectTransform.anchorMax = Vector2.one;
        frameInner.rectTransform.offsetMin = new Vector2(4f, 4f);
        frameInner.rectTransform.offsetMax = new Vector2(-4f, -4f);

        // ── Zona de la barra (feltro verde oscuro de fondo) ──────────────────
        var feltBG = MakeImage(root, "FeltBG", new Color(0.05f, 0.18f, 0.07f, 1f));
        feltBG.rectTransform.anchorMin = new Vector2(0f, 0f);
        feltBG.rectTransform.anchorMax = new Vector2(1f, 1f);
        feltBG.rectTransform.offsetMin = new Vector2(8f, 68f);  // 68 = espacio para número abajo
        feltBG.rectTransform.offsetMax = new Vector2(-8f, -8f);

        // ── Fill (la barra que sube) ──────────────────────────────────────────
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(feltBG.rectTransform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = ColTimerHigh;
        _timerFill = fillImg.rectTransform;
        _timerFill.anchorMin = new Vector2(0f, 0f);
        _timerFill.anchorMax = new Vector2(1f, 0f);
        _timerFill.pivot = new Vector2(0.5f, 0f);
        _timerFill.offsetMin = new Vector2(4f, 4f);
        _timerFill.offsetMax = new Vector2(-4f, 4f);
        _timerFill.sizeDelta = new Vector2(-8f, 0f);

        // ── Brillo superior del fill (línea clara que le da volumen) ─────────
        var shine = MakeImage(fillGO, "Shine", new Color(1f, 1f, 1f, 0.15f));
        shine.rectTransform.anchorMin = new Vector2(0f, 1f);
        shine.rectTransform.anchorMax = new Vector2(1f, 1f);
        shine.rectTransform.pivot = new Vector2(0.5f, 1f);
        shine.rectTransform.sizeDelta = new Vector2(0f, 3f);

        // ── Marcas doradas cada 25% (como las bandas de la mesa) ─────────────
        for (int i = 1; i < 4; i++)
        {
            var mark = MakeImage(feltBG.gameObject, "Mark" + i, new Color(0.55f, 0.38f, 0.08f, 0.55f));
            mark.rectTransform.anchorMin = new Vector2(0f, i * 0.25f);
            mark.rectTransform.anchorMax = new Vector2(1f, i * 0.25f);
            mark.rectTransform.sizeDelta = new Vector2(0f, 2f);
            mark.rectTransform.anchoredPosition = Vector2.zero;
        }

        // ── Separador dorado entre barra y número ────────────────────────────
        var divider = MakeImage(root, "Divider", new Color(0.55f, 0.38f, 0.08f, 1f));
        divider.rectTransform.anchorMin = new Vector2(0.1f, 0f);
        divider.rectTransform.anchorMax = new Vector2(0.9f, 0f);
        divider.rectTransform.pivot = new Vector2(0.5f, 0f);
        divider.rectTransform.sizeDelta = new Vector2(0f, 2f);
        divider.rectTransform.anchoredPosition = new Vector2(0f, 62f);

        // ── Número grande de segundos ─────────────────────────────────────────
        _timerSeconds = MakeTMP(root, "Seconds", "30", 46, FontStyles.Bold);
        var secRT = _timerSeconds.rectTransform;
        secRT.anchorMin = new Vector2(0f, 0f);
        secRT.anchorMax = new Vector2(1f, 0f);
        secRT.pivot = new Vector2(0.5f, 0f);
        secRT.sizeDelta = new Vector2(0f, 60f);
        secRT.anchoredPosition = new Vector2(0f, 5f);
        _timerSeconds.alignment = TextAlignmentOptions.Center;
        _timerSeconds.color = ColGoldLight;
        _timerSeconds.characterSpacing = 2f;

        // ── Etiqueta "s" pequeña junto al número ─────────────────────────────
        var labelS = MakeTMP(root, "LabelS", "seg", 14, FontStyles.Italic);
        var lsRT = labelS.rectTransform;
        lsRT.anchorMin = new Vector2(0f, 0f);
        lsRT.anchorMax = new Vector2(1f, 0f);
        lsRT.pivot = new Vector2(0.5f, 0f);
        lsRT.sizeDelta = new Vector2(0f, 20f);
        lsRT.anchoredPosition = new Vector2(0f, 44f);
        labelS.alignment = TextAlignmentOptions.Center;
        labelS.color = new Color(0.55f, 0.38f, 0.08f, 1f);

        // Ocultamos TimerText original
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    void BuildScoreUI()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            var root = new GameObject("ScoreBoard");
            root.transform.SetParent(canvas.transform, false);
            var rootRT = root.AddComponent<RectTransform>();

            // Lo anclamos en el centro de la parte de abajo
            rootRT.anchorMin = new Vector2(0.5f, 0f);
            rootRT.anchorMax = new Vector2(0.5f, 0f);
            rootRT.pivot = new Vector2(0.5f, 0.5f); // Pivot central para que rote perfecto
            rootRT.sizeDelta = new Vector2(400f, 70f);
            rootRT.anchoredPosition = new Vector2(0f, 60f); // Un poco separado del borde inferior
            
            rootRT.localEulerAngles = new Vector3(0, 0, 180);

            // Fondo semitransparente
            var bg = MakeImage(root, "ScoreBG", new Color(0.05f, 0.05f, 0.05f, 0.85f));
            StretchFull(bg.rectTransform);

            // Borde dorado
            var border = MakeImage(root, "Border", ColGold.WithAlpha(0.6f));
            StretchFull(border.rectTransform);
            border.rectTransform.offsetMin = new Vector2(-2, -2);
            border.rectTransform.offsetMax = new Vector2(2, 2);
            border.transform.SetAsFirstSibling();

            // Texto P1 (Izquierda - Color Rojo)
            var t1 = MakeTMP(root, "ScoreP1", "P1: 0", 34, FontStyles.Bold);
            AnchorRect(t1.rectTransform, 0f, 0f, 0.45f, 1f);
            t1.alignment = TextAlignmentOptions.Center;
            t1.color = new Color(0.2f, 1.0f, 0.9f, 1f);
            _textosP1.Add(t1);

            // Separador (Centro)
            var sep = MakeTMP(root, "Separator", "VS", 24, FontStyles.Bold);
            AnchorRect(sep.rectTransform, 0.45f, 0f, 0.55f, 1f);
            sep.alignment = TextAlignmentOptions.Center;
            sep.color = ColGoldLight;

            // Texto P2 (Derecha - Color Azul)
            var t2 = MakeTMP(root, "ScoreP2", "P2: 0", 34, FontStyles.Bold);
            AnchorRect(t2.rectTransform, 0.55f, 0f, 1f, 1f);
            t2.alignment = TextAlignmentOptions.Center;
            t2.color = new Color(1.0f, 0.0f, 1.0f, 1f);
            _textosP2.Add(t2);
        }
    }

    // Llama a esto para refrescar los números visualmente
    void ActualizarMarcadoresUI()
    {
        for (int i = 0; i < _textosP1.Count; i++)
        {
            _textosP1[i].text = $"P1: {puntosP1}";
            _textosP2[i].text = $"P2: {puntosP2}";
        }
    }

    void UpdateTimerBar()
    {
        if (_timerFill == null) return;

        float ratio = Mathf.Clamp01(timeRemaining / tiempoMaximo);

        // Altura del fill — necesitamos la altura del contenedor BG
        // El BG ocupa el root menos los offsets (36 abajo para el número, 3 arriba)
        float bgHeight = _timerFill.parent.GetComponent<RectTransform>().rect.height;
        if (bgHeight <= 0) bgHeight = 381f; // fallback si rect aún no está calculado
        float fillHeight = Mathf.Max(4f, (bgHeight - 6f) * ratio);
        _timerFill.sizeDelta = new Vector2(-6f, fillHeight);

        // Color: verde → amarillo → rojo
        Color fillColor;
        if (ratio > 0.6f)
            fillColor = Color.Lerp(ColTimerMid, ColTimerHigh, (ratio - 0.6f) / 0.4f);
        else if (ratio > 0.3f)
            fillColor = Color.Lerp(ColTimerLow, ColTimerMid, (ratio - 0.3f) / 0.3f);
        else
            fillColor = Color.Lerp(ColTimerLow, ColTimerLow * 1.1f, Mathf.PingPong(Time.time * 3f, 1f)); // pulso en rojo

        _timerFill.GetComponent<Image>().color = fillColor;

        // Número
        if (_timerSeconds != null)
        {
            int secs = Mathf.CeilToInt(timeRemaining);
            _timerSeconds.text = secs.ToString();
            _timerSeconds.color = ratio < 0.3f ? ColTimerLow : ColCream;
        }
    }

    void BuildResultUI()
    {
        // Buscamos todos los Canvases de la escena (el original y el duplicado)
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases.Length == 0) return;

        foreach (Canvas canvas in canvases)
        {
            GameObject canvasGO = canvas.gameObject;

            var resRoot = new GameObject("ResultScreen");
            resRoot.transform.SetParent(canvasGO.transform, false);
            var rootRT = resRoot.AddComponent<RectTransform>();
            StretchFull(rootRT);
            resRoot.SetActive(false);
            _resultRoots.Add(resRoot);

            // Overlay
            var over = MakeImage(resRoot, "Overlay", ColDark.WithAlpha(0f));
            StretchFull(over.rectTransform);
            _overlays.Add(over);

            // Panel central
            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(resRoot.transform, false);
            var pan = panelGO.AddComponent<Image>();
            pan.color = ColDark.WithAlpha(0f);
            var pr = pan.rectTransform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(860, 460);
            _panels.Add(pan);

            // Líneas doradas decorativas
            MakeHorizontalLine(panelGO, "LineTop", new Vector2(0.1f, 1f), new Vector2(0.9f, 1f), new Vector2(0, -18));
            MakeHorizontalLine(panelGO, "LineBot", new Vector2(0.1f, 0f), new Vector2(0.9f, 0f), new Vector2(0, 18));

            // Título
            var tit = MakeTMP(panelGO, "Title", "", 94, FontStyles.Bold);
            AnchorRect(tit.rectTransform, 0f, 0.55f, 1f, 0.95f);
            tit.alignment = TextAlignmentOptions.Center;
            tit.characterSpacing = 16f;
            tit.color = ColGoldLight.WithAlpha(0f);
            _titleTMPs.Add(tit);

            // Subtítulo
            var sub = MakeTMP(panelGO, "Subtitle", "", 30, FontStyles.Italic);
            AnchorRect(sub.rectTransform, 0.05f, 0.32f, 0.95f, 0.56f);
            sub.alignment = TextAlignmentOptions.Center;
            sub.color = ColCream.WithAlpha(0f);
            _subtitleTMPs.Add(sub);

            // Score
            var sco = MakeTMP(panelGO, "Score", "", 24, FontStyles.Normal);
            AnchorRect(sco.rectTransform, 0.1f, 0.18f, 0.9f, 0.34f);
            sco.alignment = TextAlignmentOptions.Center;
            sco.color = ColGold.WithAlpha(0f);
            _scoreTMPs.Add(sco);
        }
    }

    // ==========================================
    //          CONSTRUCCIÓN UI — EVENTO
    // ==========================================

    void BuildEventUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        _eventRoot = new GameObject("EventNotification");
        _eventRoot.transform.SetParent(canvas.transform, false);
        var rt = _eventRoot.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(680, 130);
        rt.anchoredPosition = new Vector2(0, -20);

        rt.localScale = new Vector3(1.5f, 1.5f, 1f);

        _eventPanel = _eventRoot.AddComponent<Image>();
        _eventPanel.color = ColDark.WithAlpha(0f);

        // Borde dorado
        var border = MakeImage(_eventRoot, "Border", ColGold.WithAlpha(0.6f));
        StretchFull(border.rectTransform);
        border.rectTransform.offsetMin = new Vector2(-2, -2);
        border.rectTransform.offsetMax = new Vector2(2, 2);
        border.transform.SetAsFirstSibling();

        // Icono
        _eventIcon = MakeTMP(_eventRoot, "Icon", "", 48, FontStyles.Normal);
        AnchorRect(_eventIcon.rectTransform, 0f, 0f, 0.15f, 1f);
        _eventIcon.alignment = TextAlignmentOptions.Center;
        _eventIcon.color = ColCream.WithAlpha(0f);

        // Título del evento
        _eventTitle = MakeTMP(_eventRoot, "EventTitle", "", 28, FontStyles.Bold);
        AnchorRect(_eventTitle.rectTransform, 0.15f, 0.48f, 1f, 1f);
        _eventTitle.alignment = TextAlignmentOptions.Left;
        _eventTitle.characterSpacing = 4f;
        _eventTitle.color = ColCream.WithAlpha(0f);

        // Descripción
        _eventDesc = MakeTMP(_eventRoot, "EventDesc", "", 20, FontStyles.Italic);
        AnchorRect(_eventDesc.rectTransform, 0.15f, 0f, 1f, 0.52f);
        _eventDesc.alignment = TextAlignmentOptions.Left;
        _eventDesc.color = ColCream.WithAlpha(0f);

        _eventRoot.SetActive(false);
    }

    // ==========================================
    //          ANIMACIONES DE RESULTADO
    // ==========================================

    IEnumerator ShowResult(bool won, int balls, string mensajeSubtitulo)
    {
        // Encendemos y configuramos los textos en todos los proyectores a la vez
        for (int i = 0; i < _resultRoots.Count; i++)
        {
            _resultRoots[i].SetActive(true);

            if (won)
            {
                _titleTMPs[i].text = "VICTORY";
                _subtitleTMPs[i].text = mensajeSubtitulo;
                _panels[i].color = ColGreen.WithAlpha(0.96f);
            }
            else
            {
                _titleTMPs[i].text = "TIE";
                _subtitleTMPs[i].text = mensajeSubtitulo;
                _panels[i].color = ColGray.WithAlpha(0.96f);
            }
            _scoreTMPs[i].text = balls > 0 ? $"Balls potted: {balls}" : "";

            // Disparamos las animaciones
            StartCoroutine(FadeGraphic(_overlays[i], 0f, 0.78f, 0.35f));

            _panels[i].rectTransform.localScale = Vector3.one * 0.72f;
            _panels[i].color = _panels[i].color.WithAlpha(0f);

            StartCoroutine(FadeGraphic(_panels[i], 0f, 0.96f, 0.4f));
            StartCoroutine(ScaleTo(_panels[i].rectTransform, 0.72f, 1f, 0.4f));

            StartCoroutine(FadeTMP(_titleTMPs[i], 0f, 1f, 0.45f, 22f));
            StartCoroutine(FadeTMP(_subtitleTMPs[i], 0f, 1f, 0.35f));

            if (_scoreTMPs[i].text.Length > 0)
                StartCoroutine(FadeTMP(_scoreTMPs[i], 0f, 1f, 0.3f));

            StartCoroutine(PulseTMP(_titleTMPs[i]));
        }

        // Esperamos a que terminen las animaciones y pasen los 5 segundos extra
        yield return new WaitForSecondsRealtime(2.2f);
        yield return new WaitForSecondsRealtime(7f);

        SceneManager.LoadScene("StartScene");
    }

    IEnumerator PulseTMP(TextMeshProUGUI tmp)
    {
        while (true)
        {
            yield return LerpColor(tmp, ColGoldLight, ColGold, 1.1f);
            yield return LerpColor(tmp, ColGold, ColGoldLight, 1.1f);
        }
    }

    // ==========================================
    //          HELPERS DE ANIMACIÓN
    // ==========================================

    IEnumerator FadeGraphic(Graphic g, float from, float to, float dur)
    {
        float t = 0; Color c = g.color;
        while (t < dur) { t += Time.deltaTime; c.a = Mathf.Lerp(from, to, t / dur); g.color = c; yield return null; }
        c.a = to; g.color = c;
    }

    IEnumerator FadeTMP(TextMeshProUGUI tmp, float from, float to, float dur, float offsetY = 0f)
    {
        float t = 0; Color c = tmp.color;
        Vector3 base3 = tmp.rectTransform.anchoredPosition3D;
        while (t < dur)
        {
            t += Time.deltaTime; float p = t / dur;
            c.a = Mathf.Lerp(from, to, p); tmp.color = c;
            if (offsetY != 0f) tmp.rectTransform.anchoredPosition3D = base3 + new Vector3(0, Mathf.Lerp(offsetY, 0, p), 0);
            yield return null;
        }
        c.a = to; tmp.color = c;
        tmp.rectTransform.anchoredPosition3D = base3;
    }

    IEnumerator ScaleTo(RectTransform rt, float from, float to, float dur)
    {
        float t = 0;
        while (t < dur) { t += Time.deltaTime; rt.localScale = Vector3.one * Mathf.Lerp(from, to, EaseOut(t / dur)); yield return null; }
        rt.localScale = Vector3.one * to;
    }

    IEnumerator LerpAnchoredY(RectTransform rt, float from, float to, float dur)
    {
        float t = 0; Vector2 pos = rt.anchoredPosition;
        while (t < dur) { t += Time.deltaTime; pos.y = Mathf.Lerp(from, to, EaseOut(t / dur)); rt.anchoredPosition = pos; yield return null; }
        pos.y = to; rt.anchoredPosition = pos;
    }

    IEnumerator LerpColor(TextMeshProUGUI tmp, Color from, Color to, float dur)
    {
        float t = 0;
        while (t < dur) { t += Time.deltaTime; tmp.color = Color.Lerp(from, to, t / dur); yield return null; }
        tmp.color = to;
    }

    IEnumerator Parallel(IEnumerator a, IEnumerator b)
    {
        bool doneA = false, doneB = false;
        StartCoroutine(Run(a, () => doneA = true));
        StartCoroutine(Run(b, () => doneB = true));
        yield return new WaitUntil(() => doneA && doneB);
    }

    IEnumerator Run(IEnumerator co, System.Action onDone) { yield return co; onDone(); }

    float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);

    // ==========================================
    //          HELPERS DE CONSTRUCCIÓN UI
    // ==========================================

    Image MakeImage(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = color; return img;
    }

    TextMeshProUGUI MakeTMP(GameObject parent, string name, string text, float size, FontStyles style)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = ColCream; tmp.enableWordWrapping = true; return tmp;
    }

    void MakeHorizontalLine(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offset)
    {
        var img = MakeImage(parent, name, ColGold.WithAlpha(0.7f));
        img.rectTransform.anchorMin = anchorMin; img.rectTransform.anchorMax = anchorMax;
        img.rectTransform.pivot = new Vector2(0.5f, anchorMin.y > 0.5f ? 1f : 0f);
        img.rectTransform.sizeDelta = new Vector2(0, 2);
        img.rectTransform.anchoredPosition = offset;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void AnchorRect(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin); rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
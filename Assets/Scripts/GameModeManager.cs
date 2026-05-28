using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    public TextMeshProUGUI resultMessageText; // se puede dejar vacío, ya no se usará
    public TextMeshProUGUI eventMessageText;

    [Header("Barra Visual")]
    public Slider timerBar;

    [Header("Audio Victoria/Derrota")]
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDerrota;

    // ── Colores elegantes ────────────────────────────────────────────────────
    private static readonly Color ColDark = new Color(0.08f, 0.05f, 0.02f, 1f);
    private static readonly Color ColGold = new Color(0.85f, 0.68f, 0.25f, 1f);
    private static readonly Color ColGoldLight = new Color(1f, 0.90f, 0.55f, 1f);
    private static readonly Color ColGreen = new Color(0.10f, 0.38f, 0.18f, 1f);
    private static readonly Color ColRed = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color ColCream = new Color(0.95f, 0.92f, 0.82f, 1f);
    private static readonly Color ColYellow = new Color(1f, 0.85f, 0.10f, 1f);
    private static readonly Color ColOrange = new Color(1f, 0.55f, 0.10f, 1f);

    // ── Referencias UI de resultado (creadas por código) ────────────────────
    private GameObject _resultRoot;
    private Image _overlay;
    private Image _panel;
    private TextMeshProUGUI _titleTMP;
    private TextMeshProUGUI _subtitleTMP;
    private TextMeshProUGUI _scoreTMP;
    private Button _restartBtn;

    // ── Referencias UI de evento (creadas por código) ───────────────────────
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
        totalBalls = GameObject.FindGameObjectsWithTag("Ball").Length;
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
    }

    void Update()
    {
        if (gameEnded) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timerText != null)
                timerText.text = "Tiempo: " + Mathf.Ceil(timeRemaining).ToString() + "s";
            if (timerBar != null) timerBar.value = timeRemaining;
        }
        else
        {
            timeRemaining = 0;
            if (timerText != null) timerText.text = "Tiempo: 0s";
            if (timerBar != null) timerBar.value = 0;
            LoseGame();
        }
    }

    public void OnBallPotted()
    {
        if (gameEnded) return;

        timeRemaining += 10f;
        if (timeRemaining > tiempoMaximo) timeRemaining = tiempoMaximo;

        totalBalls--;

        int bolasMetidas = bolasIniciales - totalBalls;
        if (!eventoYaLanzado && bolasMetidas >= (bolasIniciales / 2))
            LanzarEventoAleatorio();

        if (totalBalls <= 0)
        {
            GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
            if (bola8 != null)
                bola8.GetComponent<BallMovement>().HacerSolidaParaJugador();
        }
    }

    // ==========================================
    //          VICTORIA / DERROTA
    // ==========================================

    public void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        int bolasMetidas = bolasIniciales - totalBalls;
        if (sonidoVictoria != null) altavoz.PlayOneShot(sonidoVictoria);
        StartCoroutine(ShowResult(true, bolasMetidas));
    }

    public void LoseGame(string mensaje = "")
    {
        if (gameEnded) return;
        gameEnded = true;
        if (sonidoDerrota != null) altavoz.PlayOneShot(sonidoDerrota);
        StartCoroutine(ShowResult(false, 0));
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
            case 2: EventoGravedad(); break;
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
        StartCoroutine(MostrarMensajeEvento("🌪", "¡TORNADO SALVAJE!", "Las bolas enloquecen durante " + duracionTornado + " segundos", ColOrange));
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
        StartCoroutine(MostrarMensajeEvento("😈", "¡EL VILLANO HACE TRAMPAS!", "Añade 3 bolas nuevas a la mesa", ColRed));
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
            Vector3[] pos = new Vector3[3] {
                new Vector3(0, 0, dist),
                new Vector3(-dist, 0, -dist),
                new Vector3(dist, 0, -dist)
            };
            for (int i = 0; i < 3; i++)
            {
                int idx = Random.Range(0, prefabsDeBolas.Length);
                Instantiate(prefabsDeBolas[idx], centroDelTablero.position + pos[i], Quaternion.identity);
                totalBalls++;
            }
        }
    }

    private void EventoGravedad()
    {
        StartCoroutine(MostrarMensajeEvento("⬇", "¡GRAVEDAD EXTREMA!", "Las bolas ahora pesan 5 veces más", ColYellow));
        if (sonidoGravedad != null) altavoz.PlayOneShot(sonidoGravedad);

        GameObject[] todasLasBolas = GameObject.FindGameObjectsWithTag("Ball");
        GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
        List<GameObject> bolasEnMesa = new List<GameObject>(todasLasBolas);
        if (bola8 != null) bolasEnMesa.Add(bola8);

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

    // ==========================================
    //          CONSTRUCCIÓN UI — RESULTADO
    // ==========================================

    void BuildResultUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject canvasGO = canvas.gameObject;

        _resultRoot = new GameObject("ResultScreen");
        _resultRoot.transform.SetParent(canvasGO.transform, false);
        var rootRT = _resultRoot.AddComponent<RectTransform>();
        StretchFull(rootRT);
        _resultRoot.SetActive(false);

        // Overlay
        _overlay = MakeImage(_resultRoot, "Overlay", ColDark.WithAlpha(0f));
        StretchFull(_overlay.rectTransform);

        // Panel central
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(_resultRoot.transform, false);
        _panel = panelGO.AddComponent<Image>();
        _panel.color = ColDark.WithAlpha(0f);
        var pr = _panel.rectTransform;
        pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(860, 460);

        // Líneas doradas decorativas
        MakeHorizontalLine(panelGO, "LineTop", new Vector2(0.1f, 1f), new Vector2(0.9f, 1f), new Vector2(0, -18));
        MakeHorizontalLine(panelGO, "LineBot", new Vector2(0.1f, 0f), new Vector2(0.9f, 0f), new Vector2(0, 18));

        // Título
        _titleTMP = MakeTMP(panelGO, "Title", "", 94, FontStyles.Bold);
        AnchorRect(_titleTMP.rectTransform, 0f, 0.55f, 1f, 0.95f);
        _titleTMP.alignment = TextAlignmentOptions.Center;
        _titleTMP.characterSpacing = 16f;
        _titleTMP.color = ColGoldLight.WithAlpha(0f);

        // Subtítulo
        _subtitleTMP = MakeTMP(panelGO, "Subtitle", "", 30, FontStyles.Italic);
        AnchorRect(_subtitleTMP.rectTransform, 0.05f, 0.32f, 0.95f, 0.56f);
        _subtitleTMP.alignment = TextAlignmentOptions.Center;
        _subtitleTMP.color = ColCream.WithAlpha(0f);

        // Score
        _scoreTMP = MakeTMP(panelGO, "Score", "", 24, FontStyles.Normal);
        AnchorRect(_scoreTMP.rectTransform, 0.1f, 0.18f, 0.9f, 0.34f);
        _scoreTMP.alignment = TextAlignmentOptions.Center;
        _scoreTMP.color = ColGold.WithAlpha(0f);

        // Botón reiniciar
        var btnGO = new GameObject("RestartBtn");
        btnGO.transform.SetParent(panelGO.transform, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = ColGold.WithAlpha(0f);
        AnchorRect(btnImg.rectTransform, 0.28f, 0.03f, 0.72f, 0.17f);
        _restartBtn = btnGO.AddComponent<Button>();
        _restartBtn.targetGraphic = btnImg;
        var nav = Navigation.defaultNavigation; nav.mode = Navigation.Mode.None;
        _restartBtn.navigation = nav;
        _restartBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        var btnLabel = MakeTMP(btnGO, "Label", "JUGAR DE NUEVO", 22, FontStyles.Bold);
        StretchFull(btnLabel.rectTransform);
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.color = ColDark;
        btnGO.SetActive(false);
        _restartBtn = _restartBtn; // guardamos ref via btnGO
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

    IEnumerator ShowResult(bool won, int balls)
    {
        _resultRoot.SetActive(true);

        // Configurar textos
        if (won)
        {
            _titleTMP.text = "VICTORIA";
            _subtitleTMP.text = "¡Lo habéis conseguido, equipo!";
            _panel.color = ColGreen.WithAlpha(0.96f);
        }
        else
        {
            _titleTMP.text = "DERROTA";
            _subtitleTMP.text = "El tiempo se agotó...\n¡Intentadlo de nuevo!";
            _panel.color = ColRed.WithAlpha(0.96f);
        }
        _scoreTMP.text = balls > 0 ? $"Bolas embocadas: {balls}" : "";

        // Animar
        yield return FadeGraphic(_overlay, 0f, 0.78f, 0.35f);

        _panel.rectTransform.localScale = Vector3.one * 0.72f;
        _panel.color = _panel.color.WithAlpha(0f);
        yield return Parallel(
            FadeGraphic(_panel, 0f, 0.96f, 0.4f),
            ScaleTo(_panel.rectTransform, 0.72f, 1f, 0.4f)
        );

        yield return FadeTMP(_titleTMP, 0f, 1f, 0.45f, 22f);
        yield return FadeTMP(_subtitleTMP, 0f, 1f, 0.35f);
        if (_scoreTMP.text.Length > 0)
            yield return FadeTMP(_scoreTMP, 0f, 1f, 0.3f);

        StartCoroutine(PulseTMP(_titleTMP));

        yield return new WaitForSeconds(2.2f);

        // Mostrar botón
        var btnGO = _restartBtn.gameObject;
        btnGO.SetActive(true);
        yield return FadeGraphic(btnGO.GetComponent<Image>(), 0f, 1f, 0.4f);
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

// Extensión para añadir alpha a un Color
public static class ColorExtensions
{
    public static Color WithAlpha(this Color c, float a) => new Color(c.r, c.g, c.b, a);
}
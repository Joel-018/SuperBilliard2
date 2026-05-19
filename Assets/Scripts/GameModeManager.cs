using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    [Header("Configuración")]
    public float timeRemaining = 15f;
    public int totalBalls; // <--- Ahora es pública      
    private bool gameEnded = false;

    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultMessageText;

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
        if (resultMessageText != null) resultMessageText.gameObject.SetActive(false);

        if (timerBar != null)
        {
            timerBar.maxValue = timeRemaining;
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

        timeRemaining += 10f;
        totalBalls--;

        if (timerBar != null && timeRemaining > timerBar.maxValue)
        {
            timerBar.maxValue = timeRemaining;
        }

        // --- LA MAGIA ESTÁ AQUÍ ---
        // Si ya no quedan bolas normales, buscamos la Bola 8 y la hacemos sólida
        if (totalBalls <= 0)
        {
            GameObject bola8 = GameObject.FindGameObjectWithTag("Ball8");
            if (bola8 != null)
            {
                bola8.GetComponent<BallMovement>().HacerSolidaParaJugador();
            }
        }
    }

    public void WinGame()
    {
        gameEnded = true;
        resultMessageText.text = "¡GANASTE!";
        resultMessageText.color = Color.green; // <--- Te lo he cambiado a verde
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
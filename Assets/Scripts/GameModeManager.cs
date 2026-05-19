using UnityEngine;
using UnityEngine.UI; // Importante para poder usar el Slider (la barra)
using TMPro;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    [Header("Configuración")]
    public float timeRemaining = 15f; 
    private int totalBalls;           
    private bool gameEnded = false;   

    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultMessageText;
    
    [Header("Barra Visual")]
    public Slider timerBar; // <-- Aquí metemos la nueva barra

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalBalls = GameObject.FindGameObjectsWithTag("Ball").Length;
        if(resultMessageText != null) resultMessageText.gameObject.SetActive(false);

        // Configuramos la barra al inicio para que coincida con el tiempo
        if(timerBar != null)
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
            
            // Actualizamos la barra de forma fluida cada frame
            if(timerBar != null) timerBar.value = timeRemaining;
        }
        else
        {
            timeRemaining = 0;
            timerText.text = "Tiempo: 0s";
            if(timerBar != null) timerBar.value = 0;
            LoseGame(); // Llama al mensaje de derrota
        }
    }

    public void OnBallPotted()
    {
        if (gameEnded) return;

        timeRemaining += 10f; // Sube 10 segundos
        totalBalls--;         

        // Si al sumar tiempo nos pasamos del máximo original de la barra, la hacemos más grande
        if(timerBar != null && timeRemaining > timerBar.maxValue)
        {
            timerBar.maxValue = timeRemaining; 
        }

        if (totalBalls <= 0)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        gameEnded = true;
        resultMessageText.text = "¡GANASTE!";
        resultMessageText.color = Color.green;
        resultMessageText.gameObject.SetActive(true);
    }

    void LoseGame()
    {
        gameEnded = true;
        resultMessageText.text = "¡PERDISTE!\nSe acabó el tiempo.";
        resultMessageText.color = Color.red;
        resultMessageText.gameObject.SetActive(true);
    }
}
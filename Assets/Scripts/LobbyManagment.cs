using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Arrastra aquí los dos círculos desde la jerarquía")]
    public DetectorInicio circulo1;
    public DetectorInicio circulo2;

    [Header("Animación de Transición")]
    [Tooltip("Arrastra aquí las 8 bolas visuales del menú")]
    public List<GameObject> bolasVisuales;

    [Tooltip("Distancia desde la que aparecerán las bolas")]
    public float radioAparicion = 20f;
    public float velocidadBolas = 15f;

    [Header("Configuración")]
    public string nombreEscenaJuego = "SampleScene";

    private bool cuentaAtrasIniciada = false;
    private Coroutine corrutinaAnimacion;
    private List<Vector3> posicionesFinales = new List<Vector3>();

    void Start()
    {
        // 1. Al cargar el menú, guardamos la forma de la pirámide y escondemos las bolas
        foreach (GameObject bola in bolasVisuales)
        {
            posicionesFinales.Add(bola.transform.position);
            bola.SetActive(false);
        }
    }

    void Update()
    {
        // Comprobamos si ambos círculos tienen a un jugador encima
        if (circulo1.jugadorDentro && circulo2.jugadorDentro)
        {
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                corrutinaAnimacion = StartCoroutine(AnimarPiramideYCargar());
            }
        }
        else
        {
            // Si uno se sale, cancelamos el show
            if (cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = false;
                if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);

                EsconderBolas();
                Debug.Log("Animación cancelada. ¡Un jugador se ha salido!");
            }
        }
    }

    private IEnumerator AnimarPiramideYCargar()
    {
        // 2. Hacemos que vengan una a una para que quede más chulo
        for (int i = 0; i < bolasVisuales.Count; i++)
        {
            GameObject bola = bolasVisuales[i];
            Vector3 destino = posicionesFinales[i];

            // Calculamos un punto aleatorio circular lejos del centro
            Vector2 puntoAleatorio = Random.insideUnitCircle.normalized * radioAparicion;
            // Mantenemos la altura (destino.y) exacta que necesita tener sobre el tablero
            bola.transform.position = new Vector3(puntoAleatorio.x, destino.y, puntoAleatorio.y);

            bola.SetActive(true);

            // Movemos la bola suavemente hacia su sitio en la pirámide
            while (Vector3.Distance(bola.transform.position, destino) > 0.05f)
            {
                bola.transform.position = Vector3.MoveTowards(bola.transform.position, destino, velocidadBolas * Time.deltaTime);
                yield return null;
            }

            // La anclamos exacta por si acaso
            bola.transform.position = destino;
        }

        // 3. Cuando la última bola llega, dejamos un instante para que el jugador vea la pirámide
        yield return new WaitForSeconds(0.3f);

        // ¡PUM! Cambio de escena sin que el jugador lo note visualmente
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    private void EsconderBolas()
    {
        foreach (GameObject bola in bolasVisuales)
        {
            bola.SetActive(false);
        }
    }
}
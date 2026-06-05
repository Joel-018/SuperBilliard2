using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimadorPiramide : MonoBehaviour
{
    [Header("Configuración de Animación")]
    public List<GameObject> bolasVisuales;
    public float radioAparicion = 20f;
    public float velocidadBolas = 15f;

    private List<Vector3> posicionesFinales = new List<Vector3>();
    private Coroutine corrutinaAnimacion;

    // Prepara las bolas guardando sus posiciones finales y desactivándolas al inicio
    void Awake()
    {
        foreach (GameObject bola in bolasVisuales)
        {
            posicionesFinales.Add(bola.transform.position);
            bola.SetActive(false);
        }
    }

    // Detiene cualquier animación en curso e inicia la secuencia de animación y carga de escena
    public void IniciarAnimacion(string escenaDestino)
    {
      
        if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);

        corrutinaAnimacion = StartCoroutine(AnimarYCargar(escenaDestino));
    }

    // Función para cancelar y esconder las bolas
    public void CancelarAnimacion()
    {
        if (corrutinaAnimacion != null)
        {
            StopCoroutine(corrutinaAnimacion);
            corrutinaAnimacion = null;
        }

        // Escondemos todas las bolas otra vez
        foreach (GameObject bola in bolasVisuales)
        {
            bola.SetActive(false);
        }
    }

    //Ejecuta la animación de las bolas y carga la escena de destino al finalizar
    private IEnumerator AnimarYCargar(string escenaDestinoFinal)
    {
        for (int i = 0; i < bolasVisuales.Count; i++)
        {
            GameObject bola = bolasVisuales[i];
            Vector3 destino = posicionesFinales[i];

            Vector2 puntoAleatorio = Random.insideUnitCircle.normalized * radioAparicion;
            bola.transform.position = new Vector3(puntoAleatorio.x, destino.y, puntoAleatorio.y);
            bola.SetActive(true);

            while (Vector3.Distance(bola.transform.position, destino) > 0.05f)
            {
                bola.transform.position = Vector3.MoveTowards(bola.transform.position, destino, velocidadBolas * Time.deltaTime);
                yield return null;
            }
            bola.transform.position = destino;
        }

        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(escenaDestinoFinal);
    }
}
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
    private Coroutine corrutinaAnimacion; // Aquí guardaremos la animación en curso

    void Awake()
    {
        foreach (GameObject bola in bolasVisuales)
        {
            posicionesFinales.Add(bola.transform.position);
            bola.SetActive(false);
        }
    }

    public void IniciarAnimacion(string escenaDestino)
    {
        // Si ya hay una animación, la paramos por seguridad
        if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);

        corrutinaAnimacion = StartCoroutine(AnimarYCargar(escenaDestino));
    }

    // NUEVO: Función para cancelar y esconder las bolas
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
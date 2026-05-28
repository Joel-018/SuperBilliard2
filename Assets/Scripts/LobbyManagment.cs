using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Círculos de Modos (Arrastra los 4 aquí)")]
    public DetectorInicio circuloCoop1;
    public DetectorInicio circuloCoop2;
    public DetectorInicio circuloComp1;
    public DetectorInicio circuloComp2;

    [Header("Grupos de Interfaz (Arrastra los 3 objetos vacíos)")]
    public GameObject uiPrincipal;
    public GameObject uiCoop;
    public GameObject uiComp;

    [Header("Escenas de Destino")]
    public string escenaCoop = "EscenaCooperativo";
    public string escenaComp = "EscenaCompetitivo";

    [Header("Animación de Transición")]
    public List<GameObject> bolasVisuales;
    public float radioAparicion = 20f;
    public float velocidadBolas = 15f;

    private bool cuentaAtrasIniciada = false;
    private Coroutine corrutinaAnimacion;
    private List<Vector3> posicionesFinales = new List<Vector3>();
    private string escenaActualDestino = "";

    void Start()
    {
        foreach (GameObject bola in bolasVisuales)
        {
            posicionesFinales.Add(bola.transform.position);
            bola.SetActive(false);
        }
        // Nos aseguramos de empezar con el menú correcto
        ActualizarInterfaz(true, false, false);
    }

    void Update()
    {
        // Contamos cuánta gente hay en cada modo
        int playersCoop = (circuloCoop1.jugadorDentro ? 1 : 0) + (circuloCoop2.jugadorDentro ? 1 : 0);
        int playersComp = (circuloComp1.jugadorDentro ? 1 : 0) + (circuloComp2.jugadorDentro ? 1 : 0);

        // 1. COMPROBACIÓN DE ACTIVACIÓN (Estructura idéntica a la tuya que funciona 100% bien)
        if (playersCoop == 2)
        {
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                escenaActualDestino = escenaCoop;
                ActualizarInterfaz(false, false, false); // Apagamos interfaz para la transición
                corrutinaAnimacion = StartCoroutine(AnimarPiramideYCargar(escenaCoop));
            }
        }
        else if (playersComp == 2)
        {
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                escenaActualDestino = escenaComp;
                ActualizarInterfaz(false, false, false); // Apagamos interfaz para la transición
                corrutinaAnimacion = StartCoroutine(AnimarPiramideYCargar(escenaComp));
            }
        }

        // 2. COMPROBACIÓN DE CANCELACIÓN (Solo entramos aquí si estábamos animando y ya NO hay 2 jugadores en el modo elegido)
        else if (cuentaAtrasIniciada)
        {
            cuentaAtrasIniciada = false;
            escenaActualDestino = "";
            if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);

            EsconderBolas(); // Apaga las bolas visuales
            ActualizarInterfaz(true, false, false); // IMPORTANTE: Devolvemos la UI principal al cancelar
            Debug.Log("Animación cancelada. ¡Un jugador se ha salido!");
        }

        // 3. LÓGICA DE MENÚS DINÁMICOS (Solo se ejecuta si nadie está jugando ni animando la pirámide)
        else
        {
            if (playersCoop == 1 && playersComp == 0)
            {
                ActualizarInterfaz(false, true, false); // Muestra solo instrucciones Coop
            }
            else if (playersComp == 1 && playersCoop == 0)
            {
                ActualizarInterfaz(false, false, true); // Muestra solo instrucciones Comp
            }
            else
            {
                ActualizarInterfaz(true, false, false); // Pantalla de título por defecto
            }
        }
    }

    private void ActualizarInterfaz(bool mostrarPrincipal, bool mostrarCoop, bool mostrarComp)
    {
        // Solo actualizamos si hay un cambio, para no gastar recursos
        if (uiPrincipal != null && uiPrincipal.activeSelf != mostrarPrincipal) uiPrincipal.SetActive(mostrarPrincipal);
        if (uiCoop != null && uiCoop.activeSelf != mostrarCoop) uiCoop.SetActive(mostrarCoop);
        if (uiComp != null && uiComp.activeSelf != mostrarComp) uiComp.SetActive(mostrarComp);
    }

    // Le pasamos la escena como parámetro para que sepa adónde ir
    private IEnumerator AnimarPiramideYCargar(string escenaDestinoFinal)
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

    private void EsconderBolas()
    {
        foreach (GameObject bola in bolasVisuales)
        {
            bola.SetActive(false);
        }
    }

}
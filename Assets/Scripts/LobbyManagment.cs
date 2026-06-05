using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    //Referencias a las zonas físicas donde se colocan los jugadores 
    [Header("Círculos de Modos")]
    public DetectorCaja circuloCoop1;  // zona 1 del modo cooperativo
    public DetectorCaja circuloCoop2;  // zona 2 del modo cooperativo
    public DetectorCaja circuloComp1;  // zona 1 del modo competitivo
    public DetectorCaja circuloComp2;  // zona 2 del modo competitivo

    // Paneles de UI que se activan/desactivan según el estado 
    [Header("Grupos de Interfaz")]
    public GameObject uiPrincipal; 
    public GameObject uiCoop;      
    public GameObject uiComp;      

    // Nombres de las escenas a las que se salta tras la cuenta atrás 
    [Header("Escenas Intermedias de Destino")]
    public string escenaInterCoop = "IntermediaCoop";
    public string escenaInterComp = "IntermediaComp";

    private bool cuentaAtrasIniciada = false;  // evita lanzar la transición más de una vez
    private Coroutine corrutinaTransicion;     

    void Start()
    {
        // Al arrancar mostramos solo la UI principal, el resto oculto
        ActualizarInterfaz(true, false, false);
    }

    void Update()
    {
        // Contamos cuántos jugadores hay en cada zona (0, 1 o 2)
        int playersCoop = (circuloCoop1.jugadorDentro ? 1 : 0) + (circuloCoop2.jugadorDentro ? 1 : 0);
        int playersComp = (circuloComp1.jugadorDentro ? 1 : 0) + (circuloComp2.jugadorDentro ? 1 : 0);

        if (playersCoop == 2)
        {
            // Los 2 jugadores están en las zonas coop, iniciamos transición si no había empezado
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                ActualizarInterfaz(false, false, false); 
                corrutinaTransicion = StartCoroutine(EsperarYCargar(escenaInterCoop));
            }
        }
        else if (playersComp == 2)
        {
            // Los 2 jugadores están en las zonas competitivo, iniciamos la transición pero para comp
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                ActualizarInterfaz(false, false, false);
                corrutinaTransicion = StartCoroutine(EsperarYCargar(escenaInterComp));
            }
        }
        else if (cuentaAtrasIniciada)
        {
            // Había una transición en curso pero un jugador se salió, cancelamos
            cuentaAtrasIniciada = false;
            if (corrutinaTransicion != null) StopCoroutine(corrutinaTransicion);
            ActualizarInterfaz(true, false, false); // volvemos a la UI principal
            Debug.Log("Transición cancelada. ¡Un jugador se ha salido!");
        }
        else
        {

            if (playersCoop == 1 && playersComp == 0)
                ActualizarInterfaz(false, true, false);  
            else if (playersComp == 1 && playersCoop == 0)
                ActualizarInterfaz(false, false, true);  
            else
                ActualizarInterfaz(true, false, false);  
        }
    }

    private void ActualizarInterfaz(bool mostrarPrincipal, bool mostrarCoop, bool mostrarComp)
    {
        if (uiPrincipal != null && uiPrincipal.activeSelf != mostrarPrincipal) uiPrincipal.SetActive(mostrarPrincipal);
        if (uiCoop != null && uiCoop.activeSelf != mostrarCoop) uiCoop.SetActive(mostrarCoop);
        if (uiComp != null && uiComp.activeSelf != mostrarComp) uiComp.SetActive(mostrarComp);
    }

    // Espera 2 segundos y carga la escena destino
    private IEnumerator EsperarYCargar(string escenaDestino)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(escenaDestino);
    }
}
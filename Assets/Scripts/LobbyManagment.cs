using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Círculos de Modos")]
    public DetectorCaja circuloCoop1; 
    public DetectorCaja circuloCoop2;
    public DetectorCaja circuloComp1; 
    public DetectorCaja circuloComp2; 

    [Header("Grupos de Interfaz")]
    public GameObject uiPrincipal;
    public GameObject uiCoop;
    public GameObject uiComp;

    [Header("Escenas Intermedias de Destino")]
    public string escenaInterCoop = "IntermediaCoop";
    public string escenaInterComp = "IntermediaComp";

    private bool cuentaAtrasIniciada = false;
    private Coroutine corrutinaTransicion;

    void Start()
    {
        ActualizarInterfaz(true, false, false);
    }

    void Update()
    {
        int playersCoop = (circuloCoop1.jugadorDentro ? 1 : 0) + (circuloCoop2.jugadorDentro ? 1 : 0);
        int playersComp = (circuloComp1.jugadorDentro ? 1 : 0) + (circuloComp2.jugadorDentro ? 1 : 0);

        if (playersCoop == 2)
        {
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                ActualizarInterfaz(false, false, false);
                corrutinaTransicion = StartCoroutine(EsperarYCargar(escenaInterCoop));
            }
        }
        else if (playersComp == 2)
        {
            if (!cuentaAtrasIniciada)
            {
                cuentaAtrasIniciada = true;
                ActualizarInterfaz(false, false, false);
                corrutinaTransicion = StartCoroutine(EsperarYCargar(escenaInterComp));
            }
        }
        else if (cuentaAtrasIniciada)
        {
            cuentaAtrasIniciada = false;
            if (corrutinaTransicion != null) StopCoroutine(corrutinaTransicion);
            ActualizarInterfaz(true, false, false);
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

    private IEnumerator EsperarYCargar(string escenaDestino)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(escenaDestino);
    }
}
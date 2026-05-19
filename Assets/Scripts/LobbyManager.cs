using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject panelLobby; 
    public GameObject panelCarga; 
    public Slider barraProgreso;  

    private bool p1Listo = false;
    private bool p2Listo = false;
    private bool cargando = false;

    void Start()
    {
        
        panelLobby.SetActive(true);
        panelCarga.SetActive(false);

       
        Time.timeScale = 0;
    }

    public void ActualizarEstadoJugador(int id, bool estaDentro)
    {
        if (cargando) return; 

        if (id == 1) p1Listo = estaDentro;
        if (id == 2) p2Listo = estaDentro;

        // Si ambos están en sus zonas, empieza la transición
        if (p1Listo && p2Listo)
        {
            StartCoroutine(ProcesoDeCarga());
        }
    }

    IEnumerator ProcesoDeCarga()
    {
        cargando = true;
        panelLobby.SetActive(false); 
        panelCarga.SetActive(true);  

        float tiempo = 0;
        float duracion = 3.0f; 

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            barraProgreso.value = tiempo / duracion; 
            yield return null;
        }

        
        panelCarga.SetActive(false);
        Time.timeScale = 1; 
        Debug.Log("¡A jugar!");
    }
}
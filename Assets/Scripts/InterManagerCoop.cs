using UnityEngine;
using UnityEngine.SceneManagement;

public class InterManagerCoop : MonoBehaviour
{
    [Header("Círculos Modo Fácil (Verdes)")]
    public DetectorCaja facilP1;
    public DetectorCaja facilP2;

    [Header("Círculos Modo Difícil (Rojos)")]
    public DetectorCaja dificilP1;
    public DetectorCaja dificilP2;

    [Header("Botón de Salida")]
    public DetectorCaja botonBack;

    [Header("Configuración de Carga")]
    public string escenaFacil = "EscenaCooperativo";
    public string escenaDificil = "EscenaCoopDificil"; // La crearemos luego
    public string escenaLobby = "StartScene";
    public AnimadorPiramide animador;

    private bool transicionIniciada = false;

    void Update()
    {
        // 1. ¿Alguien ha pisado el botón de Back?
        if (botonBack.jugadorDentro)
        {
            if (transicionIniciada) animador.CancelarAnimacion();
            SceneManager.LoadScene(escenaLobby);
            return;
        }

        // 2. Comprobamos si las parejas están listas
        bool facilListo = facilP1.jugadorDentro && facilP2.jugadorDentro;
        bool dificilListo = dificilP1.jugadorDentro && dificilP2.jugadorDentro;

        // 3. Lógica de Activación
        if (facilListo)
        {
            if (!transicionIniciada)
            {
                transicionIniciada = true;
                animador.IniciarAnimacion(escenaFacil);
            }
        }
        else if (dificilListo)
        {
            if (!transicionIniciada)
            {
                transicionIniciada = true;
                animador.IniciarAnimacion(escenaDificil);
            }
        }
        else
        {
            // Si falta alguien y la transición estaba en marcha, la cancelamos
            if (transicionIniciada)
            {
                transicionIniciada = false;
                animador.CancelarAnimacion();
                Debug.Log("¡Un jugador se movió! Carga de nivel cancelada.");
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterManagerComp : MonoBehaviour
{
    [Header("Detectores Físicos")]
    public DetectorCaja cajaP1;
    public DetectorCaja cajaP2;
    public DetectorCaja botonBack;

    [Header("Configuración de Carga")]
    public string escenaJuego = "Competitivo";
    public string escenaLobby = "StartScene";
    public AnimadorPiramide animador;

    private bool transicionIniciada = false;

    void Update()
    {
        // 1. ¿Alguien ha pisado el botón de Back? (Prioridad absoluta)
        if (botonBack.jugadorDentro)
        {
            if (transicionIniciada) animador.CancelarAnimacion();
            SceneManager.LoadScene(escenaLobby);
            return;
        }

        // 2. Comprobamos el estado de los jugadores
        bool p1Listo = cajaP1.jugadorDentro;
        bool p2Listo = cajaP2.jugadorDentro;

        // 3. Lógica de Activación y Cancelación
        if (p1Listo && p2Listo)
        {
            // Si están los dos pero la transición no había empezado, la arrancamos
            if (!transicionIniciada)
            {
                transicionIniciada = true;
                animador.IniciarAnimacion(escenaJuego);
            }
        }
        else
        {
            // Si falta alguien y la transición estaba en marcha, la cancelamos
            if (transicionIniciada)
            {
                transicionIniciada = false;
                animador.CancelarAnimacion();
                Debug.Log("¡Un jugador salió de su caja! Carga de nivel cancelada.");
            }
        }
    }
}
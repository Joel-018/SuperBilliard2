using UnityEngine;

public class ReadyZone : MonoBehaviour
{
    public int jugadorID; // 1 o 2
    public LobbyManager lobbyManager;

    void OnTriggerEnter(Collider other)
    {
        // Comprobamos si lo que entra es la bola avatar del jugador
        if (other.CompareTag("Player"))
        {
            // CAMBIO: Usamos el nombre que pusimos en LobbyManager
            lobbyManager.ActualizarEstadoJugador(jugadorID, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // CAMBIO: Usamos el nombre que pusimos en LobbyManager
            lobbyManager.ActualizarEstadoJugador(jugadorID, false);
        }
    }
}
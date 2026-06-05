using UnityEngine;

public class DetectorCaja : MonoBehaviour
{
    [Tooltip("Escribe aquí el Tag que debe detectar (Ej: Player1, Player2, o Player)")]
    public string tagBuscado = "Player";

    public bool jugadorDentro = false;

    // Detecta cuando un objeto entra en el trigger y marca que el jugador está dentro si coincide con el tag buscado
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagBuscado))
            jugadorDentro = true;
    }

    // Detecta cuando un objeto sale del trigger y marca que el jugador ya no está dentro si coincide con el tag buscado
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagBuscado))
            jugadorDentro = false;
    }
}
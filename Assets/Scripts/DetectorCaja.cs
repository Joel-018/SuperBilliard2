using UnityEngine;

public class DetectorCaja : MonoBehaviour
{
    [Tooltip("Escribe aquí el Tag que debe detectar (Ej: Player1, Player2, o Player)")]
    public string tagBuscado = "Player";

    public bool jugadorDentro = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagBuscado))
            jugadorDentro = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagBuscado))
            jugadorDentro = false;
    }
}
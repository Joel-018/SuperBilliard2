using UnityEngine;

public class DetectorInicio : MonoBehaviour
{
    public bool jugadorDentro = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si el jugador se sale del círculo antes de que el otro llegue, se cancela
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
        }
    }
}

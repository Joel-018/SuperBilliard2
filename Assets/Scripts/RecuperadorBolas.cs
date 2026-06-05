using UnityEngine;

public class RecuperadorDeBolasComp : MonoBehaviour
{
    // Creamos el canal (Instance) para que BallMovementComp pueda llamarlo
    public static RecuperadorDeBolasComp Instance;

    [Tooltip("Arrastra aquí los 3 puntos de respawn")]
    public Transform[] puntosRespawn;

    private void Awake()
    {
        // Configuramos el candado del Singleton
        if (Instance == null) Instance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si una bola se sale del mapa por fuerza, la respawneamos aquí mismo
        if (other.CompareTag("Ball") || other.CompareTag("Ball8"))
        {
            Vector3 puntoAleatorio = ObtenerPuntoAleatorio();
            other.transform.position = puntoAleatorio + Vector3.up * 1.0f;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log($"¡Oops! La bola {other.name} se salió del mapa. Rescatada en punto aleatorio.");
        }
    }

    // FUNCIÓN: Cualquiera puede llamarla para obtener una posición al azar
    public Vector3 ObtenerPuntoAleatorio()
    {
        if (puntosRespawn != null && puntosRespawn.Length > 0)
        {
            int r = Random.Range(0, puntosRespawn.Length);
            return puntosRespawn[r].position;
        }
        return Vector3.zero; 
    }
}
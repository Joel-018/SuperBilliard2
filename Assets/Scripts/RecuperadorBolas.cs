using UnityEngine;

public class RecuperadorDeBolas : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Comprobamos si lo que ha chocado con el cubo es una bola normal o la bola 8
        if (other.CompareTag("Ball") || other.CompareTag("Ball8"))
        {
            // 2. Buscamos la posición del centro del tablero usando tu GameModeManager
            Transform centro = GameModeManager.Instance.centroDelTablero;

            if (centro != null)
            {
                // 3. Teletransportamos la bola. Le sumamos un poco de altura (Vector3.up * 1.0f) 
                // para que caiga limpiamente desde arriba y no se atasque dentro de la mesa.
                other.transform.position = centro.position + Vector3.up * 1.0f;

                // 4. Frenamos la bola por completo. 
                // Si no hacemos esto, la bola reaparecería en el centro pero conservando 
                // la velocidad con la que salió volando.
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero; 
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"¡Oops! La bola {other.name} se salió del mapa. Devuelta al centro.");
            }
            else
            {
                Debug.LogWarning("No se encontró 'Centro Del Tablero' en GameModeManager.");
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agujero : MonoBehaviour
{
    [Tooltip("Punto central y profundo del agujero hacia donde caerá la bola")]
    public Transform fondoDelAgujero;

    [Tooltip("Velocidad de la animación de caída")]
    public float velocidadCaida = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") || other.CompareTag("Ball8"))
        {
            // 1. Conseguimos el Rigidbody de la bola para quitarle el control físico
            Rigidbody rbBola = other.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                // La volvemos Kinematic: la gravedad normal y los choques dejan de afectarle
                rbBola.isKinematic = true;
            }
        }
    }
}

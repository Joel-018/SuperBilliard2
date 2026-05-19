using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour 
{
    [Tooltip("Punto central y profundo del agujero hacia donde caerá la bola")]
    public Transform fondoDelAgujero;

    [Tooltip("Velocidad de la animación de caída")]
    public float velocidadCaida = 5f;

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si es una bola
        if (other.CompareTag("Ball") || other.CompareTag("Ball8"))
        {
            // --- EL CANDADO ANTI-DOBLE PUNTUACIÓN ---
            // Le quitamos la etiqueta a la bola al instante. 
            // Si el motor detecta otro choque un milisegundo después, ya no entrará en este 'if'.
            other.tag = "Untagged"; 
            // ----------------------------------------

            Debug.Log("¡La bola ha tocado el agujero!"); 

            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.OnBallPotted();
            }

            // Quitamos la física a la bola
            Rigidbody rbBola = other.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                rbBola.isKinematic = true;
            }
        }
    }
}
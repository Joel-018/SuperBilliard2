using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EL NOMBRE DEBE SER Hole PARA QUE COINCIDA CON Hole.cs
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
            // ESTO NOS AVISARÁ EN LA CONSOLA SI LA COLISIÓN FUNCIONA
            Debug.Log("¡La bola ha tocado el agujero!"); 

            if (GameModeManager.Instance != null)
            {
                Debug.Log("¡Llamando al GameModeManager para sumar tiempo!");
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
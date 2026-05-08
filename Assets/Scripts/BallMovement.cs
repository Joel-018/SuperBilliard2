using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private SphereCollider colisionador;

    public Collider colisionadorMesa;

    private AudioSource altavoz;
    public AudioClip sonidoAgujero;
    private bool yaHaCaido = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colisionador = GetComponent<SphereCollider>();
        altavoz = GetComponent<AudioSource>();
    }

    


    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole"))
        {
            yaHaCaido = true;
            if (sonidoAgujero != null)
            {
                altavoz.PlayOneShot(sonidoAgujero);
            }

            // NUEVO: Quitamos las restricciones del Rigidbody (desmarca la casilla Freeze Position Y)
            rb.constraints = RigidbodyConstraints.None;

            // Hacemos que ignore la mesa para que pueda caer a trav�s de ella
            if (colisionadorMesa != null)
            {
                Physics.IgnoreCollision(colisionador, colisionadorMesa);
            }
        }
    }
}
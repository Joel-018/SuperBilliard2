using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour // Nota: en tu código original este script se llama PlayerController aunque el archivo es BallMovement.cs
{
    private Rigidbody rb;
    private AudioSource altavoz;

    [Header("Sonidos")]
    public AudioClip sonidoAgujero;
    [Tooltip("Sonido que se reproduce al chocar con otra bola o el jugador")]
    public AudioClip sonidoChoque;

    [Header("Escala del Agujero")]
    public float targetScale; // 1
    public float timeToReachTarget; // 2
    private float startScale;  // 3
    private float percentScaled; // 4
    private bool check = true; // 4

    [Header("Físicas de Colisión")]
    [Tooltip("Velocidad directa que recibe la bola al ser golpeada por la blanca")]
    public float fuerzaImpulso = 10f; // Ajusta este valor en Unity según necesites (empieza probando con 10 o 15)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        altavoz = GetComponent<AudioSource>();
        startScale = transform.localScale.x;
    }

    // --- NUEVO CÓDIGO: Detectar colisiones para el sonido y el empuje ---
    void OnCollisionEnter(Collision colision)
    {
        // 1. REPRODUCIR SONIDO DE CHOQUE
        // Comprobamos si con lo que hemos chocado es el jugador u otra bola
        if (colision.gameObject.CompareTag("Player") || colision.gameObject.CompareTag("Ball") || colision.gameObject.CompareTag("Ball8"))
        {
            if (sonidoChoque != null && altavoz != null)
            {
                altavoz.PlayOneShot(sonidoChoque);
            }
        }

        // 2. EMPUJE FÍSICO (Solo si nos golpea el jugador con WASD/Tracker)
        if (colision.gameObject.CompareTag("Player")) 
        {
            Vector3 direccionEmpuje = transform.position - colision.transform.position;
            direccionEmpuje.y = 0; // Evitamos que salte hacia arriba
            direccionEmpuje.Normalize();
            rb.linearVelocity = direccionEmpuje * fuerzaImpulso;
        }
    }
    // ---------------------------------------------------------------------

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole"))
        {
            if (otro.CompareTag("Wall"))
            {
                rb.isKinematic = true;
            }
          
            if (sonidoAgujero != null)
            {
                altavoz.PlayOneShot(sonidoAgujero);
            }

            StartCoroutine(CaerPorElAgujero()); // Mantenemos la Corrutina que me has pasado
        }
    }

    // Corrutina para animar la caída de forma fluida
    IEnumerator CaerPorElAgujero()
    {
        while (check)
        {
            if (percentScaled < 1f)
            {
                percentScaled += Time.deltaTime / timeToReachTarget;
                float scale = Mathf.Lerp(startScale, targetScale, percentScaled);
                transform.localScale = new Vector3(scale, scale, scale);
                yield return null; // Espera al siguiente frame
            }
            else
            {
                check = false;
            }
        }
        Destroy(gameObject, targetScale);
    }
}
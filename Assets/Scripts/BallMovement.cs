using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource altavoz;
    private Collider miCollider;

    [Header("Sonidos")]
    public AudioClip sonidoAgujero;
    public AudioClip sonidoChoque;

    [Header("Escala del Agujero")]
    public float targetScale;
    public float timeToReachTarget;
    private float startScale;
    private float percentScaled;
    private bool check = true;

    [Header("Físicas de Colisión")]
    public float fuerzaImpulso;
    public float tiempoAntiArrastre = 2f;

    private List<Collider> jugadoresEnCooldown = new List<Collider>();
    
    private bool yaHaCaido = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        altavoz = GetComponent<AudioSource>();
        startScale = transform.localScale.x;
        miCollider = GetComponent<Collider>();
    }

    // Colisiones físicas, reproduce sonidos de choque y aplica impulso a la bola si es empujada por un jugador
    void OnCollisionEnter(Collision colision)
    {
        if (colision.gameObject.CompareTag("Player") || colision.gameObject.CompareTag("Ball") || colision.gameObject.CompareTag("Ball8"))
        {
            if (sonidoChoque != null && altavoz != null) altavoz.PlayOneShot(sonidoChoque);
        }

        if (colision.gameObject.CompareTag("Player"))
        {
            Collider playerCollider = colision.collider;

            if (jugadoresEnCooldown.Contains(playerCollider)) return;

            Vector3 direccionEmpuje = transform.position - colision.transform.position;
            direccionEmpuje.y = 0;
            direccionEmpuje.Normalize();
            rb.linearVelocity = direccionEmpuje * fuerzaImpulso;

            StartCoroutine(ActivarCooldownJugador(playerCollider));
        }
    }

    ////Desactiva temporalmente las colisiones con el jugador que chutó para evitar efectos de arrastre.
    private IEnumerator ActivarCooldownJugador(Collider playerCollider)
    {
         yield return new WaitForSeconds(2f);
         jugadoresEnCooldown.Add(playerCollider);
         Physics.IgnoreCollision(miCollider, playerCollider, true);
         yield return new WaitForSeconds(tiempoAntiArrastre);
         Physics.IgnoreCollision(miCollider, playerCollider, false);
         jugadoresEnCooldown.Remove(playerCollider);
     }

    //// Detecta la entrada en el agujero, desactiva la física y determina si la partida se gana, se pierde (bola 8 antes de tiempo)
    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole") && !yaHaCaido)
        {
            yaHaCaido = true; // Ya no volverá a contar esta bola.
            rb.isKinematic = true;

            if (gameObject.CompareTag("Ball8"))
            {
                if (GameModeManager.Instance.totalBalls > 0)
                {
                    GameModeManager.Instance.LoseGame("8-Ball potted too early!\nTry again!");
                }
                else
                {
                    GameModeManager.Instance.WinGame();
                }
            }
            else if (gameObject.CompareTag("Ball"))
            {
                GameModeManager.Instance.OnBallPotted();
            }

            if (sonidoAgujero != null) altavoz.PlayOneShot(sonidoAgujero);
            StartCoroutine(CaerPorElAgujero());
        }
    }

    // Animación visual de encogimiento de la bola simulando la caída y destruye el objeto al finalizar
    IEnumerator CaerPorElAgujero()
    {
        while (check)
        {
            if (percentScaled < 1f)
            {
                percentScaled += Time.deltaTime / timeToReachTarget;
                float scale = Mathf.Lerp(startScale, targetScale, percentScaled);
                transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }
            else
            {
                check = false;
            }
        }
        Destroy(gameObject);
    }
}
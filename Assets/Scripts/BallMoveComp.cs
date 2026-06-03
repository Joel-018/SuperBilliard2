using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovComp : MonoBehaviour
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

    [Header("Efectos Visuales")]
    public TrailRenderer estela;
    public Color colorP1 = new Color(1.0f, 0.0f, 1.0f, 1f);
    public Color colorP2 = new Color(0.2f, 1.0f, 0.9f, 1f); 

    private List<Collider> jugadoresEnCooldown = new List<Collider>();

    private bool yaHaCaido = false;
    public string ultimoJugador = "";
    private Coroutine rutinaExpiracion;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        altavoz = GetComponent<AudioSource>();
        startScale = transform.localScale.x;
        miCollider = GetComponent<Collider>();
    }

    private void ActualizarEstela()
    {
        if (estela == null) return;

        if (ultimoJugador == "Player1")
        {
            estela.startColor = colorP1;
            estela.endColor = new Color(colorP1.r, colorP1.g, colorP1.b, 0f); // Se desvanece a transparente
            estela.emitting = true;
        }
        else if (ultimoJugador == "Player2")
        {
            estela.startColor = colorP2;
            estela.endColor = new Color(colorP2.r, colorP2.g, colorP2.b, 0f);
            estela.emitting = true;
        }
        else
        {
            // Si la bola no es de nadie, apagamos la estela
            estela.emitting = false;
        }
    }

    void OnCollisionEnter(Collision colision)
    {
        string tagChoque = colision.gameObject.tag;

        if (tagChoque == "Player1" || tagChoque == "Player2")
        {
            // 1. FASE DE ZOMBIE: Si el jugador está aturdido, no puede interactuar con la bola
            bool aturdido = (tagChoque == "Player1" && GMManagerComp.Instance.isPlayer1Stunned) ||
                            (tagChoque == "Player2" && GMManagerComp.Instance.isPlayer2Stunned);

            // Si está aturdido, cortamos aquí. La bola rebotará en él como si fuera una pared de piedra.
            if (aturdido) return;

            // 2. FASE DE PELOTAZO: ¿La bola es del rival y va a cierta velocidad?
            // (linearVelocity.magnitude > 1.5f asegura que la bola lleva impulso real)
            if (ultimoJugador != "" && ultimoJugador != tagChoque && rb.linearVelocity.magnitude > 1.5f)
            {
                Debug.Log($"¡ZASCA! {ultimoJugador} le ha pegado un pelotazo a {tagChoque}!");

                // Llamamos al Mánager para que haga el robo y el aturdimiento
                if (GMManagerComp.Instance != null)
                {
                    GMManagerComp.Instance.RobarPuntoYAturdir(ultimoJugador, tagChoque);
                }

                // Borramos la memoria de la bola para que no robe puntos 2 veces seguidas rebotando
                ultimoJugador = "";
                return; // Cortamos aquí para que la víctima no pueda chutarla de vuelta
            }

            // 3. FASE NORMAL: El jugador toca la bola para chutarla
            ultimoJugador = tagChoque;

            ActualizarEstela();

            if (rutinaExpiracion != null) StopCoroutine(rutinaExpiracion);
            rutinaExpiracion = StartCoroutine(ExpirarUltimoJugador());

            if (sonidoChoque != null && altavoz != null) altavoz.PlayOneShot(sonidoChoque);

            Collider playerCollider = colision.collider;
            if (jugadoresEnCooldown.Contains(playerCollider)) return;

            Vector3 direccionEmpuje = transform.position - colision.transform.position;
            direccionEmpuje.y = 0;
            direccionEmpuje.Normalize();
            rb.linearVelocity = direccionEmpuje * fuerzaImpulso;

            StartCoroutine(ActivarCooldownJugador(playerCollider));
        }
        else if (tagChoque == "Ball" || tagChoque == "Ball8")
        {
            if (sonidoChoque != null && altavoz != null) altavoz.PlayOneShot(sonidoChoque);
        }
    }

    private IEnumerator ActivarCooldownJugador(Collider playerCollider)
    {
        yield return new WaitForSeconds(2f);
        jugadoresEnCooldown.Add(playerCollider);
        Physics.IgnoreCollision(miCollider, playerCollider, true);
        yield return new WaitForSeconds(tiempoAntiArrastre);
        Physics.IgnoreCollision(miCollider, playerCollider, false);
        jugadoresEnCooldown.Remove(playerCollider);
    }

    private IEnumerator ExpirarUltimoJugador()
    {
        // Esperamos 5 segundos
        yield return new WaitForSeconds(5f);

        // Borramos la memoria
        ultimoJugador = "";
        Debug.Log($"La bola {gameObject.name} ya no es de nadie (han pasado 5s).");
    }

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole") && !yaHaCaido)
        {
            yaHaCaido = true;
            rb.isKinematic = true;

            // Mandamos los puntos al Manager dándole el Tag de esta bola y el string de quién la tocó
            if (GMManagerComp.Instance != null)
            {
                GMManagerComp.Instance.OnBallPottedComp(gameObject.tag, ultimoJugador);
            }

            if (sonidoAgujero != null) altavoz.PlayOneShot(sonidoAgujero);
            StartCoroutine(CaerPorElAgujero());
        }
    }

    IEnumerator CaerPorElAgujero()
    {
        // 1. Animación de encogerse (idéntica)
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

        // 2. ¿Muerte Súbita o Reaparición?
        if (!GMManagerComp.Instance.faseMuerteSubita)
        {
            // FASE NORMAL: Pedimos el punto aleatorio al Recuperador de Bolas
            if (RecuperadorDeBolasComp.Instance != null)
            {
                Vector3 punto = RecuperadorDeBolasComp.Instance.ObtenerPuntoAleatorio();
                transform.position = punto + (Vector3.up * 0.5f);
            }

            // Reseteamos la bola para que vuelva a estar operativa
            transform.localScale = new Vector3(startScale, startScale, startScale);
            percentScaled = 0f;
            check = true;
            yaHaCaido = false;
            ultimoJugador = "";

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            // FASE MUERTE SÚBITA: El tiempo es 0, la bola se elimina para siempre
            Destroy(gameObject);
        }
    }
}
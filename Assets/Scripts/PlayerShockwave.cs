using UnityEngine;

public class PlayerShockwave : MonoBehaviour
{
    [Header("Configuración de la Onda")]
    public float fuerzaExplosion = 500f;
    public float radioExplosion = 20f;
    public float cooldown = 5f;
    public float distanciaDeChoque = 3.0f;

    [Header("Sonido")]
    public AudioClip sonidoOnda;
    private AudioSource miAltavoz; // Nuestro propio altavoz

    [Header("DEBUG - MIRA ESTO MIENTRAS JUEGAS")]
    public bool FORZAR_EXPLOSION = false;
    public float distanciaEnTiempoReal = 0f;

    private static float proximoUso = 0f;

    void Start()
    {
        // Buscamos si el jugador ya tiene un altavoz. Si no, le creamos uno invisible.
        miAltavoz = GetComponent<AudioSource>();
        if (miAltavoz == null) miAltavoz = gameObject.AddComponent<AudioSource>();
        

        miAltavoz.spatialBlend = 0f; 


        // --- 2. EVITAMOS CHOQUES FÍSICOS ---
        Collider[] misColliders = GetComponentsInChildren<Collider>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p != gameObject)
            {
                Collider[] otrosColliders = p.GetComponentsInChildren<Collider>();
                foreach (Collider miC in misColliders)
                {
                    foreach (Collider otroC in otrosColliders)
                    {
                        Physics.IgnoreCollision(miC, otroC, true);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (FORZAR_EXPLOSION)
        {
            FORZAR_EXPLOSION = false; 
            Debug.Log("¡BOMBA FORZADA DESDE EL INSPECTOR!");
            proximoUso = Time.time + cooldown;
            EjecutarOnda(transform.position);
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p != gameObject)
            {
                distanciaEnTiempoReal = Vector3.Distance(transform.position, p.transform.position);

                if (Time.time >= proximoUso && distanciaEnTiempoReal <= distanciaDeChoque)
                {
                    proximoUso = Time.time + cooldown;
                    Vector3 puntoMedio = (transform.position + p.transform.position) / 2f;
                    EjecutarOnda(puntoMedio);
                    return;
                }
            }
        }
    }

    private void EjecutarOnda(Vector3 origen)
    {
        Debug.Log("💥 ¡BOOM! Ejecutando onda expansiva...");

        // --- REPRODUCCIÓN DEL SONIDO ---
        if (sonidoOnda != null && miAltavoz != null) 
        {
            miAltavoz.PlayOneShot(sonidoOnda);
        }
        else if (sonidoOnda == null)
        {
            Debug.LogWarning("⚠️ No has arrastrado ningún Audio Clip en el hueco 'Sonido Onda' del Inspector.");
        }

        GameObject[] bolasNormales = GameObject.FindGameObjectsWithTag("Ball");
        GameObject GameObjectBola8 = GameObject.FindGameObjectWithTag("Ball8");

        AplicarFuerzaExplosionPlana(bolasNormales, origen);
        if (GameObjectBola8 != null) AplicarFuerzaExplosionPlana(new GameObject[] { GameObjectBola8 }, origen);
    }

    private int AplicarFuerzaExplosionPlana(GameObject[] bolas, Vector3 origen)
    {
        int contador = 0;
        foreach (GameObject bola in bolas)
        {
            Rigidbody rb = bola.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                Vector3 direccionVector = bola.transform.position - origen;
                direccionVector.y = 0f;

                float distanciaABola = direccionVector.magnitude;

                if (distanciaABola <= radioExplosion)
                {
                    if (distanciaABola == 0) direccionVector = Vector3.forward;

                    direccionVector.Normalize();
                    float fuerzaProporcional = fuerzaExplosion * (1f - (distanciaABola / radioExplosion));
                    rb.AddForce(direccionVector * fuerzaProporcional, ForceMode.Impulse);
                    contador++;
                }
            }
        }
        return contador;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeChoque / 2f);
    }
}
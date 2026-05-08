using UnityEngine;

public class TrackerAvatarController : MonoBehaviour
{
    public Transform sensorTracker; //el objeto sigue el movimiento del player
    public Collider colisionadorMesa;

    private Rigidbody rb;
    private SphereCollider colisionador;

    public float alturaMesa = 0.5f; // Ajusta según la altura de tu mesa en Unity 
    private bool haCaido = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colisionador = GetComponent<SphereCollider>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;


    }

    void FixedUpdate()
    {
        if (!haCaido)
        {
            // Sigue la posición del sensorTracker
            transform.position = sensorTracker.position;
            // Verifica si el avatar ha caído por debajo de la altura de la mesa
            if (transform.position.y < alturaMesa)
            {
                haCaido = true;
                rb.isKinematic = false; // Permite que el avatar sea afectado por la física
                rb.useGravity = true; // Activa la gravedad para que caiga
                if (colisionadorMesa != null)
                {
                    Physics.IgnoreCollision(colisionador, colisionadorMesa); // Ignora la colisión con la mesa
                }
            }
        }
    }

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Hole") && !haCaido)
        {
            haCaido = true;
            rb.isKinematic = false; // Desactivamos el control del tracker
            rb.useGravity = true;   // Dejamos que caiga físicamente
            rb.constraints = RigidbodyConstraints.None;
        }
        if (colisionadorMesa != null)
        {
            Physics.IgnoreCollision(colisionador, colisionadorMesa);
        }
    }

}

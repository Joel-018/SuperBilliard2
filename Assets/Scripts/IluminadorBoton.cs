using UnityEngine;

public class IluminadorBoton : MonoBehaviour
{
    [Header("Conexiones (Se rellenan solas si están en el mismo objeto)")]
    public DetectorCaja detector;
    public MeshRenderer miMesh;

    [Header("Configuración de Color")]
    public Color colorEncendido = Color.white;

    private Color colorOriginal;

    void Start()
    {
        // Autocompletar si se nos olvida arrastrarlos en el Inspector
        if (detector == null) detector = GetComponent<DetectorCaja>();
        if (miMesh == null) miMesh = GetComponent<MeshRenderer>();

        // Guardamos el color apagado que le hayas puesto en Unity
        if (miMesh != null)
        {
            colorOriginal = miMesh.material.color;
        }
    }

    void Update()
    {
        if (detector != null && miMesh != null)
        {
            // Si hay alguien dentro, brilla. Si no, vuelve a su color apagado original.
            miMesh.material.color = detector.jugadorDentro ? colorEncendido : colorOriginal;
        }
    }
}
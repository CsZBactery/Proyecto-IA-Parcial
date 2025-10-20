using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class AgenteVision : MonoBehaviour
{
    // DOXYGEN: Componente NavMeshAgent para la navegación.
    private NavMeshAgent navMeshAgent;

    // DOXYGEN: Transform del objetivo a detectar y perseguir.
    public Transform targetPlayer;

    // DOXYGEN: Referencia al script o componente que gestiona el cono de visión.
    public VisionCone visionConeScript;

    // DOXYGEN: Distancia de detección del cono de visión.
    public float detectionRange = 10f;
    // DOXYGEN: Ángulo del cono de visión (por ejemplo, 60 grados para un cono de 120 grados total).
    public float visionAngle = 60f;

    // DOXYGEN: Duración de la persecución en segundos.
    public float chaseDuration = 3f;

    // DOXYGEN: Posición inicial del agente para el patrullaje (puntos extra).
    private Vector3 initialPosition;

    // DOXYGEN: Bandera para saber si el agente está persiguiendo.
    private bool isChasing = false;

    // DOXYGEN: Inicializa referencias y guarda la posición inicial.
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) Debug.LogError("NavMeshAgent no encontrado en AgenteVision.");

        initialPosition = transform.position; // Guarda la posición inicial para el extra
    }

    // DOXYGEN: Lógica de detección y cambio de comportamiento en cada frame.
    void Update()
    {
        if (targetPlayer == null || navMeshAgent == null) return;

        // d) Lógica del cono de visión
        // Puedes integrar aquí tu función previa de cono de visión
        // Ejemplo básico de detección:
        if (IsTargetInVisionCone(targetPlayer)) // Puedes usar tu script VisionCone aquí
        {
            if (!isChasing)
            {
                StartCoroutine(ChaseAndStop());
            }
        }

        // Si no está persiguiendo y está en el modo de patrullaje (extra)
        if (!isChasing && Vector3.Distance(transform.position, initialPosition) > navMeshAgent.stoppingDistance)
        {
            navMeshAgent.SetDestination(initialPosition);
        }
    }

    // DOXYGEN: Verifica si el objetivo está dentro del cono de visión.
    /// <param name="target">Transform del objetivo a verificar.</param>
    /// <returns>Verdadero si el objetivo está en el cono de visión, falso en caso contrario.</returns>
    private bool IsTargetInVisionCone(Transform target)
    {
        // d) Reutiliza tu lógica de VisionCone aquí.
        // Esto es un ejemplo simplificado:
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < detectionRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            if (angle < visionAngle)
            {
                // Opcional: Raycast para asegurarse de que no haya obstáculos
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionRange))
                {
                    if (hit.transform == target)
                    {
                        return true;
                    }
                }
                else
                {
                    return true; // Si no hay colisión, asumimos que se ve si no hay Raycast.
                }
            }
        }
        return false;
    }

    // DOXYGEN: Coroutine que gestiona la persecución y la detención.
    // d.2) Cuando detecta al Player/otro agente, lo persigue por 3 segundos.
    // d.Extra) Regresa a la posición inicial de patrullaje.
    private IEnumerator ChaseAndStop()
    {
        isChasing = true;
        navMeshAgent.isStopped = false; // Asegura que el agente pueda moverse

        float timer = 0f;
        while (timer < chaseDuration)
        {
            if (targetPlayer != null)
            {
                navMeshAgent.SetDestination(targetPlayer.position);
            }
            timer += Time.deltaTime;
            yield return null; // Espera un frame
        }

        // Se queda en la posición que esté.
        navMeshAgent.isStopped = true;
        isChasing = false;

        // d.Extra) Si se necesita regresar a la posición inicial después de perseguir
        // Descomenta la línea de abajo para activar el patrullaje de regreso
        // navMeshAgent.SetDestination(initialPosition); 
        // navMeshAgent.isStopped = false; // Asegura que el agente pueda moverse de nuevo
    }

    // DOXYGEN: Detecta la colisión con otro agente y lo destruye o daña.
    // e) Si el agente del cono de visión toca al otro agente, debe destruirlo o hacerle daño.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("OtroAgente"))
        {
            // Destruir al otro agente
            Destroy(other.gameObject);
            Debug.Log("¡Agente con visión ha destruido al otro agente!");

            // O hacerle daño (ejemplo)
            // HealthComponent health = other.GetComponent<HealthComponent>();
            // if (health != null)
            // {
            //     health.TakeDamage(20);
            // }
        }
    }

    // DOXYGEN: Método para activar o desactivar el cono de visión (útil para debug).
    /// <param name="state">Verdadero para activar, falso para desactivar.</param>
    public void SetVisionConeActive(bool state)
    {
        if (visionConeScript != null)
        {
            // visionConeScript.gameObject.SetActive(state); // O la lógica de tu cono
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class AgenteMovimiento : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform targetDestination;

    void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    void Update()
    {
        if (navMeshAgent != null && targetDestination != null)
        {
            // AÑADE ESTA LÍNEA DE VERIFICACIÓN
            // Solo establece el destino si el agente está activo y sobre la NavMesh.
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.SetDestination(targetDestination.position);
            }
        }
    }

    public void SetNewDestination(Vector3 newDestination)
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(newDestination);
        }
    }
}
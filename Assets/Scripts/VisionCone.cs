/*
 * =====================================================================================
 *
 * Filename:  VisionCone.cs
 *
 * Description:  Implementa la l�gica de un cono de visi�n para un agente 3D.
 * Detecta objetivos y se comunica con SteeringAgent para la persecuci�n.
 * Tambi�n genera una malla visual para representar el cono.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingenier�a del Conocimiento
 *
 * =====================================================================================
 */

using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform agentTransform;

    [Header("Configuraci�n del Cono de Visi�n")]
    public float visionRange = 10f;
    [Range(0f, 180f)]
    public float visionAngle = 60f;
    public LayerMask targetLayers; // Capa del objetivo a detectar (ej. "Player").
    public Transform targetGameObject;

    [Header("Configuraci�n Visual del Cono")]
    public Color noTargetColor = Color.green;
    public Color targetDetectedColor = Color.red;
    [Range(8, 64)]
    public int coneSegments = 32;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh visionMesh;
    private bool targetIsDetected = false;
    private SteeringAgent steeringAgent;

    void Awake()
    {
        // Obtiene la referencia al script de movimiento del agente.
        steeringAgent = agentTransform.GetComponent<SteeringAgent>();
        if (steeringAgent == null)
        {
            Debug.LogWarning("VisionCone: Script 'SteeringAgent' no encontrado en 'agentTransform'.");
        }

        // Prepara los componentes para dibujar la malla del cono.
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) { meshFilter = gameObject.AddComponent<MeshFilter>(); }
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }

        visionMesh = new Mesh();
        meshFilter.mesh = visionMesh;
        UpdateVisionMeshColor(noTargetColor);
    }

    void Update()
    {
        DetectTarget();
        UpdateVisionConeVisuals();

        // Comunica el resultado de la detecci�n al script de movimiento.
        if (targetIsDetected && targetGameObject != null && steeringAgent != null)
        {
            steeringAgent.target = targetGameObject; // Le asigna el objetivo.
        }
        else if (steeringAgent != null)
        {
            steeringAgent.target = null; // Le quita el objetivo para que se detenga.
        }
    }

    private void DetectTarget()
    {
        targetIsDetected = false;
        targetGameObject = null;

        // 1. Detecci�n por radio: encuentra todos los colliders en un �rea.
        Collider[] hitColliders = Physics.OverlapSphere(agentTransform.position, visionRange, targetLayers);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == agentTransform.gameObject) continue;

            // 2. Detecci�n por �ngulo: comprueba si el objeto est� dentro del cono.
            Vector3 directionToTarget = (hitCollider.transform.position - agentTransform.position).normalized;
            float angleToTarget = Vector3.Angle(agentTransform.forward, directionToTarget);

            if (angleToTarget < visionAngle)
            {
                // 3. Detecci�n por l�nea de visi�n: lanza un rayo para ver si hay paredes en medio.
                RaycastHit hit;
                if (Physics.Raycast(agentTransform.position, directionToTarget, out hit, visionRange, targetLayers | (1 << LayerMask.NameToLayer("Obstacle"))))
                {
                    if (hit.collider.gameObject == hitCollider.gameObject)
                    {
                        targetIsDetected = true;
                        targetGameObject = hit.transform;
                        break;
                    }
                }
            }
        }
        UpdateVisionMeshColor(targetIsDetected ? targetDetectedColor : noTargetColor);
    }

    // Genera la malla 3D (en el plano XZ) para el cono de visi�n.
    private void UpdateVisionConeVisuals()
    {
        visionMesh.Clear();
        Vector3[] vertices = new Vector3[coneSegments + 2];
        int[] triangles = new int[coneSegments * 3];
        vertices[0] = Vector3.zero;
        float currentAngle = -visionAngle;
        float angleIncrement = (visionAngle * 2) / coneSegments;

        for (int i = 0; i <= coneSegments; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
            Vector3 vertexPosition = rotation * Vector3.forward * visionRange;
            vertices[i + 1] = vertexPosition;

            if (i < coneSegments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            currentAngle += angleIncrement;
        }

        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    private void UpdateVisionMeshColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = color;
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. �C�mo se puede generar una malla (Mesh) en Unity mediante c�digo para visualizar un �rea?
     * - Se consult� el proceso de definir `v�rtices` y `tri�ngulos` para construir una forma
     * geom�trica y asignarla a un `MeshFilter`.
     * 2. �Cu�l es la forma correcta de combinar varias LayerMasks para un Raycast en Unity?
     * - Se investig� el uso del operador a nivel de bits `|` (OR) para que el rayo pueda
     * colisionar con m�s de una capa a la vez (ej. `targetLayers | obstacleLayer`).
     * ================================================================
     */
}
/*
 * =====================================================================================
 *
 * Filename:  VisionCone.cs
 *
 * Description:  Implementa la lógica de un cono de visión para un agente 3D.
 * Detecta objetivos y se comunica con SteeringAgent para la persecución.
 * También genera una malla visual para representar el cono.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingeniería del Conocimiento
 *
 * =====================================================================================
 */

using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform agentTransform;

    [Header("Configuración del Cono de Visión")]
    public float visionRange = 10f;
    [Range(0f, 180f)]
    public float visionAngle = 60f;
    public LayerMask targetLayers; // Capa del objetivo a detectar (ej. "Player").
    public Transform targetGameObject;

    [Header("Configuración Visual del Cono")]
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

        // Comunica el resultado de la detección al script de movimiento.
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

        // 1. Detección por radio: encuentra todos los colliders en un área.
        Collider[] hitColliders = Physics.OverlapSphere(agentTransform.position, visionRange, targetLayers);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == agentTransform.gameObject) continue;

            // 2. Detección por ángulo: comprueba si el objeto está dentro del cono.
            Vector3 directionToTarget = (hitCollider.transform.position - agentTransform.position).normalized;
            float angleToTarget = Vector3.Angle(agentTransform.forward, directionToTarget);

            if (angleToTarget < visionAngle)
            {
                // 3. Detección por línea de visión: lanza un rayo para ver si hay paredes en medio.
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

    // Genera la malla 3D (en el plano XZ) para el cono de visión.
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
     * 1. ¿Cómo se puede generar una malla (Mesh) en Unity mediante código para visualizar un área?
     * - Se consultó el proceso de definir `vértices` y `triángulos` para construir una forma
     * geométrica y asignarla a un `MeshFilter`.
     * 2. ¿Cuál es la forma correcta de combinar varias LayerMasks para un Raycast en Unity?
     * - Se investigó el uso del operador a nivel de bits `|` (OR) para que el rayo pueda
     * colisionar con más de una capa a la vez (ej. `targetLayers | obstacleLayer`).
     * ================================================================
     */
}
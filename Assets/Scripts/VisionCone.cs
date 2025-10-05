using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Transform agentTransform;

    [Header("Configuración del Cono de Visión")]
    public float visionRange = 10f;
    [Range(0f, 180f)]
    public float visionAngle = 60f;
    public LayerMask targetLayers;
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
        steeringAgent = agentTransform.GetComponent<SteeringAgent>();
        if (steeringAgent == null)
        {
            Debug.LogWarning("VisionCone: Script 'SteeringAgent' no encontrado en 'agentTransform'.");
        }

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

        if (targetIsDetected && targetGameObject != null && steeringAgent != null)
        {
            steeringAgent.target = targetGameObject;
        }
        else if (steeringAgent != null)
        {
            steeringAgent.target = null;
        }
    }

    private void DetectTarget()
    {
        targetIsDetected = false;
        targetGameObject = null;

        Collider[] hitColliders = Physics.OverlapSphere(agentTransform.position, visionRange, targetLayers);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == agentTransform.gameObject) continue;

            Vector3 directionToTarget = (hitCollider.transform.position - agentTransform.position).normalized;
            float angleToTarget = Vector3.Angle(agentTransform.forward, directionToTarget);

            if (angleToTarget < visionAngle)
            {
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
}
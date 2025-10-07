using UnityEngine;

// Requiere que el GameObject tenga estos componentes
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    private Mesh visionMesh;
    private MeshFilter meshFilter;
    private float fov;
    private float viewDistance;
    private int rayCount = 50; // Calidad del cono, más rayos = más suave

    void Awake()
    {
        visionMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = visionMesh;
    }

    public void SetVisionParameters(float fieldOfView, float distance)
    {
        fov = fieldOfView;
        viewDistance = distance;
        DrawVisionCone();
    }

    // E) Dibuja el cono de visión
    private void DrawVisionCone()
    {
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // El origen del cono es la posición de la torreta

        float currentAngle = -fov / 2;
        float angleIncrement = fov / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            vertices[i + 1] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * viewDistance;
            currentAngle += angleIncrement;
        }

        for (int i = 0, j = 0; i < rayCount; i++, j += 3)
        {
            triangles[j] = 0;
            triangles[j + 1] = i + 1;
            triangles[j + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }
}
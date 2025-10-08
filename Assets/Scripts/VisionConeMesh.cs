/*
 * =====================================================================================
 *
 * Filename:  VisionConeMesh.cs
 *
 * Description:  Script puramente visual que genera una malla (Mesh) en 2D para
 * representar el cono de visión de un enemigo (ej. una torreta).
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

// Requiere que el GameObject siempre tenga estos componentes para poder dibujar la malla.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    private Mesh visionMesh;
    private MeshFilter meshFilter;
    private float fov; // Field of View (ángulo de visión)
    private float viewDistance;
    private int rayCount = 50; // Calidad del cono, más rayos = más suave

    void Awake()
    {
        // Inicializa los componentes necesarios al empezar.
        visionMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = visionMesh;
    }

    /// <summary>
    /// Función pública para que otro script (como el de la torreta) le pase
    /// los parámetros de visión y ordene dibujar el cono.
    /// </summary>
    public void SetVisionParameters(float fieldOfView, float distance)
    {
        fov = fieldOfView;
        viewDistance = distance;
        DrawVisionCone();
    }

    /// <summary>
    /// Dibuja la malla del cono de visión calculando sus vértices y triángulos.
    /// </summary>
    private void DrawVisionCone()
    {
        // Un vértice en el origen, más un vértice por cada "rayo" del arco.
        Vector3[] vertices = new Vector3[rayCount + 2];
        // Cada sección del arco necesita un triángulo, y cada triángulo tiene 3 vértices.
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // El primer vértice es el origen del cono (la posición del objeto).

        float currentAngle = -fov / 2; // Empezamos a dibujar desde el ángulo izquierdo.
        float angleIncrement = fov / rayCount; // El ángulo que hay entre cada "rayo".

        // Bucle para crear los vértices del arco exterior del cono.
        for (int i = 0; i <= rayCount; i++)
        {
            // Calcula la posición de cada vértice usando trigonometría (a través de Quaternions).
            vertices[i + 1] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * viewDistance;
            currentAngle += angleIncrement;
        }

        // Bucle para definir los triángulos que unen el origen con los vértices del arco.
        for (int i = 0, j = 0; i < rayCount; i++, j += 3)
        {
            triangles[j] = 0;           // Vértice 1: el origen.
            triangles[j + 1] = i + 1; // Vértice 2: un punto del arco.
            triangles[j + 2] = i + 2; // Vértice 3: el siguiente punto del arco.
        }

        // Asigna la información calculada a la malla para que Unity la dibuje.
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals(); // Calcula las normales para una correcta iluminación.
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo se puede generar una malla (Mesh) en Unity mediante código?
     * - Se consultó el proceso de definir arrays de `vértices` y `triángulos` para construir
     * una forma geométrica y asignarla a un `MeshFilter`.
     * 2. ¿Cuál es la matemática para crear un arco o sector circular en una malla?
     * - Se investigó el uso de `Quaternion.Euler` en un bucle para calcular las posiciones
     * de los vértices a lo largo de un arco.
     * ================================================================
     */
}
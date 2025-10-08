/*
 * =====================================================================================
 *
 * Filename:  VisionConeMesh.cs
 *
 * Description:  Script puramente visual que genera una malla (Mesh) en 2D para
 * representar el cono de visi�n de un enemigo (ej. una torreta).
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

// Requiere que el GameObject siempre tenga estos componentes para poder dibujar la malla.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    private Mesh visionMesh;
    private MeshFilter meshFilter;
    private float fov; // Field of View (�ngulo de visi�n)
    private float viewDistance;
    private int rayCount = 50; // Calidad del cono, m�s rayos = m�s suave

    void Awake()
    {
        // Inicializa los componentes necesarios al empezar.
        visionMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = visionMesh;
    }

    /// <summary>
    /// Funci�n p�blica para que otro script (como el de la torreta) le pase
    /// los par�metros de visi�n y ordene dibujar el cono.
    /// </summary>
    public void SetVisionParameters(float fieldOfView, float distance)
    {
        fov = fieldOfView;
        viewDistance = distance;
        DrawVisionCone();
    }

    /// <summary>
    /// Dibuja la malla del cono de visi�n calculando sus v�rtices y tri�ngulos.
    /// </summary>
    private void DrawVisionCone()
    {
        // Un v�rtice en el origen, m�s un v�rtice por cada "rayo" del arco.
        Vector3[] vertices = new Vector3[rayCount + 2];
        // Cada secci�n del arco necesita un tri�ngulo, y cada tri�ngulo tiene 3 v�rtices.
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; // El primer v�rtice es el origen del cono (la posici�n del objeto).

        float currentAngle = -fov / 2; // Empezamos a dibujar desde el �ngulo izquierdo.
        float angleIncrement = fov / rayCount; // El �ngulo que hay entre cada "rayo".

        // Bucle para crear los v�rtices del arco exterior del cono.
        for (int i = 0; i <= rayCount; i++)
        {
            // Calcula la posici�n de cada v�rtice usando trigonometr�a (a trav�s de Quaternions).
            vertices[i + 1] = Quaternion.Euler(0, 0, currentAngle) * Vector3.up * viewDistance;
            currentAngle += angleIncrement;
        }

        // Bucle para definir los tri�ngulos que unen el origen con los v�rtices del arco.
        for (int i = 0, j = 0; i < rayCount; i++, j += 3)
        {
            triangles[j] = 0;           // V�rtice 1: el origen.
            triangles[j + 1] = i + 1; // V�rtice 2: un punto del arco.
            triangles[j + 2] = i + 2; // V�rtice 3: el siguiente punto del arco.
        }

        // Asigna la informaci�n calculada a la malla para que Unity la dibuje.
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals(); // Calcula las normales para una correcta iluminaci�n.
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. �C�mo se puede generar una malla (Mesh) en Unity mediante c�digo?
     * - Se consult� el proceso de definir arrays de `v�rtices` y `tri�ngulos` para construir
     * una forma geom�trica y asignarla a un `MeshFilter`.
     * 2. �Cu�l es la matem�tica para crear un arco o sector circular en una malla?
     * - Se investig� el uso de `Quaternion.Euler` en un bucle para calcular las posiciones
     * de los v�rtices a lo largo de un arco.
     * ================================================================
     */
}
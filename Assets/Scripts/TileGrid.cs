/*
 * =====================================================================================
 *
 * Filename:  TileGrid.cs
 *
 * Description:  Crea y gestiona una cuadrícula de nodos para pathfinding.
 * Ahora incluye la implementación del algoritmo Breadth-First Search (BFS).
 *
 * Authors:  Carlos Hernan
             Eduardo Calderon
             Cesar Sasia
 *
 * =====================================================================================
 */

using UnityEngine;
using System.Collections.Generic; 

public class TileGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2 gridWorldSize;
    public float nodeRadius;
    private Node[,] grid; // La cuadrícula 2D de nodos

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    [Header("Pathfinding Objects")]
    public Transform start; // Objeto que marca el inicio
    public Transform target; // Objeto que marca el final
    public LineRenderer pathRenderer; // Referencia al Line Renderer para dibujar en el juego

    private List<Node> finalPath; // Lista para guardar el camino encontrado

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void Update()
    {
        // Ejecuta la búsqueda en cada frame para ver los cambios en tiempo real.
        FindPath(start.position, target.position);
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                // Usa una Layer "Obstacle" para los muros
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, LayerMask.GetMask("Obstacle"));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    // ================================================================
    // a) Implementación de Breadth-First Search (BFS)
    // ================================================================
    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);

        Queue<Node> frontier = new Queue<Node>();
        HashSet<Node> visitedNodes = new HashSet<Node>();

        frontier.Enqueue(startNode);
        visitedNodes.Add(startNode);
        startNode.parent = null;

        bool pathFound = false;

        while (frontier.Count > 0)
        {
            Node currentNode = frontier.Dequeue();

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = currentNode.gridX + x;
                    int checkY = currentNode.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        Node neighbour = grid[checkX, checkY];
                        if (neighbour.walkable && !visitedNodes.Contains(neighbour))
                        {
                            neighbour.parent = currentNode;
                            visitedNodes.Add(neighbour);
                            frontier.Enqueue(neighbour);
                        }
                    }
                }
            }
        }

        if (pathFound)
        {
            RetracePath(startNode, targetNode);
        }
        else
        {
            finalPath = null;
        }

        // d) Llama a la función para dibujar el camino en la pantalla de Play.
        DrawPathInGame();
    }

    // ================================================================
    // c) Imprimir el camino en el orden correcto
    // ================================================================
    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);

        // Invierte la lista para que el camino vaya del inicio al final.
        path.Reverse();
        finalPath = path;

        // Imprime el camino en la consola para verificación.
        string pathString = "Camino encontrado: ";
        foreach (Node node in finalPath)
        {
            pathString += $"({node.gridX}, {node.gridY}) -> ";
        }
        Debug.Log(pathString + "Llegó!");
    }

    // ================================================================
    // d) Visualización del camino en la pantalla de Play
    // ================================================================
    void DrawPathInGame()
    {
        // Si no hay un Line Renderer asignado en el Inspector, no hace nada.
        if (pathRenderer == null) return;

        // Si se encontró un camino y tiene al menos un nodo...
        if (finalPath != null && finalPath.Count > 0)
        {
            // Le decimos al Line Renderer cuántos puntos tendrá la línea.
            pathRenderer.positionCount = finalPath.Count;

            // Recorremos cada nodo del camino y asignamos su posición a la línea.
            for (int i = 0; i < finalPath.Count; i++)
            {
                // Añadimos un pequeño offset vertical (Y) para que la línea se dibuje
                // ligeramente por encima del suelo y no parpadee (Z-fighting).
                pathRenderer.SetPosition(i, finalPath[i].worldPosition + Vector3.up * 0.1f);
            }
        }
        else
        {
            // Si no hay camino, "apagamos" la línea poniéndole 0 puntos.
            pathRenderer.positionCount = 0;
        }
    }

    // ================================================================
    // d) Visualización con Gizmos en el Editor
    // ================================================================
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }

        if (finalPath != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < finalPath.Count - 1; i++)
            {
                Gizmos.DrawLine(finalPath[i].worldPosition, finalPath[i + 1].worldPosition);
                Gizmos.DrawCube(finalPath[i].worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
            Gizmos.DrawCube(finalPath[finalPath.Count - 1].worldPosition, Vector3.one * (nodeDiameter - .1f));
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo se implementa BFS en C# para encontrar un camino?
     * - Se consultó el uso de `Queue<T>` para manejar la frontera de nodos y `HashSet<T>` para
     * registrar los nodos ya visitados de forma eficiente.
     * 2. ¿Cómo reconstruyo un camino desde un nodo final hasta el inicio?
     * - Se investigó el método de seguir los punteros `parent` de cada nodo hacia atrás
     * y luego usar `List.Reverse()` para obtener el camino en el orden correcto.
     * 3. ¿Cómo puedo dibujar una línea en la vista de juego (Game View) de Unity?
     * - Se investigó el uso del componente `Line Renderer` y cómo actualizar sus puntos
     * (`positionCount` y `SetPosition`) mediante un script.
     * ================================================================
     */
}
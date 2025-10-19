/*
 * =====================================================================================
 *
 * Filename:  TileGrid.cs
 *
 * Description:  Crea y gestiona una cuadrícula de nodos, implementa el algoritmo BFS
 * y visualiza el camino encontrado.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
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
    private Node[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    [Header("Pathfinding Objects")]
    public Transform start;
    public Transform target;
    public LineRenderer pathRenderer; 

    private List<Node> finalPath;

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

    // a) Implementación de Breadth-First Search (BFS)
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

            // Revisa los 8 vecinos del nodo actual
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

        DrawPathInGame();
    }

    // c) Imprimir el camino en el orden correcto
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
        path.Reverse();
        finalPath = path;

        string pathString = "Camino encontrado: ";
        foreach (Node node in finalPath)
        {
            pathString += $"({node.gridX}, {node.gridY}) -> ";
        }
        Debug.Log(pathString + "Llegó!");
    }

    
    void DrawPathInGame()
    {
        if (pathRenderer == null) return;
        if (finalPath != null && finalPath.Count > 0)
        {
            pathRenderer.positionCount = finalPath.Count;
            for (int i = 0; i < finalPath.Count; i++)
            {
                pathRenderer.SetPosition(i, finalPath[i].worldPosition + Vector3.up * 0.1f);
            }
        }
        else
        {
            pathRenderer.positionCount = 0;
        }
    }

    // d) Visualización con Gizmos
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
            }
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cuál es la estructura de datos ideal para implementar BFS?
     * - Se consultó el uso de `Queue<T>` para manejar la frontera de nodos.
     * 2. ¿Cómo reconstruir el camino del BFS en el orden correcto?
     * - Se investigó el método de seguir los punteros `parent` hacia atrás y luego
     * usar `List.Reverse()` para obtener la ruta de inicio a fin.
     * 3. ¿Cómo visualizar una ruta en la pantalla de juego de Unity?
     * - Se investigó el uso del componente `Line Renderer` y cómo actualizar sus puntos.
     * ================================================================
     */
}
/*
 * =====================================================================================
 *
 * Filename:  Pathfinding.cs
 *
 * Description:  Gestiona una cuadrícula de nodos e implementa el algoritmo de
 * búsqueda de caminos Breadth-First Search (BFS).
 *
 * Authors:  Carlos Hernan Gonzalez Gonzalez
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * =====================================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Las clases y enums proporcionados por el profesor se mantienen intactos.
public enum EDirections : int
{
    Up = -1, Right = 1, Left = -1, Down = 1
}

public enum ETileType
{
    Normal, Fire, Forest, Sand, COUNT,
}

public class Node
{
    public Node Parent;
    public int X { get; }
    public int Y { get; }
    public bool Walkable;
    public float TerrainCost, GCost, HCost, TotalCost;
    public ETileType TileType;

    public Node(int x, int y, ETileType tileType, float terrainCost, bool isWalkable = true)
    {
        X = x; Y = y; TileType = tileType; TerrainCost = terrainCost; Walkable = isWalkable;
        Parent = null; HCost = float.PositiveInfinity;
    }
}

public class Pathfinding : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int height = 20;
    [SerializeField] private int width = 20;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float walkableProbability = 0.85f;

    [Header("Pathfinding Endpoints")]
    [SerializeField] private int originX = 1;
    [SerializeField] private int originY = 1;
    [SerializeField] private int goalX = 18;
    [SerializeField] private int goalY = 18;

    [Header("Visualization (Puntos Extra)")]
    [Tooltip("Arrastra aquí el objeto con el componente Line Renderer")]
    public LineRenderer pathLineRenderer;

    private Node[][] _grid;
    private List<Node> _pathToGoal = new List<Node>();

    // Al iniciar el juego, se ejecuta la búsqueda una vez.
    void Start()
    {
        FindPath();
    }

    // En cada frame, se vuelve a ejecutar para que sea dinámico si movemos los marcadores.
    void Update()
    {
        // Para que funcione dinámicamente, movemos la lógica de Start a Update.
        // Pero primero, nos aseguramos de que el grid exista.
        if (_grid == null)
        {
            InitializeGrid();
        }
        FindPath();
    }

    void InitializeGrid()
    {
        _grid = new Node[height][];
        for (int i = 0; i < height; i++)
        {
            _grid[i] = new Node[width];
            for (int j = 0; j < width; j++)
            {
                bool isWalkable = Random.value < walkableProbability;
                _grid[i][j] = new Node(j, i, ETileType.Normal, 1.0f, isWalkable);
            }
        }
    }

    void SetupGridForNewSearch()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                _grid[i][j].Parent = null;
            }
        }
        _grid[originY][originX].Walkable = true;
        _grid[goalY][goalX].Walkable = true;
        _grid[originY][originX].Parent = _grid[originY][originX];
    }

    public bool FindPath()
    {
        InitializeGrid();
        SetupGridForNewSearch();

        if (BreadthFirstSearch(_grid[originY][originX], _grid[goalY][goalX]))
        {
            // c) Reconstruir y revertir el camino para el orden correcto.
            _pathToGoal.Clear();
            Node current = _grid[goalY][goalX];
            while (current.Parent != current && current.Parent != null)
            {
                _pathToGoal.Add(current);
                current = current.Parent;
            }
            _pathToGoal.Add(current);
            _pathToGoal.Reverse();

            // Imprime el camino en la consola.
            PrintPath();
            DrawPathInGame();
            return true;
        }
        else
        {
            _pathToGoal.Clear();
            DrawPathInGame();
            return false;
        }
    }

    // a) Implementación del algoritmo Breadth-First Search
    private bool BreadthFirstSearch(Node origin, Node goal)
    {
        Queue<Node> openList = new Queue<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Enqueue(origin);
        closedList.Add(origin);

        while (openList.Count > 0)
        {
            Node current = openList.Dequeue();

            if (current == goal) return true;

            // Explorar los 4 vecinos (arriba, derecha, abajo, izquierda)
            for (int i = 0; i < 4; i++)
            {
                int xOffset = (i == 1) ? 1 : (i == 3) ? -1 : 0;
                int yOffset = (i == 0) ? -1 : (i == 2) ? 1 : 0;

                Node neighbor = CheckValidNode(current, xOffset, yOffset);
                if (neighbor != null && !closedList.Contains(neighbor))
                {
                    neighbor.Parent = current;
                    closedList.Add(neighbor);
                    openList.Enqueue(neighbor);
                }
            }
        }
        return false;
    }

    private Node CheckValidNode(Node current, int xOffset, int yOffset)
    {
        int newX = current.X + xOffset;
        int newY = current.Y + yOffset;

        if (newY >= height || newY < 0 || newX >= width || newX < 0) return null;

        Node neighborNode = _grid[newY][newX];
        if (!neighborNode.Walkable) return null;

        return neighborNode;
    }

    private void PrintPath()
    {
        string pathString = "Camino en orden: ";
        foreach (Node node in _pathToGoal)
        {
            pathString += $"({node.X}, {node.Y}) -> ";
        }
        Debug.Log(pathString + "Llegó!");
    }

    // d) Visualización en la pantalla de Play (Puntos Extra)
    void DrawPathInGame()
    {
        if (pathLineRenderer == null) return;
        if (_pathToGoal != null && _pathToGoal.Count > 0)
        {
            pathLineRenderer.positionCount = _pathToGoal.Count;
            for (int i = 0; i < _pathToGoal.Count; i++)
            {
                pathLineRenderer.SetPosition(i, new Vector3(_pathToGoal[i].X, 0.1f, -_pathToGoal[i].Y));
            }
        }
        else
        {
            pathLineRenderer.positionCount = 0;
        }
    }

    // d) Visualización con Gizmos
    private void OnDrawGizmos()
    {
        if (_grid == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Gizmos.color = _grid[y][x].Walkable ? new Color(1, 1, 1, 0.5f) : new Color(1, 0, 0, 0.5f);
                Gizmos.DrawCube(new Vector3(x, 0, -y), Vector3.one * 0.8f);
            }
        }

        if (_pathToGoal != null && _pathToGoal.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _pathToGoal.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(_pathToGoal[i].X, 0, -_pathToGoal[i].Y), new Vector3(_pathToGoal[i + 1].X, 0, -_pathToGoal[i + 1].Y));
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(originX, 0.1f, -originY), Vector3.one);

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(new Vector3(goalX, 0.1f, -goalY), Vector3.one);
    }
}
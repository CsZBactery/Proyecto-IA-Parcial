/*
 * =====================================================================================
 *
 * Filename:  Node.cs
 *
 * Description:  Representa un único nodo o "baldosa" en la cuadrícula de pathfinding.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * =====================================================================================
 */

using UnityEngine;

public class Node
{
    public bool walkable;       // ¿Se puede caminar sobre este nodo?
    public Vector3 worldPosition; // La posición del nodo en el mundo 3D.
    public int gridX;           // La coordenada X del nodo en la cuadrícula.
    public int gridY;           // La coordenada Y del nodo en la cuadrícula.

    // El nodo desde el cual llegamos a este. Esencial para reconstruir el camino.
    public Node parent;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}
/*
 * =====================================================================================
 *
 * Filename:  SteeringAgent.cs
 *
 * Description:  Maneja múltiples Steering Behaviors para un agente 3D, incluyendo
 * Seek, Flee, Pursuit y Obstacle Avoidance.
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

[RequireComponent(typeof(Rigidbody))]
public class SteeringAgent : MonoBehaviour
{
    // Un 'enum' nos permite crear un menú desplegable en el Inspector para elegir el comportamiento.
    public enum Behavior { Seek, Flee, Pursuit }

    [Header("Configuración de Comportamiento")]
    public Behavior currentBehavior = Behavior.Pursuit;

    [Header("Configuración de Movimiento")]
    public float maxSpeed = 10f;
    public float maxForce = 15f;
    public float friction = 0.95f;

    [Header("Comportamiento 'Pursuit'")]
    public float predictionTime = 1f;

    [Header("Esquivar Obstáculos")]
    public bool canAvoidObstacles = true;
    public float obstacleDetectionDistance = 3f;
    public float obstacleAvoidanceForce = 200f;
    public LayerMask obstacleLayer;

    public Transform target;
    private Rigidbody targetRb;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Obtenemos el Rigidbody del objetivo para poder leer su velocidad (necesario para Pursuit).
            if (targetRb == null)
            {
                targetRb = target.GetComponent<Rigidbody>();
            }

            Vector3 primaryForce = Vector3.zero;

            // Un 'switch' elige qué comportamiento ejecutar basado en la selección del Inspector.
            switch (currentBehavior)
            {
                case Behavior.Seek:
                    primaryForce = Seek(target.position);
                    break;
                case Behavior.Flee:
                    primaryForce = Flee(target.position);
                    break;
                case Behavior.Pursuit:
                    primaryForce = (targetRb != null) ? Pursuit(target.position, targetRb.velocity) : Seek(target.position);
                    break;
            }

            // Si está activado, calcula una fuerza adicional para esquivar paredes.
            Vector3 avoidanceForce = canAvoidObstacles ? ObstacleAvoidance() : Vector3.zero;

            // Combina la fuerza del comportamiento principal con la de esquivar y la aplica.
            rb.AddForce(primaryForce + avoidanceForce);

            LimitSpeed();
            ApplyRotation();
        }
        else
        {
            // Si no hay objetivo, el agente se frena gradualmente.
            rb.velocity *= friction;
        }
    }

    // --- MÉTODOS DE STEERING BEHAVIOR ---

    // Calcula la fuerza para ir directamente a un punto.
    private Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        Vector3 steeringForce = desiredVelocity - rb.velocity;
        return Vector3.ClampMagnitude(steeringForce, maxForce);
    }

    // Calcula la fuerza para huir de un punto.
    private Vector3 Flee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        Vector3 steeringForce = desiredVelocity - rb.velocity;
        return Vector3.ClampMagnitude(steeringForce, maxForce);
    }

    // Calcula la fuerza para interceptar un objetivo en movimiento.
    private Vector3 Pursuit(Vector3 targetPosition, Vector3 targetVelocity)
    {
        // Predice la posición futura sumando la velocidad actual del objetivo multiplicada por un tiempo.
        Vector3 futurePosition = targetPosition + (targetVelocity * predictionTime);
        // Una vez predicha la posición, simplemente hace un "Seek" hacia ese punto futuro.
        return Seek(futurePosition);
    }

    // Calcula una fuerza para esquivar obstáculos.
    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        // Lanza un rayo hacia adelante.
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectionDistance, obstacleLayer))
        {
            // Si detecta un obstáculo, calcula una fuerza perpendicular para desviarse.
            Vector3 avoidanceDirection = hit.point - transform.position;
            Vector3 force = Vector3.Cross(Vector3.up, avoidanceDirection).normalized * obstacleAvoidanceForce;
            return force;
        }
        return Vector3.zero;
    }

    // --- MÉTODOS AUXILIARES ---

    private void LimitSpeed()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void ApplyRotation()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(rb.velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo puedo implementar múltiples Steering Behaviors en un solo script de forma seleccionable?
     * - Se investigó el uso de 'Enums' y 'Switch case' para crear un menú desplegable en el Inspector
     * y cambiar entre lógicas de movimiento como Seek, Flee y Pursuit.
     * 2. ¿Cuál es la lógica para un Steering Behavior de 'Obstacle Avoidance' en 3D?
     * - Se consultó sobre el método de usar `Physics.Raycast` para detectar un obstáculo al frente y `Vector3.Cross`
     * para calcular una fuerza de evasión perpendicular.
     * ================================================================
     */
}
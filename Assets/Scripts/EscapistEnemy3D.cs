/*
 * =====================================================================================
 *
 * Filename:  EscapistEnemy3D.cs
 *
 * Description:  Implementa un enemigo que huye del jugador, esquiva obstáculos y dispara
 * de forma predictiva, todo en un ciclo de actividad y descanso.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingeniería del Conocimiento
 *
 * =====================================================================================
 */

using System.Collections;
using UnityEngine;

// Heredamos de BaseEnemy para tener vida y poder morir.
public class EscapistEnemy3D : BaseEnemy
{
    // A) Ciclo de Huida y Descanso
    [Header("Flee & Rest Cycle")]
    [Tooltip("Segundos que el enemigo pasará huyendo.")]
    public float fleeDuration = 5f;
    [Tooltip("Segundos que el enemigo se detendrá a descansar.")]
    public float restDuration = 3f;

    // C) Movimiento "Ligero"
    [Header("Movement Settings")]
    [Tooltip("Aceleración alta para un movimiento ágil.")]
    public float accelerationForce = 50f;
    [Tooltip("Velocidad máxima baja para que no se aleje demasiado.")]
    public float maxSpeed = 4f;

    // B) Disparo Predictivo
    [Header("Shooting Behavior")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f; // Disparos por segundo
    [Tooltip("Qué tan en el futuro intentará predecir la posición del jugador para disparar.")]
    public float shotPredictionTime = 0.5f;
    public float bulletSpeed = 20f; // Velocidad de la bala

    // D) Esquivar Obstáculos
    [Header("Obstacle Avoidance")]
    [Tooltip("Distancia a la que detectará obstáculos para esquivarlos.")]
    public float obstacleDetectionDistance = 2f;
    [Tooltip("Fuerza con la que evitará los obstáculos.")]
    public float obstacleAvoidanceForce = 100f;
    public LayerMask obstacleLayer; // La capa donde se encuentran los obstáculos (ej. "Obstacle")

    private Rigidbody rb;
    private Transform playerTransform;
    private Rigidbody playerRb;

    private enum State { Fleeing, Resting }
    private State currentState;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();

        GameObject player = GameObject.FindGameObjectWithTag("PLAYER");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        StartCoroutine(FleeAndRestCycle());
        StartCoroutine(ShootingCycle());
    }

    private void FixedUpdate()
    {
        // Solo aplica las fuerzas de movimiento si está en el estado "Fleeing".
        if (currentState == State.Fleeing)
        {
            ApplyMovementForces();
        }
    }

    // A) Corutina que controla el ciclo de huir y descansar.
    private IEnumerator FleeAndRestCycle()
    {
        while (true) // Este ciclo se repetirá infinitamente.
        {
            // Fase de Huida
            currentState = State.Fleeing;
            yield return new WaitForSeconds(fleeDuration);

            // Fase de Descanso
            currentState = State.Resting;
            rb.linearVelocity = Vector3.zero; // Detiene al enemigo por completo.
            yield return new WaitForSeconds(restDuration);
        }
    }

    // B) Corutina que controla el disparo, es independiente del ciclo de movimiento.
    private IEnumerator ShootingCycle()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (playerTransform != null)
            {
                ShootPredictive();
            }
            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    private void ApplyMovementForces()
    {
        if (playerTransform == null) return;

        // --- Fuerza Principal: Huir (Flee) ---
        // Se calcula en el plano XZ ignorando la altura.
        Vector3 fleeDirection = transform.position - playerTransform.position;
        fleeDirection.y = 0; // CAMBIO: Ignorar la diferencia de altura para el movimiento.
        fleeDirection.Normalize();

        // --- Fuerza Secundaria: Esquivar Obstáculos (Obstacle Avoidance) ---
        Vector3 avoidanceDirection = Vector3.zero;
        RaycastHit hit;

        // CAMBIO: El rayo debe lanzarse en la dirección del movimiento, no del "forward" del modelo.
        // Y solo si el enemigo se está moviendo.
        if (rb.linearVelocity.magnitude > 0.1f && Physics.Raycast(transform.position, rb.linearVelocity.normalized, out hit, obstacleDetectionDistance, obstacleLayer))
        {
            // Calcula una dirección perpendicular al obstáculo en el plano XZ.
            avoidanceDirection = Vector3.Cross(hit.normal, Vector3.up);

            // CAMBIO: Asegurarse de que la dirección de evasión no apunte hacia el jugador.
            // Esto evita que el enemigo esquive una pared corriendo hacia el jugador.
            float dot = Vector3.Dot(avoidanceDirection, -fleeDirection);
            if (dot < 0)
            {
                avoidanceDirection *= -1;
            }
        }

        // --- Combinamos las fuerzas y aplicamos el movimiento ---
        Vector3 finalForce = (fleeDirection * accelerationForce) + (avoidanceDirection * obstacleAvoidanceForce);
        rb.AddForce(finalForce); // CAMBIO: No multiplicar por Time.fixedDeltaTime aquí, AddForce ya lo considera.

        // --- Limita la velocidad máxima ---
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // --- Rota para mirar en la dirección del movimiento ---
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private void ShootPredictive()
    {
        if (playerRb == null) return; // No se puede predecir sin el Rigidbody del jugador.

        // Predice la posición futura del jugador en 3D.
        Vector3 futurePosition = playerTransform.position + (playerRb.linearVelocity * shotPredictionTime);
        Vector3 aimDirection = (futurePosition - firePoint.position).normalized;

        // Crea la bala y dispara en 3D.
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.AddForce(aimDirection * bulletSpeed, ForceMode.Impulse);
    }
}
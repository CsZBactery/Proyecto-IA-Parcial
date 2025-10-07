/*
 * =====================================================================================
 *
 * Filename:  TurretEnemy.cs (Versión 3D)
 *
 * Description:  Implementa un enemigo tipo torreta en 3D. Hereda la lógica base de
 * BaseEnemy y añade un cono de visión para detección, un comportamiento de
 * patrulla/rotación y un sistema de disparo.
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

// Hereda de BaseEnemy para tener vida, recibir daño y morir.
public class TurretEnemy : BaseEnemy
{
    [Header("Vision Cone Settings")]
    [Range(0, 180)]
    public float visionAngle = 90f;
    public float visionDistance = 15f;
    public LayerMask visionObstacles; // Capas que bloquean la visión (ej. "Obstacle").
    public VisionConeMesh visionConeMesh; // Referencia al script que dibuja el cono.

    [Header("Patrol Behavior")]
    public float patrolRotationAngle = 120f;
    public float patrolRotationSpeed = 30f;
    public float patrolPauseDuration = 2f;

    [Header("Attack Behavior")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletForce = 20f;
    public float attackCooldown = 3f;

    private Transform playerTransform;
    private Coroutine patrolCoroutine;
    private Coroutine attackCoroutine;
    private bool isPlayerDetected = false;
    private float timeSincePlayerSeen = 0f;

    protected override void Awake()
    {
        base.Awake(); // Ejecuta la lógica de vida y efectos de BaseEnemy.
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Si hay un script para el cono visual, le pasamos los parámetros.
        if (visionConeMesh != null)
        {
            visionConeMesh.SetVisionParameters(visionAngle, visionDistance);
        }
    }

    private void Start()
    {
        // Inicia el comportamiento de patrulla al empezar.
        patrolCoroutine = StartCoroutine(PatrolRotation());
    }

    private void Update()
    {
        bool playerCurrentlyVisible = CheckForPlayer();

        // Lógica de transición entre estados: Patrullar y Atacar.
        if (playerCurrentlyVisible && !isPlayerDetected)
        {
            isPlayerDetected = true;
            if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
            attackCoroutine = StartCoroutine(AttackPlayer());
        }
        else if (!playerCurrentlyVisible && isPlayerDetected)
        {
            timeSincePlayerSeen += Time.deltaTime;
            if (timeSincePlayerSeen >= attackCooldown)
            {
                isPlayerDetected = false;
                timeSincePlayerSeen = 0f;
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                patrolCoroutine = StartCoroutine(PatrolRotation());
            }
        }

        if (playerCurrentlyVisible)
        {
            timeSincePlayerSeen = 0f;
        }
    }

    // --- LÓGICA DE DETECCIÓN EN 3D ---
    private bool CheckForPlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. ¿Está el jugador demasiado lejos? Si es así, no es visible.
        if (distanceToPlayer > visionDistance)
        {
            return false;
        }

        // 2. ¿Está el jugador fuera del ángulo de visión? Si es así, no es visible.
        if (Vector3.Angle(transform.forward, directionToPlayer) > visionAngle / 2)
        {
            return false;
        }

        // 3. ¿Hay una línea de visión directa? (Raycast)
        RaycastHit hit;
        // Lanza un rayo desde la torreta hacia el jugador. Si en el camino choca con CUALQUIER COSA
        // que esté en la capa de obstáculos...
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer, visionObstacles))
        {
            // ...significa que la vista está bloqueada.
            return false;
        }

        // Si pasó todas las pruebas anteriores, ¡el jugador es visible!
        return true;
    }

    // --- CORUTINA DE PATRULLA EN 3D (GIRA EN EL EJE Y) ---
    private IEnumerator PatrolRotation()
    {
        Quaternion initialRotation = transform.rotation;
        // La rotación ahora es alrededor del eje Y (vertical).
        Quaternion targetRotationA = initialRotation * Quaternion.Euler(0, patrolRotationAngle / 2, 0);
        Quaternion targetRotationB = initialRotation * Quaternion.Euler(0, -patrolRotationAngle / 2, 0);

        while (true)
        {
            yield return RotateTo(targetRotationA, patrolRotationSpeed);
            yield return new WaitForSeconds(patrolPauseDuration);
            yield return RotateTo(targetRotationB, patrolRotationSpeed);
            yield return new WaitForSeconds(patrolPauseDuration);
        }
    }

    // --- CORUTINA DE ATAQUE EN 3D ---
    private IEnumerator AttackPlayer()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                Vector3 aimDirection = (playerTransform.position - firePoint.position).normalized;

                // En 3D, usamos Quaternions para la rotación. LookRotation crea una rotación que "mira" a un punto.
                Quaternion lookRotation = Quaternion.LookRotation(aimDirection);

                // Crea la bala con la rotación correcta.
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, lookRotation);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                // Aplica la fuerza en 3D.
                bulletRb.AddForce(aimDirection * bulletForce, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    // Esta función auxiliar no necesita cambios, ya que los Quaternions funcionan igual en 3D.
    private IEnumerator RotateTo(Quaternion targetRotation, float speed)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    // OnDrawGizmos tampoco necesita cambios, Unity lo dibuja en 3D automáticamente.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isPlayerDetected ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Vector3 fovLine1 = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionDistance;
        Vector3 fovLine2 = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionDistance;

        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
    }
}
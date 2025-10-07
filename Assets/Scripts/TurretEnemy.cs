using System.Collections;
using UnityEngine;

public class TurretEnemy : BaseEnemy
{
    [Header("Vision Cone Settings")]
    [Range(0, 360)]
    public float visionAngle = 90f;
    public float visionDistance = 10f;
    public LayerMask visionObstacles;
    public VisionConeMesh visionConeMesh;

    [Header("Patrol Behavior")]
    public float patrolRotationAngle = 120f;
    public float patrolRotationSpeed = 30f;
    public float patrolPauseDuration = 2f;

    [Header("Attack Behavior")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float attackCooldown = 3f;

    private Transform playerTransform;
    private Coroutine patrolCoroutine;
    private Coroutine attackCoroutine;
    private bool isPlayerDetected = false;
    private float timeSincePlayerSeen = 0f;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (visionConeMesh != null)
            visionConeMesh.SetVisionParameters(visionAngle, visionDistance);
    }

    private void Start()
    {
        patrolCoroutine = StartCoroutine(PatrolRotation());
    }

    private void Update()
    {
        bool playerCurrentlyVisible = CheckForPlayer();

        if (playerCurrentlyVisible && !isPlayerDetected)
        {
            isPlayerDetected = true;
            StopCoroutine(patrolCoroutine);
            attackCoroutine = StartCoroutine(AttackPlayer());
        }
        else if (!playerCurrentlyVisible && isPlayerDetected)
        {
            timeSincePlayerSeen += Time.deltaTime;
            if (timeSincePlayerSeen >= attackCooldown)
            {
                isPlayerDetected = false;
                timeSincePlayerSeen = 0f;
                StopCoroutine(attackCoroutine);
                patrolCoroutine = StartCoroutine(PatrolRotation());
            }
        }

        if (playerCurrentlyVisible)
            timeSincePlayerSeen = 0f;
    }

    // --- DETECCIÓN DEL JUGADOR (AHORA EN 3D, HORIZONTAL) ---
    private bool CheckForPlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = (playerTransform.position - transform.position);
        directionToPlayer.y = 0; // Ignora diferencias verticales
        directionToPlayer.Normalize();

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. Dentro de distancia
        if (distanceToPlayer <= visionDistance)
        {
            // 2. Dentro del ángulo
            if (Vector3.Angle(transform.forward, directionToPlayer) < visionAngle / 2)
            {
                // 3. ¿Algún obstáculo entre ambos?
                if (!Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer, distanceToPlayer, visionObstacles))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // --- ROTACIÓN DE PATRULLA (AHORA EN PLANO HORIZONTAL) ---
    private IEnumerator PatrolRotation()
    {
        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotationA = Quaternion.Euler(0, patrolRotationAngle / 2, 0) * initialRotation;
        Quaternion targetRotationB = Quaternion.Euler(0, -patrolRotationAngle / 2, 0) * initialRotation;

        while (true)
        {
            yield return RotateTo(targetRotationA, patrolRotationSpeed);
            yield return new WaitForSeconds(patrolPauseDuration);
            yield return RotateTo(targetRotationB, patrolRotationSpeed);
            yield return new WaitForSeconds(patrolPauseDuration);
        }
    }

    // --- ATAQUE HACIA EL JUGADOR ---
    private IEnumerator AttackPlayer()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                Vector3 aimDirection = (playerTransform.position - firePoint.position);
                aimDirection.y = 0; // Mantiene la dirección horizontal
                aimDirection.Normalize();

                Quaternion lookRotation = Quaternion.LookRotation(aimDirection);
                firePoint.rotation = lookRotation;

                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, lookRotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                rb.AddForce(aimDirection * 15f, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    private IEnumerator RotateTo(Quaternion targetRotation, float speed)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null;
        }
    }

    // --- DIBUJO DEL CONO DE VISIÓN EN HORIZONTAL ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionDistance);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionDistance);

        // Arco del cono (solo visual)
        int segments = 20;
        Vector3 prev = transform.position + leftBoundary * visionDistance;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -visionAngle / 2 + (visionAngle / segments) * i;
            Vector3 nextDir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 next = transform.position + nextDir * visionDistance;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}

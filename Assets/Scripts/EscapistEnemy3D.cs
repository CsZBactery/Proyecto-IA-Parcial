using System.Collections;
using UnityEngine;

// Heredamos de BaseEnemy para tener vida y poder morir.
public class EscapistEnemy3D : BaseEnemy
{
    [Header("Flee & Rest Cycle")]
    [Tooltip("Segundos que el enemigo pasar� huyendo.")]
    public float fleeDuration = 5f;
    [Tooltip("Segundos que el enemigo se detendr� a descansar.")]
    public float restDuration = 3f;

    [Header("Movement Settings")]
    [Tooltip("Aceleraci�n alta para un movimiento �gil.")]
    public float accelerationForce = 50f;
    [Tooltip("Velocidad m�xima baja para que no se aleje demasiado.")]
    public float maxSpeed = 4f;

    [Header("Shooting Behavior")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1.5f; // Disparos por segundo
    [Tooltip("Qu� tan en el futuro intentar� predecir la posici�n del jugador para disparar.")]
    public float shotPredictionTime = 0.5f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Distancia a la que detectar� obst�culos para esquivarlos.")]
    public float obstacleDetectionDistance = 2f;
    [Tooltip("Fuerza con la que evitar� los obst�culos.")]
    public float obstacleAvoidanceForce = 100f;
    public LayerMask obstacleLayer;

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
        if (currentState == State.Fleeing)
        {
            ApplyMovementForces();
        }
    }

    private IEnumerator FleeAndRestCycle()
    {
        while (true)
        {
            // Fase de huida
            currentState = State.Fleeing;
            yield return new WaitForSeconds(fleeDuration);

            // Fase de descanso
            currentState = State.Resting;
            rb.linearVelocity = Vector3.zero;
            yield return new WaitForSeconds(restDuration);
        }
    }

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

        // --- Fuerza principal: huir ---
        Vector3 fleeDirection = (transform.position - playerTransform.position).normalized;

        // --- Esquivar obst�culos ---
        Vector3 avoidanceDirection = Vector3.zero;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out hit, obstacleDetectionDistance, obstacleLayer))
        {
            // Calcula una direcci�n perpendicular al obst�culo
            avoidanceDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
        }

        // --- Aplica la fuerza combinada ---
        Vector3 finalForce = (fleeDirection * accelerationForce) + (avoidanceDirection * obstacleAvoidanceForce);
        rb.AddForce(finalForce * Time.fixedDeltaTime, ForceMode.Acceleration);

        // --- Limita la velocidad m�xima ---
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // --- Rota para mirar en la direcci�n del movimiento ---
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private void ShootPredictive()
    {
        // Predice la posici�n futura del jugador
        Vector3 futurePosition = playerTransform.position + (playerRb.linearVelocity * shotPredictionTime);
        Vector3 aimDirection = (futurePosition - firePoint.position).normalized;

        // Crea la bala y dispara
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.AddForce(aimDirection * 20f, ForceMode.Impulse);
    }
}

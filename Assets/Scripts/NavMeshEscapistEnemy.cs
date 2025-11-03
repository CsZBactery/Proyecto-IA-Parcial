using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshEscapistEnemy : BaseEnemy // ← hereda de tu base del 1er parcial
{
    public enum EnemyState { Active, Tired }

    [Header("Refs")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform muzzle;              // punto de disparo
    [SerializeField] private GameObject projectilePrefab;   // proyectil con Rigidbody + Collider

    [Header("Movimiento (ligero)")]
    [SerializeField] private NavMeshAgent agent;
    [Tooltip("Velocidad máxima moderada (sensación ligera = acel. alta, speed moderado)")]
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float angularSpeed = 360f;
    [SerializeField] private float stoppingDistance = 0.15f;

    [Header("Línea de visión (LoS)")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private LayerMask losObstacles;  // capas que BLOQUEAN la vista (muros, etc.)
    [SerializeField] private float losCheckInterval = 0.1f;
    [SerializeField] private float lostSightGraceSeconds = 1.5f; // si no ve al player por X s, vuelve a perseguir

    [Header("Huida (Flee)")]
    [SerializeField] private float fleeDistance = 6f;        // qué tan lejos intenta huir
    [SerializeField] private float sampleMaxDistance = 2.0f; // radio para NavMesh.SamplePosition

    [Header("Temporizadores")]
    [SerializeField] private float tiredDuration = 3.0f;     // dura cansado
    [SerializeField] private float activeDuration = 5.0f;    // dura activo antes de cansarse

    [Header("Disparo")]
    [SerializeField] private float fireRateActive = 2.0f;    // balas/seg en Activo
    [SerializeField] private float fireRateTired = 1.0f;     // balas/seg en Cansado
    [SerializeField] private float projectileSpeed = 16f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private float tiredSpreadDegrees = 6f;  // peor puntería estando cansado

    [Header("Feedback visual")]
    [SerializeField] private Renderer[] renderersToTint;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color tiredColor = new Color(1f, 0.8f, 0.2f, 1f);

    // Estado
    private EnemyState state = EnemyState.Active;
    private Coroutine stateRoutine;
    private float lastSeenTime = -999f;

    // Debug/Gizmos
    private Vector3 lastFleeTarget = Vector3.zero;
    private bool hadValidFlee = false;
    private Vector3 lastRayOrigin, lastRayHitOrEnd;
    private bool lastRayBlocked = false;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (!agent) agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Config “ligera”: acel. alta, velocidad moderada
        agent.speed = maxSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        ChangeState(EnemyState.Active);
        StartCoroutine(LoSWatcher());
    }

    private void ChangeState(EnemyState newState)
    {
        if (stateRoutine != null) StopCoroutine(stateRoutine);
        state = newState;

        // feedback de color
        Tint(state == EnemyState.Tired ? tiredColor : activeColor);

        stateRoutine = (state == EnemyState.Active)
            ? StartCoroutine(StateActive())
            : StartCoroutine(StateTired());
    }

    private void Tint(Color c)
    {
        foreach (var r in renderersToTint)
            if (r) r.material.color = c;
    }

    // Revisa línea de visión periódicamente
    private IEnumerator LoSWatcher()
    {
        var wait = new WaitForSeconds(losCheckInterval);
        while (true)
        {
            if (player)
            {
                bool hasLoS = HasLineOfSight(out Vector3 rayEnd, out bool blocked);
                lastRayOrigin = transform.position + Vector3.up * 0.25f;
                lastRayHitOrEnd = rayEnd;
                lastRayBlocked = blocked;
                if (hasLoS) lastSeenTime = Time.time;
            }
            yield return wait;
        }
    }

    private bool HasLineOfSight(out Vector3 rayEnd, out bool blocked)
    {
        blocked = false;
        rayEnd = transform.position;
        if (!player) return false;

        Vector3 origin = transform.position + Vector3.up * 0.25f;
        Vector3 toPlayer = player.position - origin;
        float dist = toPlayer.magnitude;
        if (dist <= 0.001f) { rayEnd = origin; return true; }

        Vector3 dir = toPlayer / dist;

        // Raycast 3D: si golpea un obstáculo antes del player → LoS bloqueado
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, losObstacles, QueryTriggerInteraction.Ignore))
        {
            blocked = true;
            rayEnd = hit.point;
            return false;
        }

        rayEnd = origin + dir * dist;
        return true;
    }

    private IEnumerator StateTired()
    {
        // A) Cansado: se detiene, sin destino, solo dispara con peor puntería
        agent.ResetPath();
        agent.isStopped = true;

        float fireInterval = (fireRateTired > 0f) ? 1f / fireRateTired : 999f;
        float t = 0f;
        while (t < tiredDuration)
        {
            if (player) ShootAtPlayer(useTiredSpread: true);
            yield return new WaitForSeconds(fireInterval);
            t += fireInterval;
        }

        // B) Al terminar el temporizador, pasa a Activo
        ChangeState(EnemyState.Active);
    }

    private IEnumerator StateActive()
    {
        float endTime = Time.time + activeDuration;
        float nextFireTime = 0f;
        agent.isStopped = false;

        // A) Al entrar: asigna destino = posición del jugador
        if (player) agent.SetDestination(player.position);

        while (Time.time < endTime)
        {
            if (!player) { yield return null; continue; }

            // A.1) Si hay LoS → deja de moverse; si no, sigue
            bool hasLoS = HasLineOfSight(out _, out _);
            if (hasLoS)
            {
                agent.isStopped = true;
            }
            else
            {
                // F) Si no ha visto al jugador en X s → vuelve a perseguir su posición actual
                if (Time.time - lastSeenTime > lostSightGraceSeconds)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                    lastSeenTime = Time.time;
                }
            }

            // Disparo en Activo
            if (fireRateActive > 0f && Time.time >= nextFireTime)
            {
                ShootAtPlayer(useTiredSpread: false);
                nextFireTime = Time.time + (1f / fireRateActive);
            }

            // B/C) Si jugador dentro del radio y NO está cansado → calcula punto de huida (C)
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= detectionRadius)
            {
                Vector3 fleeDir = (transform.position - player.position).normalized;
                Vector3 desired = transform.position + fleeDir * fleeDistance;

                if (TrySampleOnNavMesh(desired, out Vector3 valid))
                {
                    hadValidFlee = true;
                    lastFleeTarget = valid;
                    agent.isStopped = false;
                    agent.SetDestination(valid);
                }
                else
                {
                    // C.2) Fallback: hacia el jugador (opuesto de flee)
                    Vector3 towards = transform.position + (player.position - transform.position).normalized * fleeDistance;
                    hadValidFlee = TrySampleOnNavMesh(towards, out Vector3 valid2);
                    lastFleeTarget = hadValidFlee ? valid2 : towards;
                    if (hadValidFlee)
                    {
                        agent.isStopped = false;
                        agent.SetDestination(valid2);
                    }
                }
                // D) Tras asignar el destino flee, sigue corriendo el temporizador de activo (E al salir)
            }

            yield return null;
        }

        // E) Termina el temporizador → pasa a Cansado
        ChangeState(EnemyState.Tired);
    }

    private bool TrySampleOnNavMesh(Vector3 pos, out Vector3 sampled)
    {
        sampled = pos;
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, sampleMaxDistance, NavMesh.AllAreas))
        {
            sampled = hit.position;
            return true;
        }
        return false;
    }

    private void ShootAtPlayer(bool useTiredSpread)
    {
        if (!projectilePrefab || !player) return;

        Vector3 origin = muzzle ? muzzle.position : (transform.position + Vector3.up * 0.2f);
        Vector3 target = player.position; // posición ACTUAL del jugador
        Vector3 dir = (target - origin);
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        if (useTiredSpread && tiredSpreadDegrees > 0f)
        {
            dir = ApplySpread(dir, tiredSpreadDegrees);
        }

        GameObject proj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));
        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
        Destroy(proj, projectileLifetime);
    }

    private Vector3 ApplySpread(Vector3 dir, float degrees)
    {
        // Desviación cónica aleatoria
        Quaternion q = Quaternion.AngleAxis(Random.Range(-degrees, degrees), Random.onUnitSphere);
        return (q * dir).normalized;
    }

    // === Gizmos / Debug ===
    private void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Raycast hacia Player
        Gizmos.color = lastRayBlocked ? Color.red : Color.green;
        Gizmos.DrawLine(lastRayOrigin, lastRayHitOrEnd);

        // Flee target
        if (lastFleeTarget != Vector3.zero)
        {
            Gizmos.color = hadValidFlee ? Color.yellow : Color.magenta;
            Gizmos.DrawSphere(lastFleeTarget, 0.2f);
            Gizmos.DrawLine(transform.position, lastFleeTarget);
        }
    }
}

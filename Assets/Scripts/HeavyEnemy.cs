/*
 * =====================================================================================
 *
 * Filename:  HeavyEnemy.cs
 *
 * Description:  Implementa un enemigo "pesado" que persigue constantemente al jugador
 * usando el Steering Behavior de Pursuit y causa daño por impacto.
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

// Hereda de BaseEnemy para tener vida, poder morir, etc.
public class HeavyEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    [Tooltip("Fuerza de aceleración. Un valor bajo hace que se sienta pesado.")]
    public float accelerationForce = 5f;
    [Tooltip("Velocidad máxima que puede alcanzar.")]
    public float maxSpeed = 8f;

    [Header("Pursuit Behavior")]
    [Tooltip("Cuán adelante en el futuro intentará predecir la posición del jugador.")]
    public float predictionTime = 1f;

    [Header("Damage Scaling")]
    [Tooltip("Multiplicador de daño basado en la velocidad.")]
    public float speedDamageMultiplier = 2f;

    private Rigidbody rb;
    private Transform playerTransform;
    private Rigidbody playerRb; // Necesitamos el Rigidbody del jugador para leer su velocidad.

    protected override void Awake()
    {
        base.Awake(); // Llama a la lógica de BaseEnemy (inicializar vida, etc.)
        rb = GetComponent<Rigidbody>();

        // Busca al jugador al iniciar para perseguirlo siempre.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    // FixedUpdate es el mejor lugar para la física.
    private void FixedUpdate()
    {
        if (playerTransform != null)
        {
            PursuePlayer();
        }
    }

    /// <summary>
    /// Implementa la lógica de Persecución (Pursuit) para interceptar al jugador.
    /// </summary>
    private void PursuePlayer()
    {
        if (playerRb == null) // Si el jugador no tiene Rigidbody, no podemos predecir.
        {
            // En este caso, podríamos hacer un Seek simple como alternativa, pero por ahora no hacemos nada.
            return;
        }

        // 1. Predice la posición futura del jugador sumando su velocidad actual.
        Vector3 playerFuturePosition = playerTransform.position + (playerRb.linearVelocity * predictionTime);

        // 2. Calcula la dirección hacia esa posición futura.
        Vector3 directionToFuturePos = (playerFuturePosition - rb.position).normalized;

        // 3. Aplica una fuerza en esa dirección si no ha alcanzado la velocidad máxima.
        // El uso de AddForce da la sensación de inercia y "peso".
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(directionToFuturePos * accelerationForce);
        }

        // 4. Limita la velocidad máxima para que no acelere infinitamente.
        // Usamos ClampMagnitude por ser más conciso (sugerencia del profesor).
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

        // Opcional: rota al enemigo para que mire hacia donde se mueve.
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    // Sobreescribimos la función de colisión para añadir lógicas específicas.
    protected override void OnCollisionEnter(Collision collision)
    {
        // F) Si choca con el jugador, calcula el daño basado en la velocidad.
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Obtenemos la velocidad actual en el momento del impacto.
                float currentSpeed = rb.linearVelocity.magnitude;
                // Calculamos el daño: daño base + (velocidad * multiplicador).
                int finalDamage = Mathf.RoundToInt(contactDamage + (currentSpeed * speedDamageMultiplier));

                player.TakeDamage(finalDamage);
                Debug.Log($"Enemigo pesado golpeó al jugador. Velocidad: {currentSpeed:F2}, Daño: {finalDamage}");
            }
        }

        // E) Si choca con una pared, su velocidad se reinicia a cero.
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo implemento un Steering Behavior de 'Pursuit' en 3D?
     * - Se pidió la fórmula para predecir la posición futura de un objetivo basándose en su velocidad (`posición + velocidad * tiempo`)
     * y cómo usar esa posición futura como objetivo para un 'Seek'.
     * 2. ¿Cómo hago que el daño de un enemigo dependa de su velocidad de impacto?
     * - Se consultó cómo obtener la velocidad actual de un Rigidbody con `rb.velocity.magnitude` dentro
     * de `OnCollisionEnter` y usarla en una fórmula para calcular el daño final.
     * ================================================================
     */
}
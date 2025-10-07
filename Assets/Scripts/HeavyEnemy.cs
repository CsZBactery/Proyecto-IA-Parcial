using UnityEngine;

// Hereda de BaseEnemy para tener vida, poder morir, etc.
public class HeavyEnemy : BaseEnemy
{
    [Header("Movement Settings")]
    [Tooltip("C) Fuerza de aceleraci�n. Un valor bajo hace que se sienta pesado.")]
    public float accelerationForce = 5f;
    [Tooltip("C) Velocidad m�xima que puede alcanzar.")]
    public float maxSpeed = 8f;

    [Header("Pursuit Behavior")]
    [Tooltip("D) Cu�n adelante en el futuro intentar� predecir la posici�n del jugador.")]
    public float predictionTime = 1f;

    [Header("Damage Scaling")]
    [Tooltip("F) Multiplicador de da�o basado en la velocidad.")]
    public float speedDamageMultiplier = 2f;

    private Rigidbody rb;
    private Transform playerTransform;
    private Rigidbody playerRb; // Rigidbody del jugador para predecir su movimiento

    // Usamos 'override' para extender la funci�n Awake de la clase base.
    protected override void Awake()
    {
        base.Awake(); // Llama a la l�gica de BaseEnemy (inicializar vida, etc.)
        rb = GetComponent<Rigidbody>();

        // B) Encuentra al jugador al iniciar para perseguirlo siempre.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    // FixedUpdate es el mejor lugar para la f�sica.
    private void FixedUpdate()
    {
        if (playerTransform != null)
        {
            PursuePlayer();
        }
    }

    // D) L�gica de Persecuci�n (Pursuit)
    private void PursuePlayer()
    {
        // 1. Predice la posici�n futura del jugador
        Vector3 playerFuturePosition = playerTransform.position + (playerRb.linearVelocity * predictionTime);

        // 2. Calcula la direcci�n hacia esa posici�n futura
        Vector3 directionToFuturePos = (playerFuturePosition - rb.position).normalized;

        // 3. Aplica una fuerza en esa direcci�n para acelerar
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(directionToFuturePos * accelerationForce, ForceMode.Acceleration);
        }

        // 4. Limita la velocidad m�xima directamente
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Opcional: rota al enemigo para que mire hacia donde se mueve
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    // Sobreescribimos la funci�n de colisi�n para manejar el da�o y el choque con paredes.
    protected override void OnCollisionEnter(Collision collision)
    {
        // F) Si choca con el jugador, calcula el da�o basado en la velocidad
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                float currentSpeed = rb.linearVelocity.magnitude;
                int finalDamage = Mathf.RoundToInt(contactDamage + (currentSpeed * speedDamageMultiplier));

                player.TakeDamage(finalDamage);
                Debug.Log($"Enemigo pesado golpe� al jugador. Velocidad: {currentSpeed:F2}, Da�o: {finalDamage}");
            }
        }

        // E) Si choca con una pared, su velocidad se reinicia a cero.
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            rb.linearVelocity = Vector3.zero;
            Debug.Log("Enemigo pesado choc� contra una pared.");
        }
    }
}

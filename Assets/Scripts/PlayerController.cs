/*
 * =====================================================================================
 *
 * Filename:  PlayerController.cs
 *
 * Description:  Controla el movimiento, apuntado, disparo y salud del personaje jugable en 3D.
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
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 7f;
    private Rigidbody rb;
    private Vector3 movement;

    [Header("Apuntado y Disparo")]
    public LayerMask groundLayer; // Capa del suelo para que el rayo del mouse sepa dónde impactar.
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    [Header("Salud")]
    public int maxHealth = 100;
    public int currentHealth;

    private Camera cam;

    void Start()
    {
        // Obtenemos la referencia al componente Rigidbody para controlar las físicas.
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        cam = Camera.main; // Asignamos la cámara principal de la escena.
    }

    void Update()
    {
        // Captura la entrada del teclado para el movimiento en los ejes X (horizontal) y Z (vertical en el plano 3D).
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");

        // Detecta el clic izquierdo del mouse para disparar.
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // Comprueba si la vida ha llegado a cero para activar la función de muerte.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        // Aplica el movimiento al Rigidbody en FixedUpdate para una interacción correcta con las físicas.
        // Se normaliza el vector para evitar que el movimiento diagonal sea más rápido.
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        // --- Lógica de Apuntado con el Mouse en 3D ---
        // 1. Crea un rayo que va desde la cámara hacia la posición del cursor en la pantalla.
        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit groundHit; // Variable para almacenar la información del impacto del rayo.

        // 2. Lanza el rayo y comprueba si choca con algo en la capa "Ground".
        if (Physics.Raycast(cameraRay, out groundHit, 100f, groundLayer))
        {
            // 3. Si choca, obtenemos el punto exacto del impacto en el mundo 3D.
            Vector3 pointToLook = groundHit.point;
            // 4. Hacemos que el personaje rote para mirar hacia ese punto, pero manteniendo su altura actual
            //    para evitar que se incline hacia arriba o abajo.
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    void Shoot()
    {
        // Crea una nueva instancia de la bala (prefab) en la posición y rotación del 'FirePoint'.
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        // Aplica una fuerza a la bala en la dirección hacia donde está apuntando el 'FirePoint'.
        bulletRb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
    }

    // Función pública para que otros scripts (como el de la bala enemiga o el contacto del enemigo) puedan hacerle daño.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    void Die()
    {
        Debug.Log("El jugador ha muerto.");
        // Busca el GameManager en la escena y llama a su función para reiniciar el nivel.
        FindObjectOfType<GameManager>()?.RestartLevel();
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo puedo hacer que un personaje 3D apunte hacia el cursor del mouse en un juego con vista cenital?
     * - Se consultó sobre el método de usar un Raycast desde la cámara hacia una capa de suelo (Ground Layer)
     * para obtener un punto en el espacio 3D y usar `transform.LookAt`.
     * 2. ¿Cuál es la forma correcta de mover un personaje con un Rigidbody en 3D?
     * - Se investigó el uso de `Rigidbody.MovePosition` en `FixedUpdate` para un movimiento basado en físicas
     * que respeta las colisiones.
     * ================================================================
     */
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 7f;
    private Rigidbody rb;
    private Vector3 movement;

    [Header("Apuntado y Disparo")]
    public LayerMask groundLayer;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    [Header("Salud")]
    public int maxHealth = 100;
    public int currentHealth;

    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        cam = Camera.main;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        Ray cameraRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit groundHit;

        if (Physics.Raycast(cameraRay, out groundHit, 100f, groundLayer))
        {
            Vector3 pointToLook = groundHit.point;
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    void Die()
    {
        Debug.Log("El jugador ha muerto.");
        FindObjectOfType<GameManager>()?.RestartLevel();
    }
}
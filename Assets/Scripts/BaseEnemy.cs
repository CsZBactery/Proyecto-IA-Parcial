using System.Collections;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int contactDamage = 10;

    [Header("Effects")]
    public GameObject deathEffectPrefab;
    private Renderer meshRenderer;
    private Color originalColor;
    public float flashDuration = 0.1f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            StartCoroutine(FlashRed());
        }
        else
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            meshRenderer.material.color = originalColor;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }

    protected virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
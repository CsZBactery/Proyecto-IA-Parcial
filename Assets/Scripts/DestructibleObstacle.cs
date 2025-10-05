using System.Collections;
using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("Effects")]
    public GameObject destructionEffectPrefab;
    public float flashDuration = 0.1f;

    private Renderer meshRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            DestroyObstacle();
        }
        else
        {
            StartCoroutine(FlashEffect());
        }
    }

    private void DestroyObstacle()
    {
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private IEnumerator FlashEffect()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            meshRenderer.material.color = originalColor;
        }
    }
}
/*
 * =====================================================================================
 *
 * Filename:  DestructibleObstacle.cs
 *
 * Description:  Controla el comportamiento de un obstáculo 3D que puede ser destruido.
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

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("Effects")]
    public GameObject destructionEffectPrefab;
    public float flashDuration = 0.1f;

    // Usamos Renderer para ser compatible con cualquier objeto 3D.
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

    /// <summary>
    /// Función pública que será llamada por la bala del jugador para infligir daño.
    /// </summary>
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

    /// <summary>
    /// Se encarga de la lógica de destrucción del objeto.
    /// </summary>
    private void DestroyObstacle()
    {
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Corutina que hace que el objeto parpadee en rojo brevemente al ser golpeado.
    /// </summary>
    private IEnumerator FlashEffect()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            meshRenderer.material.color = originalColor;
        }
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo crear un objeto destructible con vida en Unity?
     * - Se consultó sobre la implementación de un sistema de vida simple (HP actual y máximo)
     * y una función `TakeDamage` para interactuar con otros scripts.
     * ================================================================
     */
}
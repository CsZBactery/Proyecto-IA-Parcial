/*
 * =====================================================================================
 *
 * Filename:  DestructibleObstacle.cs
 *
 * Description:  Controla el comportamiento de un obst�culo 3D que puede ser destruido.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingenier�a del Conocimiento
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
    /// Funci�n p�blica que ser� llamada por la bala del jugador para infligir da�o.
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
    /// Se encarga de la l�gica de destrucci�n del objeto.
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
     * 1. �C�mo crear un objeto destructible con vida en Unity?
     * - Se consult� sobre la implementaci�n de un sistema de vida simple (HP actual y m�ximo)
     * y una funci�n `TakeDamage` para interactuar con otros scripts.
     * ================================================================
     */
}
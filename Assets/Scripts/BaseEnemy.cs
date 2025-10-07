/*
 * =====================================================================================
 *
 * Filename:  BaseEnemy.cs
 *
 * Description:  Clase base para todos los enemigos. Proporciona funcionalidades
 * comunes como vida, recibir daño, morir y hacer daño por contacto.
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

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int contactDamage = 10;

    [Header("Effects")]
    public GameObject deathEffectPrefab; // Efecto de partículas al morir.
    private Renderer meshRenderer; // Usamos Renderer para objetos 3D.
    private Color originalColor;
    public float flashDuration = 0.1f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        // Obtenemos el Renderer del objeto (o de sus hijos) para poder cambiar su color.
        meshRenderer = GetComponentInChildren<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    // Función pública para que otros scripts (como el del proyectil) puedan infligirle daño.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth > 0)
        {
            // Si sigue vivo, activa el efecto de parpadeo.
            StartCoroutine(FlashRed());
        }
        else
        {
            // Si la vida llega a cero, muere.
            Die();
        }
    }

    // Corutina que cambia el color del material a rojo por un instante.
    private IEnumerator FlashRed()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            meshRenderer.material.color = originalColor;
        }
    }

    // Se activa al chocar con otro objeto con Collider y Rigidbody.
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Si choca con el jugador, le hace daño.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }

    // Función virtual para que clases hijas puedan sobreescribir y extender el comportamiento de muerte.
    protected virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo hago que un objeto 3D parpadee en rojo al recibir daño?
     * - Se consultó cómo acceder al `material` de un `Renderer` (en lugar de un `SpriteRenderer` 2D)
     * para cambiar su propiedad `color` dentro de una corutina.
     * 2. ¿Cómo implemento un sistema de vida base para que otros scripts lo hereden?
     * - Se investigó el uso de `public` para la función `TakeDamage` y `protected virtual` para
     * los métodos `Awake`, `OnCollisionEnter` y `Die`, permitiendo la herencia y sobreescritura.
     * ================================================================
     */
}
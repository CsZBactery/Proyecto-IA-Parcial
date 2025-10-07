/*
 * =====================================================================================
 *
 * Filename:  BaseEnemy.cs
 *
 * Description:  Clase base para todos los enemigos. Proporciona funcionalidades
 * comunes como vida, recibir da�o, morir y hacer da�o por contacto.
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

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int contactDamage = 10;

    [Header("Effects")]
    public GameObject deathEffectPrefab; // Efecto de part�culas al morir.
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

    // Funci�n p�blica para que otros scripts (como el del proyectil) puedan infligirle da�o.
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
        // Si choca con el jugador, le hace da�o.
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
        }
    }

    // Funci�n virtual para que clases hijas puedan sobreescribir y extender el comportamiento de muerte.
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
     * 1. �C�mo hago que un objeto 3D parpadee en rojo al recibir da�o?
     * - Se consult� c�mo acceder al `material` de un `Renderer` (en lugar de un `SpriteRenderer` 2D)
     * para cambiar su propiedad `color` dentro de una corutina.
     * 2. �C�mo implemento un sistema de vida base para que otros scripts lo hereden?
     * - Se investig� el uso de `public` para la funci�n `TakeDamage` y `protected virtual` para
     * los m�todos `Awake`, `OnCollisionEnter` y `Die`, permitiendo la herencia y sobreescritura.
     * ================================================================
     */
}
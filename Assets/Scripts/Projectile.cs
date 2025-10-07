/*
 * =====================================================================================
 *
 * Filename:  Projectile.cs
 *
 * Description:  Define el comportamiento de un proyectil, detectando colisiones,
 * infligiendo da�o y destruy�ndose al impactar o tras cierto tiempo.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingenier�a del Conocimiento
 *
 * =====================================================================================
 */

using UnityEngine;
public class Projectile : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Da�o infligido por el proyectil.")]
    public int damage = 25;

    [Header("Lifetime Settings")]
    [Tooltip("Tiempo de vida del proyectil si no colisiona.")]
    public float lifetime = 3f;

    void Start()
    {
        // Si no colisiona con nada en 'lifetime' segundos, se destruye.
        Destroy(gameObject, lifetime);
    }

    // Funci�n de Unity que se llama autom�ticamente cuando este objeto colisiona con otro.
    void OnCollisionEnter(Collision collision)
    {
        // Comprueba si el objeto impactado tiene el Tag "Enemy".
        if (collision.gameObject.CompareTag("ENEMY"))
        {
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        // Comprueba si es un obst�culo destructible.
        else if (collision.gameObject.CompareTag("Destructible"))
        {
            DestructibleObstacle obstacle = collision.gameObject.GetComponent<DestructibleObstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(damage);
            }
        }

        // En cualquier caso, el proyectil se destruye inmediatamente.
        DestroyBullet();
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. �Cu�l es la diferencia entre OnCollisionEnter2D y OnCollisionEnter en Unity?
     * - Se aclar� que `OnCollisionEnter` y el par�metro `Collision` son para f�sicas 3D,
     * mientras que las versiones '2D' son para el sistema de f�sicas 2D.
     * ================================================================
     */
}

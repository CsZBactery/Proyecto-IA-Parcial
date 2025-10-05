/*
 * =====================================================================================
 *
 * Filename:  Projectile.cs
 *
 * Description:  Define el comportamiento de un proyectil, detectando colisiones y destruy�ndose.
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
    // Funci�n de Unity que se llama autom�ticamente cuando este objeto colisiona con otro.
    void OnCollisionEnter(Collision collision)
    {
        bool shouldDestroy = false;

        // Comprueba si el objeto impactado tiene el Tag "Enemy".
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Intenta obtener el script BaseEnemy del enemigo.
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                // Si lo encuentra, le inflige da�o.
                enemy.TakeDamage(25);
            }
            shouldDestroy = true;
        }
        // Si no, comprueba si es un obst�culo destructible.
        else if (collision.gameObject.CompareTag("Destructible"))
        {
            DestructibleObstacle obstacle = collision.gameObject.GetComponent<DestructibleObstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(25);
            }
            shouldDestroy = true;
        }
        // Si no, comprueba si es una pared normal.
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            shouldDestroy = true;
        }

        // Si se cumpli� alguna de las condiciones, la bala se destruye para no seguir infinitamente.
        if (shouldDestroy)
        {
            DestroyBullet();
        }
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
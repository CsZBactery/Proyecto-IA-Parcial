using UnityEngine;

public class Projectile : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        bool shouldDestroy = false;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(25);
            }
            shouldDestroy = true;
        }
        else if (collision.gameObject.CompareTag("Destructible"))
        {
            DestructibleObstacle obstacle = collision.gameObject.GetComponent<DestructibleObstacle>();
            if (obstacle != null)
            {
                obstacle.TakeDamage(25);
            }
            shouldDestroy = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            shouldDestroy = true;
        }

        if (shouldDestroy)
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
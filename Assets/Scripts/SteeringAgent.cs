using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SteeringAgent : MonoBehaviour
{
    public enum Behavior { Seek, Flee, Pursuit }

    [Header("Configuraci�n de Comportamiento")]
    public Behavior currentBehavior = Behavior.Pursuit;

    [Header("Configuraci�n de Movimiento")]
    public float maxSpeed = 10f;
    public float maxForce = 15f;
    public float friction = 0.95f;

    [Header("Comportamiento 'Pursuit'")]
    public float predictionTime = 1f;

    [Header("Esquivar Obst�culos")]
    public bool canAvoidObstacles = true;
    public float obstacleDetectionDistance = 3f;
    public float obstacleAvoidanceForce = 200f;
    public LayerMask obstacleLayer;

    public Transform target;
    private Rigidbody targetRb;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            if (targetRb == null)
            {
                targetRb = target.GetComponent<Rigidbody>();
            }

            Vector3 primaryForce = Vector3.zero;
            switch (currentBehavior)
            {
                case Behavior.Seek:
                    primaryForce = Seek(target.position);
                    break;
                case Behavior.Flee:
                    primaryForce = Flee(target.position);
                    break;
                case Behavior.Pursuit:
                    if (targetRb != null)
                    {
                        primaryForce = Pursuit(target.position, targetRb.linearVelocity);
                    }
                    else
                    {
                        primaryForce = Seek(target.position);
                    }
                    break;
            }

            Vector3 avoidanceForce = Vector3.zero;
            if (canAvoidObstacles)
            {
                avoidanceForce = ObstacleAvoidance();
            }

            Vector3 finalForce = primaryForce + avoidanceForce;
            rb.AddForce(finalForce);
            LimitSpeed();
            ApplyRotation();
        }
        else
        {
            rb.linearVelocity *= friction;
        }
    }

    private Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized * maxSpeed;
        Vector3 steeringForce = desiredVelocity - rb.linearVelocity;
        return Vector3.ClampMagnitude(steeringForce, maxForce);
    }

    private Vector3 Flee(Vector3 targetPosition)
    {
        Vector3 desiredVelocity = (transform.position - targetPosition).normalized * maxSpeed;
        Vector3 steeringForce = desiredVelocity - rb.linearVelocity;
        return Vector3.ClampMagnitude(steeringForce, maxForce);
    }

    private Vector3 Pursuit(Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 futurePosition = targetPosition + (targetVelocity * predictionTime);
        return Seek(futurePosition);
    }

    private Vector3 ObstacleAvoidance()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleDetectionDistance, obstacleLayer))
        {
            Vector3 avoidanceDirection = hit.point - transform.position;
            Vector3 force = Vector3.Cross(Vector3.up, avoidanceDirection).normalized * obstacleAvoidanceForce;
            return force;
        }
        return Vector3.zero;
    }

    private void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void ApplyRotation()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(rb.linearVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}
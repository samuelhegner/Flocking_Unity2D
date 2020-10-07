using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private Vector2 velocity;

    private Vector2 acceleration;

    private float neighbourCount;

    [SerializeField] private float maxSpeed = 10f;


    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public Vector2 Acceleration
    {
        get => acceleration;
        set => acceleration = value;
    }

    public float MaxSpeed
    {
        get => maxSpeed;
        set => maxSpeed = value;
    }

    public float NeighbourCount
    {
        get => neighbourCount;
        set => neighbourCount = value;
    }

    void Awake()
    {
        velocity = Random.insideUnitCircle * maxSpeed;
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)Vector2.ClampMagnitude(velocity, maxSpeed);
        velocity += acceleration;
    }
}

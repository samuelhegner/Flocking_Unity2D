using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private Vector2 velocity;

    private Vector2 acceleration;

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
    }

    void Awake()
    {
        velocity = Random.insideUnitCircle * maxSpeed;
    }

    void Update()
    {
        transform.position += (Vector3)Vector2.ClampMagnitude(velocity, maxSpeed);
        velocity += acceleration;
    }
}

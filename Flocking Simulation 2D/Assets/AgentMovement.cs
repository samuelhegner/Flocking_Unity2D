using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private Vector2 velocity;
    [SerializeField] private float maxSpeed = 2f;


    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    void Awake()
    {
        velocity = new Vector2();
    }

    void Update()
    {
        Vector2 clampedVelocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += new Vector3(clampedVelocity.x, clampedVelocity.y, 0);
    }
}

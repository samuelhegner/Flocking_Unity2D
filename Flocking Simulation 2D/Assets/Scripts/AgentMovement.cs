using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private Vector2 velocity;

    private Vector2 acceleration;

    private float neighbourCount;

    [SerializeField] private float maxSpeed = 10f;

    Vector2 maxCam;
    Vector2 minCam;


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
        Camera cam = Camera.main;
        maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));
    }

    void Update()
    {
        Wrap();

        //print("The agents" + gameObject.name + "velocity:" + velocity);
        transform.position += (Vector3)Vector2.ClampMagnitude(velocity, maxSpeed);
        velocity += acceleration;
    }

    private void Wrap()
    {
        if (transform.position.x > maxCam.x)
        {
            transform.position = new Vector3(minCam.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < minCam.x)
        {
            transform.position = new Vector3(maxCam.x, transform.position.y, transform.position.z);
        }

        if (transform.position.y > maxCam.y)
        {
            transform.position = new Vector3(transform.position.x, minCam.y, transform.position.z);
        }
        else if (transform.position.y < minCam.y)
        {
            transform.position = new Vector3(transform.position.x, maxCam.y, transform.position.z);
        }

        //transform.position = new Vector3(Mathf.Clamp(transform.position.x, minCam.x, maxCam.x), Mathf.Clamp(transform.position.y, minCam.y, maxCam.y), transform.position.z);
    }
}

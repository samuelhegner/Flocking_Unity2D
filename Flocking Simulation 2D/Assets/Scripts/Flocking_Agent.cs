using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking_Agent : MonoBehaviour
{

    public List<GameObject> localAgents = new List<GameObject>();

    Rigidbody2D rb;

    public float maxSpeed;
    public float maxForce;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Random.insideUnitCircle * maxSpeed;
    }

    void Update()
    {
        rb.AddForce(Alignment(localAgents) + Cohesion(localAgents));

        Wrap();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!localAgents.Contains(other.gameObject)) {
            localAgents.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        localAgents.Remove(other.gameObject);
    }

    Vector2 Alignment(List<GameObject> agents) {
        Vector2 averageVelocity = new Vector2();
        Vector2 fastAverage = new Vector2();

        foreach (GameObject obj in agents) {
            averageVelocity += obj.GetComponent<Rigidbody2D>().velocity;
        }

        if (agents.Count > 0) {
            averageVelocity /= agents.Count;
            averageVelocity.Normalize();
            fastAverage = averageVelocity * maxSpeed;

            fastAverage -= rb.velocity;

            if (fastAverage.magnitude > maxForce)
            {
                fastAverage.Normalize();
                fastAverage *= maxForce;
            }
        }
        
        return fastAverage;
    }

    Vector2 Cohesion(List<GameObject> agents) {
        Vector2 averagePosition = new Vector2();
        Vector2 fasterAverage = new Vector2();

        foreach (GameObject obj in agents)
        {
            averagePosition += new Vector2(obj.transform.position.x, obj.transform.position.y);
        }

        if (agents.Count > 0)
        {
            averagePosition /= agents.Count;

            averagePosition -= new Vector2(transform.position.x, transform.position.y);
            averagePosition.Normalize();
            fasterAverage = averagePosition * maxSpeed;
            fasterAverage -= rb.velocity;

            if (fasterAverage.magnitude > maxForce) {
                fasterAverage.Normalize();
                fasterAverage *= maxForce;
            }
        }

        return fasterAverage;

    }

    void Wrap() {
        if (transform.position.x > 26)
        {
            transform.position = new Vector3(-26, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < -26) {
            transform.position = new Vector3(26, transform.position.y, transform.position.z);
        }

        if (transform.position.y > 15)
        {
            transform.position = new Vector3(transform.position.x, -15, transform.position.z);
        }
        else if (transform.position.y < -15) {
            transform.position = new Vector3(transform.position.x, 15, transform.position.z);
        }
    }
}

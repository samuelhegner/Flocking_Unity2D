using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking_Agent : MonoBehaviour
{

    public List<GameObject> localAgents = new List<GameObject>();

    Rigidbody2D rb;

    public float maxSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Random.insideUnitCircle * maxSpeed;
    }

    void Update()
    {
        rb.AddForce(Align(localAgents));

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

    Vector2 Align(List<GameObject> agents) {
        Vector2 averageVelocity = new Vector2();

        foreach (GameObject obj in agents) {
            averageVelocity += obj.GetComponent<Rigidbody2D>().velocity;
        }

        if (agents.Count > 0) {
            averageVelocity /= agents.Count;
            averageVelocity -= rb.velocity;
        }
        
        return averageVelocity;
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

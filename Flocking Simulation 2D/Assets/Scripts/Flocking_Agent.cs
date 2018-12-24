using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking_Agent : MonoBehaviour
{

    public bool cohesion;
    public bool seperation;
    public bool alignment;

    public Gradient colourGrad;

    SpriteRenderer sr;

    Spawn_Agents spawner;

    public List<GameObject> localAgents = new List<GameObject>();

    Rigidbody2D rb;

    Camera cam;

    public float maxSpeed;
    public float maxForce;

    float colEvaluation;

    int highestNumberOfAgents;

    ParticleSystem.MainModule mm;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Random.insideUnitCircle * maxSpeed * 2;

        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();

        spawner = GameObject.Find("Game_Manager").GetComponent<Spawn_Agents>();
        sr.color = colourGrad.Evaluate(0);

        mm = GetComponent<ParticleSystem>().main;

    }

    void Update()
    {
        if(localAgents.Count > highestNumberOfAgents && highestNumberOfAgents < spawner.numberOfAgents/4)
        highestNumberOfAgents = localAgents.Count;

        colEvaluation = (float)localAgents.Count / highestNumberOfAgents;
        sr.color = Color.Lerp(sr.color, colourGrad.Evaluate(colEvaluation), Time.deltaTime);

        mm.startColor = sr.color;


        Vector2 forceToAdd = new Vector2();

        if (cohesion) {
            forceToAdd += Cohesion(localAgents);
        }

        if (alignment) {
            forceToAdd += Alignment(localAgents);
        }

        if (seperation) {
            forceToAdd += Seperation(localAgents);
        }

        rb.AddForce(forceToAdd, ForceMode2D.Impulse);

        Wrap();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!localAgents.Contains(other.gameObject) && localAgents.Count< 20) {
            localAgents.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (localAgents.Contains(other.gameObject)) {
            localAgents.Remove(other.gameObject);
        }
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

            if (fastAverage.magnitude > maxForce * 2)
            {
                fastAverage.Normalize();
                fastAverage *= maxForce * 2;
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

    Vector2 Seperation(List<GameObject> agents) {
        Vector2 averagePosition = new Vector2();
        Vector2 fasterAverage = new Vector2();

        foreach (GameObject obj in agents)
        {
            float distance = Vector2.Distance(transform.position, obj.transform.position);

            Vector2 difference = transform.position - obj.transform.position;


            difference /= distance;
            

            averagePosition += difference;
        }

        if (agents.Count > 0)
        {
            averagePosition /= agents.Count;

            averagePosition.Normalize();
            averagePosition *= maxSpeed;

            fasterAverage = averagePosition - rb.velocity;
        }

        if (colEvaluation > 0.6)
        {
            if (fasterAverage.magnitude > maxForce)
            {
                fasterAverage.Normalize();
                fasterAverage *= maxForce * 2;
            }
        }
        else {
            if (fasterAverage.magnitude > maxForce)
            {
                fasterAverage.Normalize();
                fasterAverage *= maxForce;
            }
        }
        

        return fasterAverage;
    }

    void Wrap() {

        Vector2 maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        Vector2 minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        if (transform.position.x > maxCam.x)
        {
            transform.position = new Vector3(minCam.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x < minCam.x) {
            transform.position = new Vector3(maxCam.x, transform.position.y, transform.position.z);
        }

        if (transform.position.y > maxCam.y)
        {
            transform.position = new Vector3(transform.position.x, minCam.y, transform.position.z);
        }
        else if (transform.position.y < minCam.y) {
            transform.position = new Vector3(transform.position.x, maxCam.y, transform.position.z);
        }
    }
}

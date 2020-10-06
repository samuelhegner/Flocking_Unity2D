using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlAgents : MonoBehaviour
{
    

    [SerializeField] private float alignmentForce;
    [SerializeField] private float seperationForce;
    [SerializeField] private float cohesionForce;

    [SerializeField] private float agentNeighbourRange;

    [SerializeField] private GameObject agentPrefab;

    [SerializeField] private int numberOfAgentsToSpawn;

    [SerializeField] private List <Transform> agents;




    public float CohesionForce
    {
        get => cohesionForce;
    }

    public float SeperationForce
    {
        get => seperationForce;
    }

    public float AlignmentForce
    {
        get => alignmentForce;
    }

    public float NumberOfAgentsToSpawn
    {
        get => numberOfAgentsToSpawn;
    }

    void Start()
    {
        Camera cam =Camera.main;
        
        Vector2 maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        Vector2 minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            agents.Add(Instantiate(agentPrefab, new Vector2(Random.Range(minCam.x, maxCam.x), Random.Range(minCam.y, maxCam.y)), Quaternion.identity).transform);
        }
    }


    void Update()
    {
        for(int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            Vector2 velocity = Vector2.zero;

        }
    }

    Transform[] CalculateNeighbours(Transform agentToCheck)
    {


        List<Transform> neighbours = new List<Transform>();
        return neighbours.ToArray;
    }

    void CalculateAlignmentForce(float currentVelocityValue, Transform[] neighbours)
    {

    }

    void CalculateCohesionForce(float currentVelocityValue, Transform[] neighbours)
    {

    }

    void CalculateSeperationForce(float currentVelocityValue, Transform[] neighbours)
    {

    }
}

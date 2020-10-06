using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlAgents : MonoBehaviour
{


    [SerializeField] [Range(0, 2)] private float alignmentForce = 1;
    [SerializeField] [Range(0, 2)] private float separationForce = 1;
    [SerializeField] [Range(0, 2)] private float cohesionForce = 1;

    [SerializeField] private float agentNeighbourRange = 10f;

    [SerializeField] private GameObject agentPrefab;

    [SerializeField] private int numberOfAgentsToSpawn;

    [SerializeField] private List<AgentMovement> agents;




    public float CohesionForce
    {
        get => cohesionForce;
    }

    public float SeparationForce
    {
        get => separationForce;
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
        Camera cam = Camera.main;

        Vector2 maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        Vector2 minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            agents.Add(Instantiate(agentPrefab, new Vector2(Random.Range(minCam.x, maxCam.x), Random.Range(minCam.y, maxCam.y)), Quaternion.identity).GetComponent<AgentMovement>());
        }
    }


    void Update()
    {
        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            Vector2 velocity = Vector2.zero;

            AgentMovement[] agentNeighbours = CalculateNeighbours(agents[i]);

            velocity += CalculateSeparationForce(agents[i], agentNeighbours) * separationForce;

            velocity += CalculateAlignmentForce(agents[i], agentNeighbours) * alignmentForce;

            velocity += CalculateCohesionForce(agents[i], agentNeighbours) * cohesionForce;

            agents[i].Velocity = velocity.normalized;
        }
    }

    AgentMovement[] CalculateNeighbours(AgentMovement agentToCheck)
    {
        List<AgentMovement> neighbours = new List<AgentMovement>();

        for (int i = 0; i < agents.Count; i++)
        {
            if (agentToCheck != agents[i])
            {
                if (Vector2.Distance(agentToCheck.transform.position, agents[i].transform.position) < agentNeighbourRange)
                {
                    neighbours.Add(agents[i]);
                }
            }
        }

        return neighbours.ToArray();
    }

    Vector2 CalculateAlignmentForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 runningTotal = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            runningTotal += neighbours[i].Velocity;
        }

        Vector2 averageRunningTotal = runningTotal / neighbours.Length;
        return averageRunningTotal.normalized;
    }

    Vector2 CalculateCohesionForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 runningTotalPosition = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            runningTotalPosition += Vector3Extension.AsVector2(neighbours[i].transform.position);
        }

        Vector2 AverageRunningTotalPosition = runningTotalPosition / neighbours.Length;

        Vector2 velocityToAverageRunningTotalPosition = AverageRunningTotalPosition - Vector3Extension.AsVector2(currentAgent.transform.position);


        return velocityToAverageRunningTotalPosition.normalized;
    }

    Vector2 CalculateSeparationForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 runningTotal = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            runningTotal += Vector3Extension.AsVector2(currentAgent.transform.position) - Vector3Extension.AsVector2(neighbours[i].transform.position);
        }

        Vector2 averageRunningTotal = runningTotal / neighbours.Length;

        return averageRunningTotal.normalized;
    }
}



//Extension that allows for easier access of the X and Y of a Vector3
public static class Vector3Extension
{
    public static Vector2 AsVector2(this Vector3 _v)
    {
        return new Vector2(_v.x, _v.y);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlAgents : MonoBehaviour
{


    [SerializeField] [Range(0, 2)] private float alignmentForce = 1;
    [SerializeField] [Range(0, 2)] private float separationForce = 1;
    [SerializeField] [Range(0, 2)] private float cohesionForce = 1;

    [SerializeField] [Range(0, 2)] private float maximumForce = 2f;


    [SerializeField] bool useVisionRadius = false;

    [SerializeField] [Range(0, 180)] private float visionRadiusAngle = 90f;




    [SerializeField] private float agentNeighbourRange = 10f;

    [SerializeField] private GameObject agentPrefab;

    [SerializeField] [Range(0, 500)] private int numberOfAgentsToSpawn = 200;

    [SerializeField] private List<AgentMovement> agents;

    Vector2 maxCam;
    Vector2 minCam;


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

        maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            agents.Add(Instantiate(agentPrefab, new Vector2(Random.Range(minCam.x, maxCam.x), Random.Range(minCam.y, maxCam.y)), Quaternion.identity, transform).GetComponent<AgentMovement>());
        }
    }


    void Update()
    {
        UpdateAgentCount();

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            Wrap(agents[i].transform);

            Vector2 calculatedSteeringForce = Vector2.zero;

            AgentMovement[] agentNeighbours = CalculateNeighbours(agents[i]);

            calculatedSteeringForce += CalculateSeparationForce(agents[i], agentNeighbours) * separationForce;

            calculatedSteeringForce += CalculateAlignmentForce(agents[i], agentNeighbours) * alignmentForce;

            calculatedSteeringForce += CalculateCohesionForce(agents[i], agentNeighbours) * cohesionForce;

            agents[i].Acceleration = calculatedSteeringForce;
        }
    }

    AgentMovement[] CalculateNeighbours(AgentMovement agentToCheck)
    {
        List<AgentMovement> neighbours = new List<AgentMovement>();

        for (int i = 0; i < agents.Count; i++)
        {
            if (agentToCheck != agents[i])
            {
                if (CheckForNeighbourInFront(agentToCheck, agents[i]))
                {
                    if (Vector2.Distance(agentToCheck.transform.position, agents[i].transform.position) < agentNeighbourRange)
                    {
                        neighbours.Add(agents[i]);
                    }
                }

            }
        }

        agentToCheck.NeighbourCount = neighbours.Count;
        return neighbours.ToArray();
    }

    Vector2 CalculateAlignmentForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 steeringForce = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            steeringForce += neighbours[i].Velocity;
        }


        if (neighbours.Length > 0)
        {
            steeringForce /= neighbours.Length;
            steeringForce.Normalize();
            steeringForce *= currentAgent.MaxSpeed;
            steeringForce -= currentAgent.Velocity;
            steeringForce = Vector2.ClampMagnitude(steeringForce, maximumForce);
        }

        return steeringForce;
    }

    Vector2 CalculateCohesionForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 steeringForce = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            steeringForce += (Vector2)neighbours[i].transform.position;
        }


        if (neighbours.Length > 0)
        {
            steeringForce /= neighbours.Length;
            steeringForce -= (Vector2)currentAgent.transform.position;
            steeringForce.Normalize();
            steeringForce *= currentAgent.MaxSpeed;
            steeringForce -= currentAgent.Velocity;
            steeringForce = Vector2.ClampMagnitude(steeringForce, maximumForce);
        }

        return steeringForce;
    }

    Vector2 CalculateSeparationForce(AgentMovement currentAgent, AgentMovement[] neighbours)
    {
        Vector2 steeringForce = new Vector2();

        for (int i = 0; i < neighbours.Length; i++)
        {
            float distanceToNeighbour = Vector2.Distance(currentAgent.transform.position, neighbours[i].transform.position);

            Vector2 vectorToCurrentAgent = currentAgent.transform.position - neighbours[i].transform.position;
            vectorToCurrentAgent /= distanceToNeighbour;

            steeringForce += vectorToCurrentAgent;
        }

        if (neighbours.Length > 0)
        {
            steeringForce /= neighbours.Length;
            steeringForce.Normalize();
            steeringForce *= currentAgent.MaxSpeed;
            steeringForce -= currentAgent.Velocity;
            steeringForce = Vector2.ClampMagnitude(steeringForce, maximumForce);
        }

        return steeringForce;
    }


    void Wrap(Transform agent)
    {

        if (agent.position.x > maxCam.x)
        {
            agent.position = new Vector3(minCam.x, agent.position.y, agent.position.z);
        }
        else if (agent.position.x < minCam.x)
        {
            agent.position = new Vector3(maxCam.x, agent.position.y, agent.position.z);
        }

        if (agent.position.y > maxCam.y)
        {
            agent.position = new Vector3(agent.position.x, minCam.y, agent.position.z);
        }
        else if (agent.position.y < minCam.y)
        {
            agent.position = new Vector3(agent.position.x, maxCam.y, agent.position.z);
        }
    }


    bool CheckForNeighbourInFront(AgentMovement currentAgent, AgentMovement agentToCheck)
    {
        if (useVisionRadius)
        {
            Vector2 toAgentToCheck = agentToCheck.transform.position - currentAgent.transform.position;

            return Vector2.Angle(currentAgent.Velocity, toAgentToCheck) < visionRadiusAngle;
        }
        else
        {
            return true;
        }
    }

    void UpdateAgentCount()
    {
        if (agents.Count == numberOfAgentsToSpawn) return;

        if (agents.Count < numberOfAgentsToSpawn)
        {
            while (agents.Count < numberOfAgentsToSpawn)
            {
                agents.Add(Instantiate(agentPrefab,
                             new Vector2(Random.Range(minCam.x, maxCam.x), Random.Range(minCam.y, maxCam.y)),
                              Quaternion.identity,
                               transform).GetComponent<AgentMovement>());
            }

        }
        else
        {
            while (agents.Count > numberOfAgentsToSpawn)
            {
                AgentMovement agentToRemove = agents[agents.Count - 1];
                agents.Remove(agentToRemove);
                Destroy(agentToRemove.gameObject);
            }

        }
    }
}
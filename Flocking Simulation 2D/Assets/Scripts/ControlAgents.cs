using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Jobs;

public class ControlAgents : MonoBehaviour
{
    [SerializeField] [Range(0, 500)] private int numberOfAgentsToSpawn = 200;

    [SerializeField] private bool useJobs;



    [SerializeField] private GameObject agentPrefab;


    [SerializeField] private float agentAlignmentForce;
    [SerializeField] private float agentSeparationForce;
    [SerializeField] private float agentCohesionForce;

    [SerializeField] private float agentMaxSpeed;
    [SerializeField] private float agentMaxForce;

    [SerializeField] bool useVisionRadius;

    [SerializeField] private float visionRadiusAngle;

    [SerializeField] private float agentPerceptionRange;

    private List<AgentMovement> agents = new List<AgentMovement>();

    Vector2 maxCam;
    Vector2 minCam;

    public int NumberOfAgentsToSpawn { get => numberOfAgentsToSpawn; set => numberOfAgentsToSpawn = value; }
    public float AgentAlignmentForce { get => agentAlignmentForce; set => agentAlignmentForce = value; }
    public float AgentSeparationForce { get => agentSeparationForce; set => agentSeparationForce = value; }
    public float AgentCohesionForce { get => agentCohesionForce; set => agentCohesionForce = value; }
    public float AgentMaxSpeed { get => agentMaxSpeed; set => agentMaxSpeed = value; }
    public float AgentMaxForce { get => agentMaxForce; set => agentMaxForce = value; }
    public bool UseVisionRadius { get => useVisionRadius; set => useVisionRadius = value; }
    public float VisionRadiusAngle { get => visionRadiusAngle; set => visionRadiusAngle = value; }
    public float AgentPerceptionRange { get => agentPerceptionRange; set => agentPerceptionRange = value; }
    public bool UseJobs { get => useJobs; set => useJobs = value; }

    void Start()
    {
        Camera cam = Camera.main;

        maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {

            agents.Add(Instantiate(agentPrefab, new Vector2(UnityEngine.Random.Range(minCam.x, maxCam.x), UnityEngine.Random.Range(minCam.y, maxCam.y)), Quaternion.identity, transform).GetComponent<AgentMovement>());
            agents[i].transform.name = i.ToString();
        }

    }


    void Update()
    {
        if (useJobs)
        {
            UpdateAgentCount();


            NativeArray<float2> calculatedSteeringForceArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);

            NativeArray<float2> positionArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);

            NativeArray<float2> velocityArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);


            //Fill native arrays with data
            for (int i = 0; i < agents.Count; i++)
            {
                UpdateAgentSpeed(agents[i]);
                calculatedSteeringForceArray[i] = 0;
                positionArray[i] = (Vector2)agents[i].transform.position;
                velocityArray[i] = agents[i].Velocity;
            }

            FlockingParallelJob flockingParallelJob = new FlockingParallelJob
            {
                calculatedSteeringForceArray = calculatedSteeringForceArray,
                positionArray = positionArray,
                velocityArray = velocityArray,
                maxForce = agentMaxForce,
                maxSpeed = agentMaxSpeed,
                perceptionRange = agentPerceptionRange,

                alignmentForceMult = agentAlignmentForce,
                separationForceMult = agentSeparationForce,
                cohesionForceMult = agentCohesionForce
            };

            JobHandle jobHandle = flockingParallelJob.Schedule(agents.Count, 1);

            jobHandle.Complete();



            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].Acceleration = calculatedSteeringForceArray[i];
            }



            positionArray.Dispose();
            velocityArray.Dispose();
            calculatedSteeringForceArray.Dispose();
        }
        else
        {
            // Old non Job system implementation
            for (int i = 0; i < numberOfAgentsToSpawn; i++)
            {
                UpdateAgentSpeed(agents[i]);
                Wrap(agents[i].transform);

                Vector2 calculatedSteeringForce = Vector2.zero;

                AgentMovement[] agentNeighbours = CalculateNeighbours(agents[i]);

                calculatedSteeringForce += CalculateSeparationForce(agents[i], agentNeighbours) * agentSeparationForce;

                calculatedSteeringForce += CalculateAlignmentForce(agents[i], agentNeighbours) * agentAlignmentForce;

                calculatedSteeringForce += CalculateCohesionForce(agents[i], agentNeighbours) * agentCohesionForce;

                agents[i].Acceleration = calculatedSteeringForce;
            }
        }
    }

    private void UpdateAgentSpeed(AgentMovement agentToCheck)
    {
        if (agentToCheck.MaxSpeed != agentMaxSpeed)
        {
            agentToCheck.MaxSpeed = agentMaxSpeed;
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
                    if (Vector2.Distance(agentToCheck.transform.position, agents[i].transform.position) < agentPerceptionRange)
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
            steeringForce = Vector2.ClampMagnitude(steeringForce, agentMaxForce);
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
            steeringForce = Vector2.ClampMagnitude(steeringForce, agentMaxForce);
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
            steeringForce = Vector2.ClampMagnitude(steeringForce, agentMaxForce);
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
                             new Vector2(UnityEngine.Random.Range(minCam.x, maxCam.x), UnityEngine.Random.Range(minCam.y, maxCam.y)),
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

[BurstCompile]
public struct FlockingParallelJob : IJobParallelFor
{
    [WriteOnly] public NativeArray<float2> calculatedSteeringForceArray;

    [ReadOnly] public NativeArray<float2> positionArray;

    [ReadOnly] public NativeArray<float2> velocityArray;

    public float maxSpeed;
    public float maxForce;
    public float perceptionRange;

    public float alignmentForceMult;
    public float separationForceMult;
    public float cohesionForceMult;


    public void Execute(int index)
    {
        float2 tempForce = new float2();

        float2 alignmentForce = new float2();
        float2 separationForce = new float2();
        float2 cohesionForce = new float2();

        int neighbourCount = 0;

        //check distance
        for (int i = 0; i < positionArray.Length; i++)
        {
            float distance = math.distance(positionArray[index], positionArray[i]);

            if (index != i && distance < perceptionRange)
            {
                //alignment
                alignmentForce += velocityArray[i];

                //cohesion
                cohesionForce += positionArray[i];

                //separation
                float2 vectorToCurrentAgent = positionArray[index] - positionArray[i];
                vectorToCurrentAgent /= distance;

                separationForce += vectorToCurrentAgent;


                //increase Neighbour count
                neighbourCount++;
            }
        }



        if (neighbourCount > 0)
        {
            //alignment
            alignmentForce /= neighbourCount;
            alignmentForce = math.normalize(alignmentForce);
            alignmentForce *= maxSpeed;
            alignmentForce -= velocityArray[index];
            alignmentForce = Vector2.ClampMagnitude(alignmentForce, maxForce);

            //cohesion
            cohesionForce /= neighbourCount;
            cohesionForce -= positionArray[index];
            cohesionForce = math.normalize(cohesionForce);
            cohesionForce *= maxSpeed;
            cohesionForce -= velocityArray[index];
            cohesionForce = Vector2.ClampMagnitude(cohesionForce, maxForce);

            //separation
            separationForce /= neighbourCount;
            separationForce = math.normalize(separationForce);
            separationForce *= maxSpeed;
            separationForce -= velocityArray[index];
            separationForce = Vector2.ClampMagnitude(separationForce, maxForce);
        }

        //add values and multiply by editable values
        tempForce += alignmentForce * alignmentForceMult;
        tempForce += cohesionForce * cohesionForceMult;
        tempForce += separationForce * separationForceMult;

        calculatedSteeringForceArray[index] = tempForce;
    }
}

struct MoveAgentsParallelJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> calculatedSteeringForceArray;

    public NativeArray<float2> velocityArray;

    public NativeArray<float2> positionArray;

    public float maxSpeed;

    public void Execute(int index)
    {
        positionArray[index] += (float2)Vector2.ClampMagnitude(velocityArray[index], maxSpeed);
        velocityArray[index] += calculatedSteeringForceArray[index];
    }
}
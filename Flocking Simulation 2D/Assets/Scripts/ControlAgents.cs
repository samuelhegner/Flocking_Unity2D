using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Jobs;
using System.Linq;

public class ControlAgents : MonoBehaviour
{
    [SerializeField] private float numberOfAgentsToSpawn = 200;
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private float agentAlignmentForce;
    [SerializeField] private float agentSeparationForce;
    [SerializeField] private float agentCohesionForce;
    [SerializeField] private float agentMaxSpeed;
    [SerializeField] private float agentMaxForce;
    [SerializeField] bool useVisionRadius;
    [SerializeField] private float visionRadiusAngle;
    [SerializeField] private float agentPerceptionRange;
    [SerializeField] private float adjustmentMultiplier = 20f;
    [SerializeField] private Gradient colourGradient;
    [SerializeField] private float colourLerpSpeed = 1f;
    private List<AgentData> agents = new List<AgentData>();

    Vector2 maxCam;
    Vector2 minCam;

    public float NumberOfAgentsToSpawn { get => Mathf.RoundToInt(numberOfAgentsToSpawn); set => numberOfAgentsToSpawn = value; }
    public float AgentAlignmentForce { get => agentAlignmentForce; set => agentAlignmentForce = value; }
    public float AgentSeparationForce { get => agentSeparationForce; set => agentSeparationForce = value; }
    public float AgentCohesionForce { get => agentCohesionForce; set => agentCohesionForce = value; }
    public float AgentMaxSpeed { get => agentMaxSpeed; set => agentMaxSpeed = value; }
    public float AgentMaxForce { get => agentMaxForce; set => agentMaxForce = value; }
    public bool UseVisionRadius { get => useVisionRadius; set => useVisionRadius = value; }
    public float VisionRadiusAngle { get => visionRadiusAngle; set => visionRadiusAngle = value; }
    public float AgentPerceptionRange { get => agentPerceptionRange; set => agentPerceptionRange = value; }
    public float AdjustmentMultiplier { get => adjustmentMultiplier; set => adjustmentMultiplier = value; }

    public float ColourLerpSpeed { get => colourLerpSpeed; set => colourLerpSpeed = value; }

    public Gradient ColourGradient { get => colourGradient; set => colourGradient = value; }



    void Start()
    {
        Camera cam = Camera.main;

        maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            CreateNewAgent();
        }
    }


    void Update()
    {
        //Create or Destroy Agents if required
        UpdateAgentCount();

        //Create Native Arrays for Jobs
        NativeArray<float2> calculatedSteeringForceArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);

        NativeArray<float2> positionArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);

        NativeArray<float2> velocityArray = new NativeArray<float2>(agents.Count, Allocator.TempJob);

        NativeArray<int> neighbourCountArray = new NativeArray<int>(agents.Count, Allocator.TempJob);

        //Fill native arrays with data
        for (int i = 0; i < agents.Count; i++)
        {
            calculatedSteeringForceArray[i] = 0;
            positionArray[i] = (Vector2)agents[i].agentTransform.position;
            velocityArray[i] = agents[i].velocity;
            neighbourCountArray[i] = agents[i].neighbourCount;
        }

        //Create the Flocking Force Calculation Job
        FlockingParallelJob flockingParallelJob = new FlockingParallelJob
        {
            calculatedSteeringForceArray = calculatedSteeringForceArray,
            positionArray = positionArray,
            velocityArray = velocityArray,
            neighbourCountArray = neighbourCountArray,
            maxForce = agentMaxForce,
            maxSpeed = agentMaxSpeed,
            perceptionRange = agentPerceptionRange,

            alignmentForceMult = agentAlignmentForce,
            separationForceMult = agentSeparationForce,
            cohesionForceMult = agentCohesionForce,
        };

        //Schedules the flocking Job and wait for it to finish its calculations
        JobHandle flockingJobHandle = flockingParallelJob.Schedule(agents.Count, 100);
        flockingJobHandle.Complete();

        //Create the Agents Movement job
        MoveAgentsParallelJob moveAgentsParallelJob = new MoveAgentsParallelJob
        {
            calculatedSteeringForceArray = calculatedSteeringForceArray,
            positionArray = positionArray,
            velocityArray = velocityArray,
            maxSpeed = agentMaxSpeed,
            deltaTime = Time.deltaTime,
            minCam = minCam,
            maxCam = maxCam,
            adjustmentMult = adjustmentMultiplier
        };

        //Schedule the Movement job and wait for it to complete
        JobHandle movementJobHandle = moveAgentsParallelJob.Schedule(agents.Count, 100);
        movementJobHandle.Complete();

        //Slot the calculated information back into the agents
        for (int i = 0; i < agents.Count; i++)
        {
            //Set the agents movement information
            agents[i].velocity = new float2(velocityArray[i]);
            agents[i].neighbourCount = neighbourCountArray[i];
            agents[i].agentTransform.position = new Vector3(positionArray[i].x, positionArray[i].y, 0);


            //Calculate the agents colour value
            if (agents[i].highestNeighbourCount < agents[i].neighbourCount)
            {
                agents[i].highestNeighbourCount = agents[i].neighbourCount;
            }
            
            if (agents[i].highestNeighbourCount > agents.Count)
            {
                agents[i].highestNeighbourCount = Mathf.Clamp(agents.Count/8, 1, agents.Count);
            }
            
            float evaluationAmount = 0;

            SpriteRenderer rend = agents[i].spriteRenderer;
            
            if (agents[i].neighbourCount > 0)
            {
                evaluationAmount = (float)agents[i].neighbourCount / agents[i].highestNeighbourCount;
            }

            rend.color = Color.Lerp(rend.color, colourGradient.Evaluate(evaluationAmount), Time.deltaTime * colourLerpSpeed);
        }

        //Dispose the Native Arrays
        positionArray.Dispose();
        velocityArray.Dispose();
        calculatedSteeringForceArray.Dispose();
        neighbourCountArray.Dispose();


        // Old non Job system implementation
        /*for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            UpdateAgentSpeed(agents[i]);
            Wrap(agents[i].transform);

            Vector2 calculatedSteeringForce = Vector2.zero;

            AgentMovement[] agentNeighbours = CalculateNeighbours(agents[i]);

            calculatedSteeringForce += CalculateSeparationForce(agents[i], agentNeighbours) * agentSeparationForce;

            calculatedSteeringForce += CalculateAlignmentForce(agents[i], agentNeighbours) * agentAlignmentForce;

            calculatedSteeringForce += CalculateCohesionForce(agents[i], agentNeighbours) * agentCohesionForce;

            agents[i].Acceleration = calculatedSteeringForce;
        }*/

    }


    /*    AgentMovement[] CalculateNeighbours(AgentMovement agentToCheck)
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
        }*/

    void UpdateAgentCount()
    {
        if (agents.Count == (int)numberOfAgentsToSpawn) return;

        if (agents.Count < (int)numberOfAgentsToSpawn)
        {
            while (agents.Count < (int)numberOfAgentsToSpawn)
            {
                CreateNewAgent();
            }

        }
        else
        {
            while (agents.Count > (int)numberOfAgentsToSpawn)
            {
                AgentData agentToRemove = agents[agents.Count - 1];
                agents.Remove(agentToRemove);
                Destroy(agentToRemove.agentTransform.gameObject);
            }

        }
    }

    void CreateNewAgent()
    {
        GameObject newAgent = Instantiate(agentPrefab, new Vector2(UnityEngine.Random.Range(minCam.x, maxCam.x), UnityEngine.Random.Range(minCam.y, maxCam.y)), Quaternion.identity, transform);

        newAgent.name = "Agent " + agents.Count.ToString();

        AgentData newAgentData = new AgentData();

        newAgentData.agentTransform = newAgent.transform;
        newAgentData.velocity = UnityEngine.Random.insideUnitCircle * agentMaxSpeed;
        newAgentData.spriteRenderer = newAgent.GetComponent<SpriteRenderer>();
        newAgentData.neighbourCount = 0;
        newAgentData.highestNeighbourCount = 0;

        agents.Add(newAgentData);
    }
}

[BurstCompile]
public struct FlockingParallelJob : IJobParallelFor
{
    [WriteOnly] public NativeArray<float2> calculatedSteeringForceArray;

    [ReadOnly] public NativeArray<float2> positionArray;

    [ReadOnly] public NativeArray<float2> velocityArray;

    public NativeArray<int> neighbourCountArray;

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

        if (neighbourCount != neighbourCountArray[index])
            neighbourCountArray[index] = neighbourCount;

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

[BurstCompile]
struct MoveAgentsParallelJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> calculatedSteeringForceArray;

    public NativeArray<float2> velocityArray;

    public NativeArray<float2> positionArray;

    public float maxSpeed;

    public float deltaTime;
    public float adjustmentMult;

    public float2 minCam;
    public float2 maxCam;


    public void Execute(int index)
    {
        if (positionArray[index].x > maxCam.x)
        {
            positionArray[index] = new float2(minCam.x, positionArray[index].y);
        }
        else if (positionArray[index].x < minCam.x)
        {
            positionArray[index] = new float2(maxCam.x, positionArray[index].y);
        }

        if (positionArray[index].y > maxCam.y)
        {
            positionArray[index] = new float2(positionArray[index].x, minCam.y);
        }
        else if (positionArray[index].y < minCam.y)
        {
            positionArray[index] = new float2(positionArray[index].x, maxCam.y);
        }


        float2 veloctyPreClamp = (velocityArray[index]) * (deltaTime * adjustmentMult);
        positionArray[index] += (float2)Vector2.ClampMagnitude(veloctyPreClamp, maxSpeed);

        velocityArray[index] += (calculatedSteeringForceArray[index]) * (deltaTime * adjustmentMult);
    }
}


class AgentData
{
    public Transform agentTransform;
    public float2 velocity;
    public SpriteRenderer spriteRenderer;
    public int neighbourCount;
    public int highestNeighbourCount;
}
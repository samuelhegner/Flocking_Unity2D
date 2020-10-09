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
    [SerializeField] private int numberOfAgentsToSpawn = 200;

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

    [SerializeField] private float adjustmentMultiplier = 20f;

    private List<AgentData> agents = new List<AgentData>();

    private MaterialPropertyBlock materialProperty;

    Vector2 maxCam;
    Vector2 minCam;

    private int numberOfNeighboursPropertyID;
    private int numberOfHighestNeighboursPropertyID;

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
    public float AdjustmentMultiplier { get => adjustmentMultiplier; set => adjustmentMultiplier = value; }


    void Start()
    {
        Camera cam = Camera.main;
        
        maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        materialProperty = new MaterialPropertyBlock();

        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            GameObject newAgent = Instantiate(agentPrefab, new Vector2(UnityEngine.Random.Range(minCam.x, maxCam.x), UnityEngine.Random.Range(minCam.y, maxCam.y)), Quaternion.identity, transform);

            newAgent.name = "Agent " + i.ToString();

            AgentData newAgentData = new AgentData();

            newAgentData.agentTransform = newAgent.transform;
            newAgentData.velocity = UnityEngine.Random.insideUnitCircle * agentMaxSpeed;
            newAgentData.renderer = newAgent.GetComponent<Renderer>();
            newAgentData.neighbourCount = 0;
            newAgentData.highestNeighbourCount = 0;

            agents.Add(newAgentData);
        }

        numberOfNeighboursPropertyID = Shader.PropertyToID("_numberOfNeighbours");
        numberOfHighestNeighboursPropertyID = Shader.PropertyToID("_numberOfHighestNeighbours");
        agents[0].renderer.GetPropertyBlock(materialProperty);
    }


    void Update()
    {
        if (useJobs)
        {
            UpdateAgentCount();


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

                deltaTime = Time.deltaTime
            };

            JobHandle flockingJobHandle = flockingParallelJob.Schedule(agents.Count, 100);

            flockingJobHandle.Complete();

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

            JobHandle movementJobHandle = moveAgentsParallelJob.Schedule(agents.Count, 100);
            
            movementJobHandle.Complete();

            //Shader.SetGlobalFloatArray("_numberOfHighestNeighbours", agents.);

            //Shader.SetGlobalVectorArray("")

            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].velocity = new float2(velocityArray[i]);
                agents[i].neighbourCount = neighbourCountArray[i];
                
                if (agents[i].highestNeighbourCount < agents[i].neighbourCount) 
                {
                    agents[i].highestNeighbourCount = agents[i].neighbourCount;
                }
                agents[i].agentTransform.position = new Vector3(positionArray[i].x, positionArray[i].y, 0);



                materialProperty.SetInt(numberOfNeighboursPropertyID, agents[i].neighbourCount);
                materialProperty.SetInt(numberOfHighestNeighboursPropertyID, agents[i].highestNeighbourCount);

                agents[i].renderer.SetPropertyBlock(materialProperty);

            }

            positionArray.Dispose();
            velocityArray.Dispose();
            calculatedSteeringForceArray.Dispose();
            neighbourCountArray.Dispose();
        }
        else
        {
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
        if (agents.Count == numberOfAgentsToSpawn) return;

        if (agents.Count < numberOfAgentsToSpawn)
        {
            while (agents.Count < numberOfAgentsToSpawn)
            {
                GameObject newAgent = Instantiate(agentPrefab, new Vector2(UnityEngine.Random.Range(minCam.x, maxCam.x), UnityEngine.Random.Range(minCam.y, maxCam.y)), Quaternion.identity, transform);

                newAgent.name = "Agent " + agents.Count.ToString();

                AgentData newAgentData = new AgentData();

                newAgentData.agentTransform = newAgent.transform;
                newAgentData.velocity = UnityEngine.Random.insideUnitCircle * agentMaxSpeed;
                newAgentData.renderer = newAgent.GetComponent<Renderer>();
                newAgentData.neighbourCount = 0;
                newAgentData.highestNeighbourCount = 0;


                agents.Add(newAgentData);
            }

        }
        else
        {
            while (agents.Count > numberOfAgentsToSpawn)
            {
                AgentData agentToRemove = agents[agents.Count - 1];
                agents.Remove(agentToRemove);
                Destroy(agentToRemove.agentTransform.gameObject);
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

    public NativeArray<int> neighbourCountArray;

    public float maxSpeed;
    public float maxForce;
    public float perceptionRange;

    public float alignmentForceMult;
    public float separationForceMult;
    public float cohesionForceMult;

    public float deltaTime;


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

        if(neighbourCount != neighbourCountArray[index])
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


        float2 veloctyPreClamp = (velocityArray[index] * adjustmentMult) * deltaTime;
        positionArray[index] += (float2)Vector2.ClampMagnitude(veloctyPreClamp, maxSpeed);

        velocityArray[index] += (calculatedSteeringForceArray[index] * adjustmentMult) * deltaTime;
    }
}


class AgentData
{
    public Transform agentTransform;
    public float2 velocity;
    public Renderer renderer;
    public int neighbourCount;
    public int highestNeighbourCount;
}
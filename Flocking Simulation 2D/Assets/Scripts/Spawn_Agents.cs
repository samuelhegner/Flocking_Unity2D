using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Agents : MonoBehaviour
{
    public GameObject agent;

    public int numberOfAgents;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfAgents; i++) {
            Instantiate(agent, Random.insideUnitCircle * 10, Quaternion.identity);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking_Agent : MonoBehaviour
{

    public List<GameObject> localAgents = new List<GameObject>();

    void Start()
    {
        
    }

    void Update()
    {
        
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
}

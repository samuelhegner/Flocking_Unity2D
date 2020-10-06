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
        Camera cam =Camera.main;
        
        Vector2 maxCam = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, 0));
        Vector2 minCam = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));

        for (int i = 0; i < numberOfAgents; i++)
        {
            Instantiate(agent, new Vector2(Random.Range(minCam.x, maxCam.x), Random.Range(minCam.y, maxCam.y)), Quaternion.identity);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsCalculation : MonoBehaviour
{
    TextMeshProUGUI text;

    float fps;
    float ms;

    float deltaTime = 0.0f;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }


    // Update is called once per frame
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        fps = (int)(1f / Time.unscaledDeltaTime);

        ms = deltaTime * 1000.0f;

        text.text = "FPS: " + fps.ToString() + " Ms: " + ms.ToString("F2");
    }
}

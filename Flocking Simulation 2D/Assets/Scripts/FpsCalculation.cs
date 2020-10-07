using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsCalculation : MonoBehaviour
{
    TextMeshProUGUI text;

    float fps;
    float ms;

    float deltaTime = 0f;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }


    // Update is called once per frame
    void Update()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);

        text.text = "FPS: " + fps.ToString() + " Ms: " + ms;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SetValueFromSlider : MonoBehaviour
{
    [SerializeField] private ControlAgents agents;

    [SerializeField] private Slider slider;

    private void Start()
    {
        slider.value = agents.NumberOfAgentsToSpawn;
    }

    public void ChangeValue(float newValue) {
        agents.NumberOfAgentsToSpawn = (int)newValue;
    }
}

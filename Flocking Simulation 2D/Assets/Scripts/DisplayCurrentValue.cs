using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DisplayCurrentValue : MonoBehaviour
{
    Slider slider;

    TextMeshProUGUI text;

    public bool intDisplay;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        slider = GetComponentInChildren<Slider>();
        if (intDisplay)
        {
            text.text = text.transform.name + ": " + slider.value.ToString();
        }
        else
        {
            text.text = text.transform.name + ": " + slider.value.ToString("F2");
        }
    }

    public void AddCurrentValueToText()
    {
        if (intDisplay)
        {
            text.text = text.transform.name + ": " + slider.value.ToString();
        }
        else
        {
            text.text = text.transform.name + ": " + slider.value.ToString("F2");
        }
    }
}

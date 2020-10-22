using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ControlAgents))]
public class ControlAgentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ControlAgents controlAgents = (ControlAgents)target;


        //base.OnInspectorGUI();

        EditorGUILayout.LabelField("Simulation Options", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        controlAgents.NumberOfAgentsToSpawn = EditorGUILayout.IntSlider("Number of Agents: ", Mathf.RoundToInt(controlAgents.NumberOfAgentsToSpawn), 0, 5000);
        controlAgents.AdjustmentMultiplier = EditorGUILayout.Slider("Simulation Speed Modifier: ", controlAgents.AdjustmentMultiplier, 0, 100f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Agent Options", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Agent Force Options");

        controlAgents.AgentSeparationForce = EditorGUILayout.Slider("Separation Force: ", controlAgents.AgentSeparationForce, 0, 2);
        controlAgents.AgentAlignmentForce = EditorGUILayout.Slider("Alignment Force: ", controlAgents.AgentAlignmentForce, 0, 2);
        controlAgents.AgentCohesionForce = EditorGUILayout.Slider("Cohesion Force: ", controlAgents.AgentCohesionForce, 0, 2);
        controlAgents.AgentMaxForce = EditorGUILayout.Slider("Maximum Force: ", controlAgents.AgentMaxForce, 0, 2);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Agent Movement Options");
        controlAgents.AgentPerceptionRange = EditorGUILayout.Slider("Perception Range: ", controlAgents.AgentPerceptionRange, 0, 100);
        controlAgents.AgentMaxSpeed = EditorGUILayout.Slider("Maximum Speed: ", controlAgents.AgentMaxSpeed, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Agent Colour Options");
        controlAgents.ColourGradient = EditorGUILayout.GradientField("Agent Colour Range: ", controlAgents.ColourGradient);
        controlAgents.ColourLerpSpeed = EditorGUILayout.Slider("Colour Lerp Speed: ", controlAgents.ColourLerpSpeed, 0, 10);
        controlAgents.ColourMaxNeighbours = EditorGUILayout.Slider("Color Maximum Neighbours: ", controlAgents.ColourMaxNeighbours, 0, 500);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mouse Fleeing Options");
        controlAgents.FleeFromMouse = EditorGUILayout.Toggle("Flee from Mouse: ", controlAgents.FleeFromMouse);
    }
}


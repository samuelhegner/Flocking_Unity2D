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
        controlAgents.NumberOfAgentsToSpawn = EditorGUILayout.IntSlider("Number of Agents: ", controlAgents.NumberOfAgentsToSpawn, 0, 500);

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
        controlAgents.AgentMaxSpeed = EditorGUILayout.Slider("Maximum Speed: ", controlAgents.AgentMaxSpeed, 0, 10);
        controlAgents.UseVisionRadius = EditorGUILayout.Toggle("Use Vision Radius: ", controlAgents.UseVisionRadius);

        if (controlAgents.UseVisionRadius)
        {
            controlAgents.VisionRadiusAngle = EditorGUILayout.Slider("Vision Angle: ", controlAgents.VisionRadiusAngle, 0, 180);
        }

    }
}

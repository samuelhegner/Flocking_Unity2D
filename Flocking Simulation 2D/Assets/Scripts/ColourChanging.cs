using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourChanging : MonoBehaviour
{

    [SerializeField] private AgentMovement movement;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private float highestNumberOfNeighbours = 0;

    [SerializeField] private Gradient colourGradient;

    [SerializeField] private float colourLerpSpeed = 1f;


    float colourEvaluationAmount = 0;

    void Start()
    {
        
    }

    void Update()
    {
        if (movement.NeighbourCount > highestNumberOfNeighbours)
        {
            highestNumberOfNeighbours = movement.NeighbourCount;
        }

        if (movement.NeighbourCount > 0)
        {
            colourEvaluationAmount = movement.NeighbourCount / highestNumberOfNeighbours;
        }

        spriteRenderer.color = Color.Lerp(spriteRenderer.color, colourGradient.Evaluate(colourEvaluationAmount), Time.deltaTime * colourLerpSpeed);
    }
}
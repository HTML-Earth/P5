using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class IterationCounter : MonoBehaviour
{
    TextMeshProUGUI text;

    [SerializeField]
    RobotEnvironment environment;
    
    [SerializeField]
    RobotAgent robotAgent;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        int completions = robotAgent.GetTimesWon();
        int iterations = environment.GetIterations();
        
        text.text = "Current episode: " + iterations + "\nCompletions: " + completions + " / " + (iterations-1);
    }
}

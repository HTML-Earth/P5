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
        text.text = "Episode: 0    Completions: 0";
    }

    // Update is called once per frame
    void Update()
    {
        int completions = robotAgent.GetTimesWon();
        int iterations = environment.GetIterations();
        if (iterations > 1)
        {
            text.text = "Episodes: " + iterations + "\t Completions: " + completions;
        }
        else
        {
            text.text = "Episode: " + iterations + "\t Completions: " + completions;
        }
    }
}

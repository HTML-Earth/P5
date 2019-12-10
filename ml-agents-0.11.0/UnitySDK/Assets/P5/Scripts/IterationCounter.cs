using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class IterationCounter : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAcademy robotAcademy;
    RobotAgent robotAgent;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAcademy = FindObjectOfType<RobotAcademy>();
        robotAgent = FindObjectOfType<RobotAgent>();
        text.text = "Episode: 0    Completions: 0";
    }

    // Update is called once per frame
    void Update()
    {
        int completions = robotAgent.getTimesWon();
        int iterations = robotAcademy.GetIterations();
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

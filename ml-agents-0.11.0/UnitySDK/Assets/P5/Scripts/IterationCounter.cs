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

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAcademy = FindObjectOfType<RobotAcademy>();
        text.text = "iterations: 0";
    }

    // Update is called once per frame
    void Update()
    {
        int iterations = robotAcademy.GetIterations();
        text.text = "iterations: " + iterations;
    }
}

using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    private EnvironmentGeneration environmentGeneration;
    public override void InitializeAcademy()
    {
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
    }

    public override void AcademyStep()
    {
        // Check if goal is met?
    }

    public override void AcademyReset()
    {
        // Generate environment
        environmentGeneration.GenerateEnvironment();
    }
}

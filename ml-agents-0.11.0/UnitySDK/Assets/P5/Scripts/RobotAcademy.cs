using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    EnvironmentGeneration environmentGeneration;
    List<Transform> debrisInEnvironment;
    
    public override void InitializeAcademy()
    {
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
        debrisInEnvironment = new List<Transform>();
    }

    public override void AcademyStep()
    {
        // Check if goal is met?
    }

    public override void AcademyReset()
    {
        // Generate environment
        environmentGeneration.GenerateEnvironment();
        debrisInEnvironment = environmentGeneration.GetDebris();
    }

    public List<Transform> GetDebris()
    {
        return debrisInEnvironment;
    }
}

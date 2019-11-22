using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    EnvironmentGeneration environmentGeneration;
    DropZone dropZone;
    List<Transform> debrisInEnvironment;
    List<bool> debrisInZone;
    
    public override void InitializeAcademy()
    {
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
        dropZone = FindObjectOfType<DropZone>();
        debrisInEnvironment = new List<Transform>();
        debrisInZone = new List<bool>();
    }

    public override void AcademyReset()
    {
        // Generate environment
        environmentGeneration.GenerateEnvironment();
        debrisInEnvironment = environmentGeneration.GetDebris();
        dropZone.SetDebrisList(debrisInEnvironment);
        foreach (Transform debris in debrisInEnvironment)
        {
            debrisInZone.Add(dropZone.IsInZone(debris.position));
        }
    }

    public override void AcademyStep()
    {
        for (int debris = 0; debris < debrisInEnvironment.Count; debris++)
        {
            debrisInZone[debris] = dropZone.IsInZone(debrisInEnvironment[debris].position);
        }
    }

    public List<Transform> GetDebris()
    {
        return debrisInEnvironment;
    }

    public List<bool> GetDebrisInZone()
    {
        return debrisInZone;
    }
}

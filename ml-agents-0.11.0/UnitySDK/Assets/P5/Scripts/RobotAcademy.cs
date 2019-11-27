using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    EnvironmentGeneration environmentGeneration;
    DropZone dropZone;
    List<Transform> debrisInEnvironment;
    List<bool> previousDebrisInZone;
    List<bool> currentDebrisInZone;
    
    public override void InitializeAcademy()
    {
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
        dropZone = FindObjectOfType<DropZone>();
        debrisInEnvironment = new List<Transform>();
        currentDebrisInZone = new List<bool>();
        previousDebrisInZone = new List<bool>();
    }

    public override void AcademyReset()
    {
        // Generate environment
        environmentGeneration.GenerateEnvironment();
        
        // Get debris in environment
        debrisInEnvironment = environmentGeneration.GetDebris();

        // Update DropZone's debris list
        dropZone.SetDebrisList(debrisInEnvironment);
        
        // Initialize debrisInZone values
        foreach (Transform debris in debrisInEnvironment)
        {
            currentDebrisInZone.Add(dropZone.IsInZone(debris.position));
        }
        previousDebrisInZone = currentDebrisInZone;
    }

    public override void AcademyStep()
    {
        // Copy currentDebrisInZone values to previousDebrisInZone
        previousDebrisInZone = new List<bool>();
        foreach (bool b in currentDebrisInZone)
        {
            previousDebrisInZone.Add(b);
        }

        // Update currentDebrisInZone values
        for (int debris = 0; debris < debrisInEnvironment.Count; debris++)
        {
            currentDebrisInZone[debris] = dropZone.IsInZone(debrisInEnvironment[debris].position);
        }
    }

    public List<Transform> GetDebris()
    {
        return debrisInEnvironment;
    }

    public List<bool> GetCurrentDebrisInZone()
    {
        return currentDebrisInZone;
    }

    public List<bool> GetPreviousDebrisInZone()
    {
        return previousDebrisInZone;
    }
}

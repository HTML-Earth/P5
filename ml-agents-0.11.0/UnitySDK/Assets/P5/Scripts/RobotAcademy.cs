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
    RobotAgent agent;
    private float timePenalty = -0.1f;
    
    public enum CommunicatorPort
    {
        DefaultTraining = 5004,
        OurPythonScript = 5005
    }

    public CommunicatorPort communicatorPort = CommunicatorPort.OurPythonScript;
    
    public override void InitializeCommunicator()
    {
        Communicator = new RpcCommunicator(
            new CommunicatorInitParameters
            {
                port = (int)communicatorPort
            });
    }
    
    public override void InitializeAcademy()
    {
        Debug.LogWarning("Communicating on port:" + (int)communicatorPort + " (" + communicatorPort + ")");
        
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
        dropZone = FindObjectOfType<DropZone>();
        debrisInEnvironment = new List<Transform>();
        currentDebrisInZone = new List<bool>();
        previousDebrisInZone = new List<bool>();
        agent = FindObjectOfType<RobotAgent>();
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
        
        // For every step deduct rewards (roughly 5*second at normal speed)
        agent.AddReward(timePenalty, "Time passed");
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

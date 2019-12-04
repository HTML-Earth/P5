using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    EnvironmentGeneration environmentGeneration;
    
    DropZone dropZone;
    List<Debris> debrisInEnvironment;
    
    List<bool> previousDebrisInZone;
    List<bool> currentDebrisInZone;

    int iterations;

    public enum CommunicatorPort
    {
        DefaultTraining = 5004,
        OurPythonScript = 5005
    }

    [Header("Discombobulated settings")]
    public CommunicatorPort communicatorPort = CommunicatorPort.OurPythonScript;
    public bool useRandomEnvironment;

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
        Debug.LogWarning("Communicator port is set to:" + (int)communicatorPort + " (" + communicatorPort + ")");
        
        // Initialize variables
        environmentGeneration = FindObjectOfType<EnvironmentGeneration>();
        dropZone = FindObjectOfType<DropZone>();

        InitializeDebrisList();
        
        currentDebrisInZone = new List<bool>();
        previousDebrisInZone = new List<bool>();
    }

    void InitializeDebrisList()
    {
        debrisInEnvironment = new List<Debris>();
        
        // Add all debris in environment to the list
        foreach (Debris debris in FindObjectsOfType<Debris>())
        {
            debrisInEnvironment.Add(debris);
        }
    }

    public override void AcademyReset()
    {
        if (useRandomEnvironment)
        {
            // Generate environment
            environmentGeneration.GenerateEnvironment();

            // If environmentGeneration resets debris, reinitialize the list
            if (environmentGeneration.createNewDebris)
                InitializeDebrisList();
        }
        else // If random generation is disabled
        {
            // Reset debris positions
            foreach (Debris debris in debrisInEnvironment)
            {
                debris.transform.position = debris.GetStartPosition();
            }
            
            // Reset robot position
            FindObjectOfType<RobotAgent>()?.ResetPosition();
        }

        // Update DropZone's debris list
        dropZone.SetDebrisList(debrisInEnvironment);
        
        // Initialize debrisInZone values
        foreach (Debris debris in debrisInEnvironment)
        {
            currentDebrisInZone.Add(dropZone.IsInZone(debris.transform.position));
        }
        previousDebrisInZone = currentDebrisInZone;

        iterations++;
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
            currentDebrisInZone[debris] = dropZone.IsInZone(debrisInEnvironment[debris].transform.position);
        }
    }
    
    public int GetIterations()
    {
        return iterations;
    }

    public List<Debris> GetDebris()
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

    public void ResetDebrisInZone()
    {
        for (int debris = 0; debris < debrisInEnvironment.Count; debris++)
        {
            currentDebrisInZone[debris] = false;
            previousDebrisInZone[debris] = false;
        }
    }
}

using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAcademy : Academy
{
    bool initialized = false;
    
    public enum CommunicatorPort
    {
        DefaultTraining = 5004,
        OurPythonScript = 5005
    }

    RobotEnvironment[] robotEnvironments;

    [Header("Discombobulated settings")]
    public CommunicatorPort communicatorPort = CommunicatorPort.OurPythonScript;
    
    public override void InitializeCommunicator()
    {
        Communicator = new RpcCommunicator(
            new CommunicatorInitParameters
            {
                port = (int)communicatorPort
            });
        
        Debug.LogWarning("Communicator port is set to:" + (int)communicatorPort + " (" + communicatorPort + ")");
    }
    
    public override void InitializeAcademy()
    {
        if (initialized)
            return;

        initialized = true;
        
        robotEnvironments = FindObjectsOfType<RobotEnvironment>();
    }

    public override void AcademyReset()
    {
    }

    public override void AcademyStep()
    {
        foreach (RobotEnvironment robotEnvironment in robotEnvironments)
        {
            robotEnvironment.StepEnvironment();
        }
    }
}

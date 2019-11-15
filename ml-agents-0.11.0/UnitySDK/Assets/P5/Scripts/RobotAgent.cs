using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RobotAgent : Agent
{
    public GameObject debris;
    WheelDrive wheels;
    ShovelControl shovel;
    
    public override void InitializeAgent()
    {
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
    }

    public override void CollectObservations()
    {
        // Robot position
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.z);
        
        // Debris position
        AddVectorObs(debris.transform.position.x);
        AddVectorObs(debris.transform.position.z);
        
        // Arm and shovel position
        AddVectorObs(shovel.GetArmPos());
        AddVectorObs(shovel.GetShovelPos());
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        wheels.SetTorque(vectorAction[0]);
        wheels.SetAngle(vectorAction[1]);
        
        shovel.RotateArm(vectorAction[2]);
        shovel.RotateShovel(vectorAction[3]);
    }

    public override void AgentReset()
    {
        base.AgentReset();
    }

    public override float[] Heuristic()
    {
        float[] heuristicValues = new float[4];

        heuristicValues[0] = Input.GetAxis("Vertical");
        heuristicValues[1] = Input.GetAxis("Horizontal");

        heuristicValues[2] = (Input.GetKey(KeyCode.Q)) ? 1f : (Input.GetKey(KeyCode.E)) ? -1f : 0f;
        heuristicValues[3] = (Input.GetKey(KeyCode.Z)) ? 1f : (Input.GetKey(KeyCode.X)) ? -1f : 0f;

        return heuristicValues;
    }
}

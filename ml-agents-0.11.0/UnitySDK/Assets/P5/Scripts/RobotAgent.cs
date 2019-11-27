using System;
using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEditor;
using UnityEngine;

public class RobotAgent : Agent
{
    RobotAcademy academy;
    WheelDrive wheels;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;

    float timeElapsed;

    float reward_debrisEnteredZone = 10f;
    float reward_allDebrisEnteredZone = 1000f;
    
    float penalty_debrisLeftZone = -100f;

    bool goalReached = false;

    List<RobotVision.DebrisInfo> debrisInfos;
    
    readonly Color debrisHighlight = new Color(0,1,1);
    readonly Color debrisHighlightMissing = new Color(1,0,0);
    
    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RobotAcademy>();
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();

        timeElapsed = 0;
    }
    
    void FixedUpdate() {
        timeElapsed += Time.fixedDeltaTime;
    }

    public override void CollectObservations()
    {
        // Robot position (0, 1)
        Vector3 currentPosition = transform.position;
        AddVectorObs(currentPosition.x);
        AddVectorObs(currentPosition.z);

        // Arm and shovel position (2, 3)
        AddVectorObs(shovel.GetArmPos());
        AddVectorObs(shovel.GetShovelPos());
        
        // Drop-Zone Position and radius (4, 5, 6)
        Vector3 dropZonePosition = dropZone.transform.position;
        AddVectorObs(dropZonePosition.x);
        AddVectorObs(dropZonePosition.z);
        AddVectorObs(dropZone.GetRadius());
        
        // Distance sensor measurements
        float[] distances = sensors.GetMeasuredDistances();
        for (int dist = 0; dist < distances.Length; dist++)
        {
            AddVectorObs(distances[dist]);
        }
        
        // Debris positions
        debrisInfos = vision.UpdateVision();
        
        foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
        {
            AddVectorObs(debrisInfo.lastKnownPosition.x);
            AddVectorObs(debrisInfo.lastKnownPosition.y);
            AddVectorObs(debrisInfo.lastKnownPosition.z);
        }
        
        // Simulation time
        AddVectorObs(timeElapsed);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Check if debris has left/entered the zone
        List<bool> previousDebrisInZone = academy.GetPreviousDebrisInZone();
        List<bool> currentDebrisInZone = academy.GetCurrentDebrisInZone();

        for (int i = 0; i < previousDebrisInZone.Count; i++)
        {
            if (previousDebrisInZone[i])
            {
                if (!currentDebrisInZone[i])
                    AddReward(penalty_debrisLeftZone, "debris left zone");
            }
            else
            {
                if (currentDebrisInZone[i])
                    AddReward(reward_debrisEnteredZone, "debris entered zone");
            }
        }

        // Check if goal is met
        if (!goalReached && dropZone.IsAllDebrisInZone())
        {
            goalReached = true;
            AddReward(reward_allDebrisEnteredZone, "all debris in zone");
            Done();
        }

        // Perform actions
        wheels.SetTorque(vectorAction[0]);
        wheels.SetAngle(vectorAction[1]);
        
        shovel.RotateArm(vectorAction[2]);
        shovel.RotateShovel(vectorAction[3]);
    }

    public override void AgentReset()
    {
        base.AgentReset();
    }

    // Wrapper function for AddReward that prints the reward/penalty and custom message in console
    void AddReward(float reward, string message)
    {
        Debug.Log(((reward < 0) ? "Penalty: " : "Reward: ") + reward + " (" + message + ")");
        AddReward(reward);
    }

    // Used to control the agent manually
    public override float[] Heuristic()
    {
        float[] heuristicValues = new float[4];

        heuristicValues[0] = Input.GetAxis("Vertical");
        heuristicValues[1] = Input.GetAxis("Horizontal");

        heuristicValues[2] = (Input.GetKey(KeyCode.Q)) ? 1f : (Input.GetKey(KeyCode.E)) ? -1f : 0f;
        heuristicValues[3] = (Input.GetKey(KeyCode.Z)) ? 1f : (Input.GetKey(KeyCode.X)) ? -1f : 0f;

        return heuristicValues;
    }

    // Used to draw debug info on screen
    void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            return;

        // Draws rings around the last known debris positions
        foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
        {
            Handles.color = debrisInfo.isVisible ? debrisHighlight : debrisHighlightMissing;
            Handles.DrawWireDisc(debrisInfo.lastKnownPosition, Vector3.up, 0.5f);
        }
    }
}

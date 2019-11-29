using System;
using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEditor;
using UnityEngine;

public class RobotAgent : Agent
{
    RobotAcademy academy;
    Rigidbody rigidbody;
    WheelDrive wheels;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;

    float timeElapsed;

    // Positive rewards
    float reward_debrisEnteredZone = 10f;
    float reward_allDebrisEnteredZone = 1000f;
    private float reward_debrisFound = 1f;
    // TODO: Implement these
    // private float reward_debrisInShovel = 5f;
    // private float reward_moveTowardsDebris = 0.1f;
    // private float reward_debrisInShovelAndMoveTowardsZone = 0.2f;
    
    // Negative rewards
    float penalty_debrisLeftZone = -100f;
    // TODO: Implement these
    // private float penalty_hitWall = -5f;
    // private float penalty_timePassed = -0.5f;
    // private float penalty_debrisRunOver = -5f;
    

    // Variables used to check for rewards
    bool goalReached = false;
    // Hard initialized to 6*false, a false for each debris
    private List<bool> listIsDebrisLocated;

    List<RobotVision.DebrisInfo> debrisInfos;
    
    readonly Color debrisHighlight = new Color(0,1,1);
    readonly Color debrisHighlightMissing = new Color(1,0,0);
    
    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RobotAcademy>();
        rigidbody = GetComponent<Rigidbody>();
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();
        listIsDebrisLocated = new List<bool>();
        
        foreach (var debrisInfo in debrisInfos)
        {
            listIsDebrisLocated.Add(false);
        }

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
        
        // Robot rotation (2)
        AddVectorObs(transform.rotation.eulerAngles.y);
        
        // Robot velocity (3, 4, 5)
        Vector3 localVelocity = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.y);
        AddVectorObs(localVelocity.z);

        // Arm and shovel position (6, 7)
        AddVectorObs(shovel.GetArmPos());
        AddVectorObs(shovel.GetShovelPos());
        
        // Drop-Zone Position and radius (8, 9, 10)
        Vector3 dropZonePosition = dropZone.transform.position;
        AddVectorObs(dropZonePosition.x);
        AddVectorObs(dropZonePosition.z);
        AddVectorObs(dropZone.GetRadius());

        // Distance sensor measurements (11 - 40)
        float[] distances = sensors.GetMeasuredDistances();
        for (int dist = 0; dist < distances.Length; dist++)
        {
            AddVectorObs(distances[dist]);
        }
        
        // Debris positions (41 - 58)
        debrisInfos = vision.UpdateVision();
        
        foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
        {
            AddVectorObs(debrisInfo.lastKnownPosition.x);
            AddVectorObs(debrisInfo.lastKnownPosition.y);
            AddVectorObs(debrisInfo.lastKnownPosition.z);
        }
        
        // Simulation time (59)
        AddVectorObs(timeElapsed);
        
        // features:
        
        //Check if robot is within dropZone (60)
        bool isInDropZone = dropZone.IsInZone(transform.position);
        AddVectorObs(isInDropZone);
        
        //Check if robot is getting closer to debris (61 -> 66)
        foreach (var debrisInfo in debrisInfos)
        {
            bool gettingCloserToDebris = debrisInfo.distanceFromRobot < debrisInfo.lastDistanceFromRobot;
            AddVectorObs(gettingCloserToDebris);
        }
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
            Done("goal reached (all debris in zone)");
        }
        
        // Check for each debris if it is visible and has not been seen before
        for (int debrisNum = 0; debrisNum < debrisInfos.Count; debrisNum++)
        {
            if (debrisInfos[debrisNum].isVisible && !listIsDebrisLocated[debrisNum])
            {
                AddReward(reward_debrisFound, "Debris was located");
                listIsDebrisLocated[debrisNum] = true;
            }
        }
        
        // Check if robot has fallen
        if (Vector3.Dot(transform.up, Vector3.up) < 0.1f)
        {
            Done("robot has fallen (probably)");
        }
        
        // Check if robot is out of bounds
        Vector3 robotPosition = transform.position;
        if (robotPosition.x > 25f || robotPosition.x < -25f || robotPosition.z > 25f || robotPosition.z < -25f || robotPosition.y < -5f)
            Done("robot is out of bounds");

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
    
    // Wrapper function for Done that prints a custom done message in console
    void Done(string reason)
    {
        Debug.Log("Done! reason: " + reason);
        Done();
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

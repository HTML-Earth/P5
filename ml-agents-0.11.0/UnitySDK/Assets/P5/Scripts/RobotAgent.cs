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
    
    Color debrisHighlight = new Color(0,1,1);
    Color debrisHighlightMissing = new Color(1,0,0);
    
    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RobotAcademy>();
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();
    }

    public override void CollectObservations()
    {
        // Robot position (0, 1)
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.z);

        // Arm and shovel position (2, 3)
        AddVectorObs(shovel.GetArmPos());
        AddVectorObs(shovel.GetShovelPos());
        
        // Drop-Zone Position and radius (4, 5, 6)
        AddVectorObs(dropZone.transform.position.x);
        AddVectorObs(dropZone.transform.position.z);
        AddVectorObs(dropZone.GetRadius());
        
        // Distance sensor measurements
        float[] distances = sensors.GetMeasuredDistances();
        for (int dist = 0; dist < distances.Length; dist++)
        {
            AddVectorObs(distances[dist]);
        }
        
        //Update debris vision
        vision.UpdateVision();

        // Debris visibility
        AddVectorObs(64); //TODO: replace with bit-shifting (64 means all 6 debris are visible)
        
        // Debris positions
        Vector3[] knownPositions = vision.GetKnownPositions();
        for (int i = 0; i < knownPositions.Length; i++)
        {
            AddVectorObs(knownPositions[i].x);
            AddVectorObs(knownPositions[i].y);
            AddVectorObs(knownPositions[i].z);
        }
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

    void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            return;

        Vector3[] knownPositions = vision.GetKnownPositions();
        bool[] visibility = vision.GetVisibiilty();

        for (int i = 0; i < knownPositions.Length; i++)
        {
            if (visibility[i])
                Handles.color = debrisHighlight;
            else
                Handles.color = debrisHighlightMissing;

            Handles.DrawWireDisc(knownPositions[i], Vector3.up, 0.5f);
        }
    }
}

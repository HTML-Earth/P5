using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class RobotAgent : Agent
{
    Rigidbody rb;
    WheelDrive wheels;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;
    DisplayRewards displayRewards;
    //public Transform debris;

    [SerializeField]
    RobotEnvironment environment;
    
    [SerializeField]
    DebrisDetector debrisInShovel;

    [SerializeField]
    DebrisDetector debrisInFront;
    
    List<bool> currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    
    List<bool> currentDebrisInFront = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInFront = new List<bool>() {false, false, false, false, false, false};

    List<bool> lastHundredAttempts = new List<bool>();

    Vector3 startPosition;
    Quaternion startRotation;
    
    public Transform Debris;
    public Transform DropZone;
    
    Vector3 rbPosition;
    Vector3 debrisPosition;
    Vector3 dropZonePosition;
    
    
    const float TimeLimit = 90f;

    bool doneHasBeenCalled = false;
    
    float timeElapsed;
    int timesWon = 0;
    int timesDone = 0;
    
    bool isAddedToSuccessRateList = false;
    
    // Variables used to check for rewards
    List<bool> listIsDebrisLocated;
    Queue<float> wallRammingPenalties;

    List<RobotVision.DebrisInfo> debrisInfos;

    const float MinimumDistanceBeforeCheck = 0.5f;
    Vector3 lastCheckedPosition;
    bool checkedPositionThisStep;
    
    float previousDistanceFromZone;
    float currentDistanceFromZone;

    const int DebrisCount = 6;

    readonly Color debrisHighlight = new Color(0,1,1);
    readonly Color debrisHighlightMissing = new Color(1,0,0);

    // Current state of action vector
    float[] actionVector;

    List<string> observationNames = new List<string>();
    bool observationsLogged = false;
    
    public override void InitializeAgent()
    {
        environment.InitializeEnvironment();

        rb = GetComponent<Rigidbody>();
        
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        
        vision = GetComponent<RobotVision>();
        vision.InitializeDebrisArray(environment);
        
        sensors = GetComponent<RobotSensors>();
        
        dropZone = environment.GetDropZone();
        
        displayRewards = FindObjectOfType<DisplayRewards>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;

        lastCheckedPosition = startPosition;
        //debrisPosition = debris.position - environment.transform.position;
        //AgentReset();
    }

    public override void AgentReset()
    {
        // TODO: Counts episodes up, Method is called when agent is done and when python calls for the simulation to be reset, also since this is called in InitializeAgent() it starts at not 0
        environment.ResetEnvironment();
        
        debrisInShovel.InitializeDetector();
        debrisInFront.InitializeDetector();
        
        // Hard initialized to 6*false, one for each debris
        listIsDebrisLocated = new List<bool>() {false, false, false, false, false, false};
        
        wallRammingPenalties = new Queue<float>();

        lastCheckedPosition = transform.position;

        currentDistanceFromZone = Vector3.Distance(transform.position, dropZone.transform.position);
        previousDistanceFromZone = currentDistanceFromZone;
        
        shovel.ResetRotations();

        doneHasBeenCalled = false;

        timeElapsed = 0;
    }
    
    // Called by academy on reset if random generation is disabled
    public void ResetPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        lastCheckedPosition = startPosition;
        
        // Reset robot velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    void FixedUpdate() {
        timeElapsed += Time.fixedDeltaTime;
    }

    public override void CollectObservations()
    {
        // Positions
        //debrisPosition = debris.position - environment.transform.position;
        dropZonePosition = dropZone.transform.position - environment.transform.position;
        
        // Robot position in each environment
        AddVectorObs((this.transform.position - environment.transform.position).x, "robot_position_x");
        AddVectorObs((this.transform.position - environment.transform.position).y, "robot_position_y");
        AddVectorObs((this.transform.position - environment.transform.position).z, "robot_position_z");

        // Robot velocity
        AddVectorObs(rb.velocity.x, "robot_velocity_x");
        AddVectorObs(rb.velocity.z, "robot_velocity_z");
        
        // Debris and DropZone position in each environment
        AddVectorObs((Debris.position - environment.transform.position).x, "debris_position_x");
        AddVectorObs((Debris.position - environment.transform.position).y, "debris_position_y");
        AddVectorObs((Debris.position - environment.transform.position).z, "debris_position_z");
        AddVectorObs((DropZone.position - environment.transform.position).x, "dropzone_position_x");
        AddVectorObs((DropZone.position - environment.transform.position).y, "dropzone_position_y");
        AddVectorObs((DropZone.position - environment.transform.position).z, "dropzone_position_z");

        AddVectorObs(timesWon, "times_won");

#if UNITY_EDITOR
        // AUTO GENERATE RobotObservations.py
        if (!observationsLogged)
        {
            DirectoryInfo mlAgentsRootDir = new DirectoryInfo(Application.dataPath).Parent.Parent;

            string filePath = Path.Combine(mlAgentsRootDir.ToString(), "project/RobotObservations.py");

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("# This file was generated by RobotAgent.cs");
                stringBuilder.AppendLine("from enum import IntEnum");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("");
                stringBuilder.AppendLine("class RobotObservations(IntEnum):");
                stringBuilder.AppendLine("    # Accessible by self.obs.'field_name'");

                for (int i = 0; i < observationNames.Count; i++)
                {
                    stringBuilder.AppendLine("    " + observationNames[i] + " = " + i);
                }

                sw.Write(stringBuilder.ToString());
            }
        }

        observationsLogged = true;
#endif
    }

    void AddVectorObs(float observation, string observationName)
    {
        AddVectorObs(observation);

        if (observationsLogged)
            return;

        observationNames.Add(observationName);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (doneHasBeenCalled)
            return;
        
        float movement =  ConvertAction((int)vectorAction[0]);
        float wheelAngle = ConvertAction((int)vectorAction[1]);

        // Perform actions
        wheels.SetTorque(movement);
        wheels.SetAngle(wheelAngle);
    
        // Store current action vector state for visualization
        actionVector = vectorAction;
        
        // if robot has moved enough to do a distance check, set checked bool to true
        checkedPositionThisStep = Vector3.Distance(lastCheckedPosition, transform.position) > MinimumDistanceBeforeCheck;
        
        //Evaluation Methods:
        CreateListWithSuccessRate();
    
    
        float distanceToTarget = Vector3.Distance(Debris.position, DropZone.position);
        
        // Reached target
        if (distanceToTarget < 5f)
        {
            SetReward(1.0f);
            timesWon++;
            Done("Debris in zone");
        }
        
        // Reset if time limit is reached
        if (timeElapsed > TimeLimit)
        {
            timeElapsed = 0;
            lastHundredAttempts.Add(false);
            Done("Time limit reached");
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.layer == LayerMask.NameToLayer("environment"))
        {
            Done("Hit wall");
        }
    }

    // Wrapper function for Done that prints a custom done message in console
    void Done(string reason)
    {
        timesDone++;
        
        doneHasBeenCalled = true;
        
        Debug.Log("Done! reason: " + reason);
        Done();

        if (lastHundredAttempts.Count >= 100)
        {
            lastHundredAttempts.RemoveAt(0);
        }

        // Reset shovel content on restart
        currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    }

    // Wrapper function for AddReward that prints the reward/penalty and custom message in console
    public void AddReward(float reward, string message)
    {
        Debug.Log(((reward < 0) ? "Penalty: " : "Reward: ") + reward + " (" + message + ")");
        AddReward(reward);
    }

    // Wrapper function for AddReward that displays the reward/penalty and custom message on the canvas
    public void AddReward(float reward, string message, Vector3 position)
    {
        if (displayRewards != null)
            displayRewards.DisplayReward(reward, message, position);
        
        AddReward(reward);
    }

    // Used to control the agent manually
    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = ConvertHeuristic((int)Input.GetAxis("Vertical"));
        action[1] = ConvertHeuristic((int)Input.GetAxis("Horizontal"));
        return action;
    }

    //Keep track of success rate every 100 attempts
    public void CreateListWithSuccessRate()
    {
        List<float> successRateList = new List<float>();
        float successfullAttempts = 0.0f;
        float successRate;


        if (timesDone % 5 == 0 && timesDone > 0)
        {
            foreach (var attempt in lastHundredAttempts)
            {
                if (attempt == true)
                {
                    successfullAttempts++;
                }
            
            }

            successRate = successfullAttempts / 5;
            {
                if (!isAddedToSuccessRateList)
                {
                    isAddedToSuccessRateList = true;
                    successRateList.Add(successRate);
                }
            }
        }
    }

    public float GetElapsedTime()
    {
        return timeElapsed;
    }

    public float[] GetActionVector()
    {
        return actionVector;
    }

    public List<bool> GetLastHundredAttemptsList()
    {
        return lastHundredAttempts;
    }

    public int GetTimesWon()
    {
        return timesWon;
    }

    // 0,1,2 --> -1,0,1
    public int ConvertAction(int action)
    {
        switch (action)
        {
            case 0:
                return -1;
            case 1:
                return 0;
            case 2:
                return 1;
        }

        return action;
    }
    
    public int ConvertHeuristic(int getAxis)
    {
        switch (getAxis)
        {
            case 0:
                return 1;
            case 1:
                return 2;
            case -1:
                return 0;
        }

        return getAxis;
    }

#if UNITY_EDITOR
    // Used to draw debug info on screen
    //void OnDrawGizmos()
    //{
    //    if (!EditorApplication.isPlaying)
    //        return;
    //
    //    if (debrisInfos == null)
    //        return;
    //    
    //    // Draws rings around the last known debris positions
    //    foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
    //    {
    //        Handles.color = debrisInfo.isVisible ? debrisHighlight : debrisHighlightMissing;
    //        Handles.DrawWireDisc(debrisInfo.lastKnownPosition, Vector3.up, 0.5f);
    //    }
    //    
    //    Handles.color = Color.red;
    //    Handles.DrawLine(transform.position, transform.position + rb.velocity);
    //}
#endif
}

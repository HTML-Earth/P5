using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class RobotAgent : Agent
{
    public enum RobotGoal
    {
        AllDebrisInDropZone,
        RobotInDropZone
    };
    
    Rigidbody rb;
    WheelDrive wheels;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;
    DisplayRewards displayRewards;
    //public Transform debris;
    
    [Header("Goal")]
    public RobotGoal goal;

    [Header("Robot")]
    public bool useSimplePhysics;

    [Header("References")]
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

    public Transform debris;
    
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

    float highestDotProduct;

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
    
    public override void InitializeAgent()
    {
        environment.InitializeEnvironment();

        rb = GetComponent<Rigidbody>();
        
        wheels = GetComponent<WheelDrive>();
        if (useSimplePhysics)
        {
            Destroy(wheels);
            WheelCollider[] wheelColliders = FindObjectsOfType<WheelCollider>();
            foreach (WheelCollider wheelCollider in wheelColliders)
            {
                Destroy(wheelCollider);
            }
        }
                
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

        highestDotProduct = 0.4f;

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
        if (debris != null)
            debrisPosition = transform.InverseTransformPoint(debris.position);
        else
            debrisPosition = Vector3.zero;
        
        dropZonePosition = transform.InverseTransformPoint(dropZone.transform.position);
        
        // Robot position in each environment
        AddVectorObs(this.transform.position - environment.transform.position);

        // Robot velocity
        AddVectorObs(rb.velocity.x);
        AddVectorObs(rb.velocity.normalized.z);

        // Debris and DropZone position in each environment
        AddVectorObs(debrisPosition);
        AddVectorObs(dropZonePosition);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (doneHasBeenCalled)
            return;

        int action = (int)vectorAction[0];

        // Perform actions
        if (useSimplePhysics)
            ControlRobot(GetThrottleFromAction(action), GetAngleFromAction(action));
        else
        {
            wheels.SetTorque(GetThrottleFromAction(action));
            wheels.SetAngle(GetAngleFromAction(action));
        }
        
        // Store current action vector state for visualization
        actionVector = vectorAction;
        
        // if robot has moved enough to do a distance check, set checked bool to true
        checkedPositionThisStep = Vector3.Distance(lastCheckedPosition, transform.position) > MinimumDistanceBeforeCheck;
        
        //Evaluation Methods:
        CreateListWithSuccessRate();

        if (debris != null)
        {
            Vector3 robotToDebris = debris.position - transform.position;

            float fwdDotDebris = Vector3.Dot(transform.forward, robotToDebris.normalized);

            if (fwdDotDebris > highestDotProduct)
            {
                highestDotProduct = fwdDotDebris;
                AddReward(0.01f);
            }
        }

        switch (goal)
        {
            case RobotGoal.AllDebrisInDropZone:
            {
                float debrisDistanceToDropZone = Vector3.Distance(debris.position,dropZone.transform.position);
        
                // Reached target
                if (debrisDistanceToDropZone < 5f)
                {
                    SetReward(1.0f);
                    Done("Debris in zone");
                }

                break;
            }

            case RobotGoal.RobotInDropZone:
            {
                float robotDistanceToDropZone = Vector3.Distance(transform.position,dropZone.transform.position);
        
                // Reached target
                if (robotDistanceToDropZone < 5f)
                {
                    SetReward(1.0f);
                    Done("Robot in zone");
                }
                
                break;
            }
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
            AddReward(-1.0f);
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
    
    // Simple robot physics
    void ControlRobot(int drive, int rotate)
    {
        if (drive == 1)
            rb.MovePosition(rb.position + transform.forward * 0.1f);
        else if (drive == -1)
            rb.MovePosition(rb.position - transform.forward * 0.1f);
        
        if (rotate == 1)
            transform.Rotate(transform.up, 2f);
        else if (rotate == -1)
            transform.Rotate(transform.up, -2f);
    }

    // Used to control the agent manually
    public override float[] Heuristic()
    {
        var action = new float[1];

        int vert = (int) Input.GetAxis("Vertical");
        int horiz = (int) Input.GetAxis("Horizontal");

        if (vert == 1)
            action[0] = 1;
        else if (vert == -1)
            action[0] = 3;
        else if (horiz == 1)
            action[0] = 4;
        else if (horiz == -1)
            action[0] = 2;
        else
            action[0] = 0;
        
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

    public int GetThrottleFromAction(int action)
    {
        switch (action)
        {
            case 1:
                return 1;
            case 3:
                return -1;
        }

        return 0;
    }
    
    public int GetAngleFromAction(int action)
    {
        switch (action)
        {
            case 2:
                return -1;
            case 4:
                return 1;
        }

        return 0;
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

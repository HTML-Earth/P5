using System;
using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class RobotAgent : Agent
{
    RobotAcademy academy;
    Rigidbody rb;
    WheelDrive wheels;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;
    
    [FormerlySerializedAs("debrisDetector")] [SerializeField]
    DebrisDetector debrisInShovel;

    [SerializeField]
    DebrisDetector debrisInfront;
    
    List<bool> currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};

    Vector3 startPosition;
    Quaternion startRotation;
    
    readonly float timeLimit = 90f;
    
    float timeElapsed;

    // Positive rewards
    float reward_debrisEnteredShovel = 0.2f;
    float reward_debrisEnteredZone = 0.4f;
    float reward_allDebrisEnteredZone = 1f;
    float reward_debrisFound = 0.1f;

    float reward_moveTowardsDebris = 0.01f;
    float reward_moveTowardsZoneWithDebris = 0.2f;
    
    // Negative rewards
    float penalty_debrisLeftShovel = -0.2f;
    float penalty_debrisLeftZone = -1f;
    float penalty_moveAwayFromZoneWithDebris = -0.2f;

    float penalty_robotRammingWall = -0.5f;

    float penalty_time = -0.01f;
    private float penalty_robot_fall = -1f;
    

    // Variables used to check for rewards
    List<bool> listIsDebrisLocated;
    Queue<float> wallRammingPenalties;

    List<RobotVision.DebrisInfo> debrisInfos;

    float previousDistanceFromZone;
    float currentDistanceFromZone;

    readonly int debrisCount = 6;
    
    readonly Color debrisHighlight = new Color(0,1,1);
    readonly Color debrisHighlightMissing = new Color(1,0,0);

    // Current state of action vector
    float[] actionVector;
    
    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RobotAcademy>();
        rb = GetComponent<Rigidbody>();
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void AgentReset()
    {
        debrisInShovel.InitializeDetector();
        debrisInfront.InitializeDetector();
        
        // Hard initialized to 6*false, one for each debris
        listIsDebrisLocated = new List<bool>() {false, false, false, false, false, false};
        
        wallRammingPenalties = new Queue<float>();

        currentDistanceFromZone = Vector3.Distance(transform.position, dropZone.transform.position);
        previousDistanceFromZone = currentDistanceFromZone;
        
        shovel.ResetRotations();

        timeElapsed = 0;
    }
    
    // Called by academy on reset if random generation is disabled
    public void ResetPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        // Reset robot velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
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

        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < debrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(Mathf.Infinity);
            AddVectorObs(Mathf.Infinity);
            AddVectorObs(Mathf.Infinity);
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

        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < debrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(false);
        }

        //Check if robot has picked up debris (67)
        List<bool> debrisInShovelList = this.debrisInShovel.GetDebrisInArea();
        bool debrisInShovel = false;
        foreach (var value in debrisInShovelList)
        {
            if (value)
            {
                debrisInShovel = true;
            }
        }

        AddVectorObs(debrisInShovel);

        //Check if debris infront of shovel (68)
        List<bool> debrisInfrontList = this.debrisInfront.GetDebrisInArea();
        bool debrisIsInfront = false;
        foreach (var value in debrisInfrontList)
        {
            if (value)
            {
                debrisIsInfront = true;
            }
        }

        AddVectorObs(debrisIsInfront);

        // Check if robot is rotated towards a debris (69) //TODO Does not take walls into account
        int counter = 0;
        bool pointingTowardDebris = false;
        foreach (var debris in debrisInfos)
        {
            // Check it is not in dropzone
            if (!dropZone.IsInZone(debris.transform.position) && !debrisInShovelList[counter])
            {
                float dot = Vector3.Dot(transform.forward, (debris.transform.position - transform.position).normalized);
                if (dot > 0.99f)
                {
                    pointingTowardDebris = true;
                }
            }
            counter++;
        }
        AddVectorObs(pointingTowardDebris);

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Perform actions
        wheels.SetTorque(vectorAction[0]);
        wheels.SetAngle(vectorAction[1]);
        
        shovel.RotateArm(vectorAction[2]);
        shovel.RotateShovel(vectorAction[3]);
        
        // Store current action vector state for visualization
        actionVector = vectorAction;
        
        // Give rewards or penalties
        RewardDebrisInShovel();
        RewardDebrisInOutZone();
        RewardMoveTowardsDebris();
        RewardMoveTowardsZoneWithDebris();
        RewardLocateDebris();
        PenaltyTime();
        PenaltyForHittingWalls();

        // Check if goal is met and simulation is done
        IsGoalMet();

        // Make sure robot in inside area and upright
        RobotUpright();
        
        // Reset if time limit is reached
        if (timeElapsed > timeLimit)
        {
            timeElapsed = 0;
            Done("Time limit reached");
        }
        
    }

    // Reward given for the first time each debris is seen
    void RewardLocateDebris()
    {
        // Check for each debris if it is visible and has not been seen before
        for (int debrisNum = 0; debrisNum < debrisInfos.Count; debrisNum++)
        {
            if (debrisInfos[debrisNum].isVisible && !listIsDebrisLocated[debrisNum])
            {
                AddReward(reward_debrisFound, "Debris was located");
                listIsDebrisLocated[debrisNum] = true;
            }
        }
    }

    // Reward for each time the agent moves towards debris
    void RewardMoveTowardsDebris()
    {
        // Check if agent moves towards debris
        for (int i = 0; i < debrisInfos.Count; i++)
        {
            if (debrisInfos[i].transform == null)
                break;
            
            // Check that debris is not in zone or shovel and is located
            if (!dropZone.IsInZone(debrisInfos[i].transform.position) && !debrisInShovel.GetDebrisInArea()[i] && listIsDebrisLocated[i])
            {
                // Subtract previous distance from current distance
                float distanceDifference = debrisInfos[i].distanceFromRobot - debrisInfos[i].lastDistanceFromRobot;
                
                // Check if robot has gotten closer by at least 0.01m (if difference is negative, it has gotten closer)
                if (distanceDifference < -0.01f)
                {
                    AddReward(reward_moveTowardsDebris, "Moved towards debris");
                    // Enable break for points to be given when moving towards atleast 1 debris (otherwise points are given up to reward * amount of debris)
                    break;
                }
            }
        }
    }
    
    // Reward for each time the agent moves towards the zone with debris
    // And penalty for moving away from the zone with debris
    void RewardMoveTowardsZoneWithDebris()
    {
        previousDistanceFromZone = currentDistanceFromZone;
        currentDistanceFromZone = Vector3.Distance(transform.position, dropZone.transform.position);

        bool carryingDebris = false;
        
        // Check if agent is carrying debris
        for (int i = 0; i < debrisInfos.Count; i++)
        {
            if (currentDebrisInShovel[i])
            {
                carryingDebris = true;
            }
        }

        float distanceDifference = currentDistanceFromZone - previousDistanceFromZone;
        if (carryingDebris)
        {
            if (distanceDifference < -0.01f)
                AddReward(reward_moveTowardsZoneWithDebris, "Moved towards zone with debris");
            
            if (distanceDifference > 0.01f)
                AddReward(penalty_moveAwayFromZoneWithDebris, "Moved away from zone with debris");
        }
    }

    // Check if debris has entered or left the dropzone
    void RewardDebrisInOutZone()
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
    }

    // Check if debris has entered or left shovel
    void RewardDebrisInShovel()
    {
        currentDebrisInShovel = debrisInShovel.GetDebrisInArea();

        for (int i = 0; i < currentDebrisInShovel.Count; i++)
        {
            if (currentDebrisInShovel[i] && !previousDebrisInShovel[i])
            {
                AddReward(reward_debrisEnteredShovel, "debris entered shovel");
                previousDebrisInShovel[i] = true;
            }
            
            if (previousDebrisInShovel[i] && !currentDebrisInShovel[i] && !dropZone.IsInZone(debrisInfos[i].transform.position))
            {
                AddReward(penalty_debrisLeftShovel, "debris left shovel outside DropZone");
                previousDebrisInShovel[i] = false;
            }
        }
    }

    // Constantly deduct rewards
    void PenaltyTime()
    {
        //AddReward(penalty_time, "Time passed");
        AddReward(penalty_time); // no message to avoid spam in console
    }

    void PenaltyForHittingWalls()
    {
        // Check every wallRammingPenalties queue and add
        while (wallRammingPenalties.Count > 0)
        {
            wallRammingPenalties.Dequeue();
            AddReward(penalty_robotRammingWall,"robot ramming wall");
        }
    }

    // Check if robot has fallen or is outside area
    void RobotUpright()
    {
        // Check if robot has fallen
        if (Vector3.Dot(transform.up, Vector3.up) < 0.1f)
        {
            AddReward(penalty_robot_fall, "Robot fell");
            Done("robot has fallen (probably)");
        }
        
        // Check if robot is out of bounds
        Vector3 robotPosition = transform.position;
        if (robotPosition.x > 25f || robotPosition.x < -25f || robotPosition.z > 25f || robotPosition.z < -25f || robotPosition.y < -5f)
            Done("robot is out of bounds");
    }

    // Check if goal is met, if then Done()
    void IsGoalMet()
    {
        // Check if goal is met
        if (dropZone.IsAllDebrisInZone())
        {
            AddReward(reward_allDebrisEnteredZone, "all debris in zone");
            Done("goal reached (all debris in zone)");
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.layer == LayerMask.NameToLayer("environment"))
        {
            wallRammingPenalties.Enqueue(1);
        }
    }

    // Wrapper function for Done that prints a custom done message in console
    void Done(string reason)
    {
        Debug.Log("Done! reason: " + reason);
        Done();
        
        academy.ResetDebrisInZone();

        // Reset shovel content on restart
        currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        
        // Force reset if not using Python
        if (!academy.IsCommunicatorOn)
            academy.ForceForcedFullReset();
    }

    // Wrapper function for AddReward that prints the reward/penalty and custom message in console
    public void AddReward(float reward, string message)
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

    public float GetElapsedTime()
    {
        return timeElapsed;
    }

    public float[] GetActionVector()
    {
        return actionVector;
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
        
        Handles.color = Color.red;
        Handles.DrawLine(transform.position, transform.position + rb.velocity);
    }
}

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
    RobotAcademy academy;
    Rigidbody rb;
    //ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    DropZone dropZone;
    DisplayRewards displayRewards;
    
    [FormerlySerializedAs("debrisDetector")] [SerializeField]
    DebrisDetector debrisInShovel;

    [FormerlySerializedAs("debrisInfront")] [SerializeField]
    DebrisDetector debrisInFront;
    
    List<bool> currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    
    List<bool> currentDebrisInFront = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInFront = new List<bool>() {false, false, false, false, false, false};

    List<bool> lastHundredAttempts = new List<bool>();

    Vector3 startPosition;
    Quaternion startRotation;
    
    const float TimeLimit = 90f;

    bool doneHasBeenCalled = false;
    
    float timeElapsed;
    int timesWon = 0;
    int timesDone = 0;
    
    bool isAddedToSuccessRateList = false;

    List<string> observationNames = new List<string>();
    bool observationsLogged = false;
    
    // Robot control values
    const float Robot_MovementPerStep = 0.1f; // (meters)
    const float Robot_RotationPerStep = 2f; // (degrees)

    // Positive rewards
    const float Reward_DebrisCameInFront = 0.08f;
    const float Reward_DebrisEnteredShovel = 0.2f;
    const float Reward_DebrisEnteredZone = 0.4f;
    const float Reward_AllDebrisEnteredZone = 1f;
    const float Reward_DebrisFound = 0.1f;
 
    const float Reward_MoveTowardsDebris = 0.01f;
    const float Reward_MoveTowardsZoneWithDebris = 0.2f;
 
    // Negative rewards
    const float Penalty_DebrisLeftInFront = -0.16f;
    const float Penalty_DebrisLeftShovel = -0.4f;
    const float Penalty_DebrisLeftZone = -1f;
    const float Penalty_MoveAwayFromZoneWithDebris = -0.2f;
 
    const float Penalty_RobotRammingWall = -0.5f;
 
    const float Penalty_Time = -0.01f;
    
    const float Penalty_RobotNotLevel = -0.01f;
    const float Penalty_RobotFall = -1f;
    

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
    
    public override void InitializeAgent()
    {
        academy = FindObjectOfType<RobotAcademy>();
        rb = GetComponent<Rigidbody>();
        //shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();
        displayRewards = FindObjectOfType<DisplayRewards>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;

        lastCheckedPosition = startPosition;
    }

    public override void AgentReset()
    {
        debrisInShovel.InitializeDetector();
        debrisInFront.InitializeDetector();
        
        // Hard initialized to 6*false, one for each debris
        listIsDebrisLocated = new List<bool>() {false, false, false, false, false, false};
        
        wallRammingPenalties = new Queue<float>();

        lastCheckedPosition = transform.position;

        currentDistanceFromZone = Vector3.Distance(transform.position, dropZone.transform.position);
        previousDistanceFromZone = currentDistanceFromZone;
        
        //shovel.ResetRotations();

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
        // Robot position
        Vector3 currentPosition = transform.position;
        AddVectorObs(currentPosition.x, "robot_position_x");
        AddVectorObs(currentPosition.z, "robot_position_z");

        // Robot rotation
        AddVectorObs(transform.rotation.eulerAngles.y, "robot_rotation");

        // Robot velocity
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        AddVectorObs(localVelocity.x, "robot_velocity_x");
        AddVectorObs(localVelocity.y, "robot_velocity_y");
        AddVectorObs(localVelocity.z, "robot_velocity_z");
        
        // Shovel position
        //AddVectorObs(shovel.GetShovelPos(), "shovel_position");
        
        // DropZone position and radius
        Vector3 dropZonePosition = dropZone.transform.position;
        AddVectorObs(dropZonePosition.x, "dropzone_position_x");
        AddVectorObs(dropZonePosition.z, "dropzone_position_z");
        AddVectorObs(dropZone.GetRadius(), "dropzone_radius");

        // Distance sensor measurements
        float[] distances = sensors.GetMeasuredDistances();
        for (int dist = 0; dist < distances.Length; dist++)
        {
            AddVectorObs(distances[dist], "sensor_measurement_" + (dist+1));
        }

        // Debris positions
        debrisInfos = vision.UpdateVision();

        for (int debris = 0; debris < debrisInfos.Count; debris++)
        {
            AddVectorObs(debrisInfos[debris].lastKnownPosition.x, "debris_" + (debris+1) + "_position_x");
            AddVectorObs(debrisInfos[debris].lastKnownPosition.y, "debris_" + (debris+1) + "_position_y");
            AddVectorObs(debrisInfos[debris].lastKnownPosition.z, "debris_" + (debris+1) + "_position_z");
        }
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(Mathf.Infinity, "debris_" + (i+debrisInfos.Count+1) + "_position_x");
            AddVectorObs(Mathf.Infinity, "debris_" + (i+debrisInfos.Count+1) + "_position_y");
            AddVectorObs(Mathf.Infinity, "debris_" + (i+debrisInfos.Count+1) + "_position_z");
        }

        // Simulation time
        AddVectorObs(timeElapsed, "simulation_time");

        // Features:

        //Check if robot is within dropZone, Returns boolean
        bool isInDropZone = dropZone.IsInZone(transform.position);
        AddVectorObs(isInDropZone, "robot_in_dropzone");

        ObsDistanceToDebris();
        ObsDebrisIsInShovel();
        ObsAngleToDebris();
        ObsDebrisInFront();
        ObsFacingDebris();
        ObsGettingCloserToDebris();

        // Angle to dropzone
        float angleToDropZone = Vector3.Angle(dropZonePosition, transform.forward);
        if (transform.rotation.eulerAngles.y > 180) {
            angleToDropZone = -angleToDropZone;
        }
        AddVectorObs(angleToDropZone, "angle_to_dropzone");

        ObsDebrisToDropZone();
        ObsNextAngleToDebris();
        
        
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
#endif

        observationsLogged = true;
    }

    void AddVectorObs(float observation, string observationName)
    {
        AddVectorObs(observation);
        
        if (observationsLogged)
            return;

        observationNames.Add(observationName);
    }
    
    void AddVectorObs(bool observation, string observationName)
    {
        AddVectorObs(observation);
        
        if (observationsLogged)
            return;

        observationNames.Add(observationName);
    }
    
    // *Old: Check if robot is getting closer to debris, Returns boolean
    // *New: Distance between robot and each debris with a total of 6, returns floats
    void ObsDistanceToDebris()
    {
        for (int debris = 0; debris < debrisInfos.Count; debris++)
        {
            Vector3 rbNewPosition = rb.position + rb.velocity; //robot current position + velocity
            
            float distanceToDebris = Vector3.Distance(debrisInfos[debris].lastKnownPosition, rbNewPosition);
            
            
            AddVectorObs(distanceToDebris, "distance_to_debris_" + (debris+1));
        }

        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(Mathf.Infinity, "distance_to_debris_" + (i+debrisInfos.Count+1));
        }
    }

    //Check if robot is getting closer to debris, Returns boolean
    void ObsGettingCloserToDebris()
    {
        int debIndex = 1;
        foreach (var debrisInfo in debrisInfos)
        {
            bool gettingCloserToDebris = debrisInfo.distanceFromRobot < debrisInfo.lastDistanceFromRobot;
            AddVectorObs(gettingCloserToDebris, "getting_closer_to_debris_" + debIndex);
            debIndex++;
        }
        
        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(false, "getting_closer_to_debris_" + (i+debrisInfos.Count+1));
        }
    }

    //Check if robot has picked up debris, Returns boolean
    void ObsDebrisIsInShovel()
    {
        List<bool> debrisInShovelList = debrisInShovel.GetDebrisInArea();
        bool debrisIsInShovel = false;
        foreach (var value in debrisInShovelList)
        {
            if (value)
            {
                debrisIsInShovel = true;
            }
        }

        AddVectorObs(debrisIsInShovel, "debris_in_shovel");
    }
    
    // Angle between (Robot forward) and (vector between robot and debris), Returns float
    void ObsAngleToDebris()
    {
        var robotPosition = transform.position;
        var forward = transform.forward;
        // Vec2 pointing straight from robot
        Vector2 vec2TransformForward = new Vector2(forward.x, forward.z);
        for (int debris = 0; debris < debrisInfos.Count; debris++)
        {
            var debrisPosition = debrisInfos[debris].transform.position;
            
            // Create vector2 from robot to debris
            Vector2 vec2RobotToDebris = new Vector2(debrisPosition.x - robotPosition.x,
                debrisPosition.z - robotPosition.z);
            
            // Find angle between robot direction and debris (Signed to indicate which side the debris is closest to)
            float angleToDebris = Vector2.SignedAngle(vec2RobotToDebris, vec2TransformForward);
            AddVectorObs(angleToDebris, "angle_robot_debris_" + (debris+1));
        }
        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(360f, "angle_robot_debris_" + (i+debrisInfos.Count+1));
        }
    }
    
    //Check if debris infront of shovel, Returns boolean
    void ObsDebrisInFront()
    {
        List<bool> debrisInFrontList = debrisInFront.GetDebrisInArea();
        bool debrisIsInFront = false;
        foreach (var value in debrisInFrontList)
        {
            if (value)
            {
                debrisIsInFront = true;
            }
        }
        AddVectorObs(debrisIsInFront, "debris_in_front");
    }

    // Check if robot is pointed towards a debris, Returns boolean 
    // TODO Does not take walls into account
    void ObsFacingDebris()
    {
        int counter = 0;
        bool pointingTowardDebris = false;
        List<bool> debrisInShovelList = this.debrisInShovel.GetDebrisInArea();
        var robotPosition = transform.position;
        Vector2 vecForward = new Vector2(transform.forward.x, transform.forward.z);

        foreach (var debris in debrisInfos)
        {
            Vector2 vecRobotToDebris2 = new Vector2(debris.transform.position.x - robotPosition.x,
                debris.transform.position.z - robotPosition.z);

            // Check it is not in DropZone
            if (!dropZone.IsInZone(debris.transform.position) && !debrisInShovelList[counter])
            {
                float dot = Vector2.Dot(vecForward, (vecRobotToDebris2).normalized);
                if (dot > 0.999f)
                {
                    pointingTowardDebris = true;
                }
            }
            counter++;
        }
        AddVectorObs(pointingTowardDebris, "robot_facing_debris");
    }
    
    // If there are fewer than 6 debris, pad out the observations
    void PadOutDebrisObs(int observationAmount, float observation, string observationName)
    {
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            for (int j = 0; j < observationAmount; j++)
            {
                AddVectorObs(observation, observationName);
            }
        }
    }
    
    // distance from each debris to DropZone (total of 6)
    void ObsDebrisToDropZone()
    {
        //List for every debris if it is in the shovel
        List<bool> debrisInShovelList = debrisInShovel.GetDebrisInArea();

        // go through debris
        for (int i = 0; i < debrisInfos.Count; i++)
        {
            bool debrisCloserToDropZone = false;
         
            // if debris is in the shovel
            if (debrisInShovelList[i].Equals(true))
            {
                
                // find the next position and predict the distance to dropzone
                Vector3 dropZonePosition = dropZone.transform.position;
                
                // as the debris is in the shovel, to predict the next position, we use robot's position
                Vector3 rbNewPosition = rb.position + rb.velocity; 
                
                // the length from DropZone to robot's new position is the new distance we will use 
                float debrisToDropZone = Vector3.Distance(dropZonePosition, rbNewPosition);

                float oldDebrisToDropZone = Vector3.Distance(dropZonePosition, rb.position);

                // check if the new distance is shorter than the old distance
                debrisCloserToDropZone = debrisToDropZone < oldDebrisToDropZone;
            }
            
            AddVectorObs(debrisCloserToDropZone, "debris_to_dropzone_" + (i+1));
        }
        
        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(Mathf.Infinity, "debris_to_dropzone_" + (i+debrisInfos.Count+1));
        }
    }
    
    // Predict the next angle between robot and each debris (83 -> 88)
    void ObsNextAngleToDebris()
    {
        int counter = 0;
        foreach (var debris in debrisInfos)
        {
            // Angle between debris and robot
            float angle_to_debris =  Vector2.SignedAngle(debris.transform.position, transform.forward);

            counter++;
            
            AddVectorObs(angle_to_debris, "angle_to_debris_" + counter);
        }
        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(Mathf.Infinity, "angle_to_debris_" + (i+debrisInfos.Count+1));
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (doneHasBeenCalled)
            return;
        
        // Perform actions
        ControlRobot(Mathf.FloorToInt(vectorAction[0]), Mathf.FloorToInt(vectorAction[1]));
        
        //shovel.RotateShovel(vectorAction[2]);
        
        // Store current action vector state for visualization
        actionVector = vectorAction;
        
        // if robot has moved enough to do a distance check, set checked bool to true
        checkedPositionThisStep = Vector3.Distance(lastCheckedPosition, transform.position) > MinimumDistanceBeforeCheck;
        
        //Evaluation Methods:
        CreateListWithSuccessRate();

        // Give rewards or penalties
        RewardDebrisInShovel();
        RewardDebrisCameInFront();
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

        // if the check bool is true, update last checked position
        if (checkedPositionThisStep)
            lastCheckedPosition = transform.position;
        
        // Reset if time limit is reached
        if (timeElapsed > TimeLimit)
        {
            timeElapsed = 0;
            lastHundredAttempts.Add(false);
            Done("Time limit reached");
        }
    }

    void ControlRobot(int drive, int rotate)
    {
        if (drive == 1)
            rb.MovePosition(rb.position + transform.forward * Robot_MovementPerStep);
        else if (drive == -1)
            rb.MovePosition(rb.position - transform.forward * Robot_MovementPerStep);
        
        if (rotate == 1)
            transform.Rotate(transform.up, Robot_RotationPerStep);
        else if (rotate == -1)
            transform.Rotate(transform.up, -Robot_RotationPerStep);
    }

    // Reward given for the first time each debris is seen
    void RewardLocateDebris()
    {
        // Check for each debris if it is visible and has not been seen before
        for (int debrisNum = 0; debrisNum < debrisInfos.Count; debrisNum++)
        {
            if (debrisInfos[debrisNum].isVisible && !listIsDebrisLocated[debrisNum])
            {
                AddReward(Reward_DebrisFound, "Debris was located", debrisInfos[debrisNum].transform.position);
                listIsDebrisLocated[debrisNum] = true;
            }
        }
    }

    // Reward for each time the agent moves towards debris
    void RewardMoveTowardsDebris()
    {
        // if robot has moved enough to do a distance check
        if (checkedPositionThisStep)
        {
            // Check if agent moves towards debris
            for (int i = 0; i < debrisInfos.Count; i++)
            {
                if (debrisInfos[i].transform == null)
                    break;
            
                // Check that debris is not in zone or shovel and is located
                if (!dropZone.IsInZone(debrisInfos[i].transform.position) && !debrisInShovel.GetDebrisInArea()[i] && listIsDebrisLocated[i])
                {
                    Vector3 debrisPos = debrisInfos[i].transform.position;

                    // If current distance is less than last checked position distance
                    if (Vector3.Distance(debrisPos, transform.position) < Vector3.Distance(debrisPos, lastCheckedPosition))
                    {
                        AddReward(Reward_MoveTowardsDebris, "Moved towards debris", transform.position);
                        // Enable break for points to be given when moving towards at least 1 debris (otherwise points are given up to reward * amount of debris)
                        break;
                    }
                }
            }
        }
    }
    
    // Reward for each time the agent moves towards the zone with debris
    // And penalty for moving away from the zone with debris
    void RewardMoveTowardsZoneWithDebris()
    {
        // if robot has moved enough to do a distance check
        if (checkedPositionThisStep)
        {
            bool carryingDebris = false;
            
            // Check if agent is carrying debris
            for (int i = 0; i < debrisInfos.Count; i++)
            {
                if (currentDebrisInShovel[i])
                {
                    carryingDebris = true;
                }
            }

            if (carryingDebris)
            {
                previousDistanceFromZone = Vector3.Distance(lastCheckedPosition, dropZone.transform.position);
                currentDistanceFromZone = Vector3.Distance(transform.position, dropZone.transform.position);
                
                if (currentDistanceFromZone < previousDistanceFromZone)
                    AddReward(Reward_MoveTowardsZoneWithDebris, "Moved towards zone with debris", transform.position);
                
                if (currentDistanceFromZone > previousDistanceFromZone)
                    AddReward(Penalty_MoveAwayFromZoneWithDebris, "Moved away from zone with debris", transform.position);
            }
        }
    }

    // Check if debris has entered or left the DropZone
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
                    AddReward(Penalty_DebrisLeftZone, "debris left zone", debrisInfos[i].transform.position);
            }
            else
            {
                if (currentDebrisInZone[i])
                    AddReward(Reward_DebrisEnteredZone, "debris entered zone", debrisInfos[i].transform.position);
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
                AddReward(Reward_DebrisEnteredShovel, "debris entered shovel", debrisInfos[i].transform.position);
                previousDebrisInShovel[i] = true;
            }
            
            if (previousDebrisInShovel[i] && !currentDebrisInShovel[i] && !dropZone.IsInZone(debrisInfos[i].transform.position))
            {
                AddReward(Penalty_DebrisLeftShovel, "debris left shovel outside DropZone", debrisInfos[i].transform.position);
                previousDebrisInShovel[i] = false;
            }
        }
    }

    // Constantly deduct rewards
    void PenaltyTime()
    {
        //AddReward(penalty_time, "Time passed", dropZone.transform.position);
        AddReward(Penalty_Time); // no message to avoid spam
    }

    void PenaltyForHittingWalls()
    {
        // Check every wallRammingPenalties queue and add
        while (wallRammingPenalties.Count > 0)
        {
            wallRammingPenalties.Dequeue();
            AddReward(Penalty_RobotRammingWall,"robot ramming wall", transform.position);
        }
    }
    
    // AddReward if debris in front
    void RewardDebrisCameInFront()
    {
        currentDebrisInFront = debrisInFront.GetDebrisInArea();
        
        for (int i = 0; i < currentDebrisInFront.Count; i++)
        {
            if (currentDebrisInFront[i] && !previousDebrisInFront[i])
            {
                AddReward(Reward_DebrisCameInFront, "debris came in front", debrisInfos[i].transform.position);
                previousDebrisInFront[i] = true;
            }
            
            if (previousDebrisInFront[i] && !currentDebrisInFront[i] && debrisInShovel && !dropZone.IsInZone(debrisInfos[i].transform.position))
            {
                AddReward(Penalty_DebrisLeftInFront, "debris left in front", debrisInfos[i].transform.position);
                previousDebrisInFront[i] = false;
            }
        }
    }

    // Check if robot is not level, has fallen or is outside area
    void RobotUpright()
    {
        float robotUpDotWorldUp = Vector3.Dot(transform.up, Vector3.up);

        // Check if robot is not level
        if (robotUpDotWorldUp < 0.9f)
        {
            AddReward(Penalty_RobotNotLevel, "Robot not level", transform.position);
        }
        
        // Check if robot has fallen
        if (robotUpDotWorldUp < 0.1f)
        {
            AddReward(Penalty_RobotFall, "Robot fell", transform.position);
            lastHundredAttempts.Add(false);
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
            AddReward(Reward_AllDebrisEnteredZone, "all debris in zone", dropZone.transform.position);
            Done("goal reached (all debris in zone)");
            
            lastHundredAttempts.Add(true);
            timesWon++;
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
        doneHasBeenCalled = true;
        
        Debug.Log("Done! reason: " + reason);
        Done();

        if (lastHundredAttempts.Count >= 100)
        {
            
            lastHundredAttempts.RemoveAt(0);
        }
        
        timesDone++;

        academy.ResetDebrisInZone();
        
        // Reset shovel content on restart
        currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        
        // Force reset if not using our Python script
        if (!academy.IsCommunicatorOn || academy.communicatorPort == RobotAcademy.CommunicatorPort.DefaultTraining)
            academy.ForceForcedFullReset();
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
        float[] heuristicValues = new float[4];

        heuristicValues[0] = Input.GetAxis("Vertical");
        heuristicValues[1] = Input.GetAxis("Horizontal");

        heuristicValues[2] = (Input.GetKey(KeyCode.Q)) ? 1f : (Input.GetKey(KeyCode.E)) ? -1f : 0f;

        return heuristicValues;
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

#if UNITY_EDITOR
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
#endif
}

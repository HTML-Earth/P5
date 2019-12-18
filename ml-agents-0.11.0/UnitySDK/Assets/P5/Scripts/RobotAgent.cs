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
    public enum RobotGoal
    {
        AllDebrisInDropZone,
        RobotInDropZone
    };
    
    // References that are fetched by the script
    Rigidbody rb;
    ShovelControl shovel;
    RobotSensors sensors;
    RobotVision vision;
    WheelDrive wheels;
    DropZone dropZone;
    DisplayRewards displayRewards;
    
    // Settings and references set in the editor
    
    [Header("Robot")]
    public bool useSimplePhysics;
    public bool limitedObservations;

    [Header("References")]
    [SerializeField]
    RobotEnvironment environment;
    
    [SerializeField]
    DebrisDetector debrisInShovel;

    [SerializeField]
    DebrisDetector debrisInFront;
    
    [Header("Goal")]
    public RobotGoal goal;
    public float timeLimit = 90f;
    public bool resetOnHitWall = true;
    
    // Rewards and Penalties (set to 0 in inspector to disable)
    
    [Header("Rewards")]
    public float rewardGoalCompleted               = 0f;
    
    public float rewardDebrisEnteredFront          = 0f;
    public float rewardDebrisEnteredShovel         = 0f;
    public float rewardDebrisEnteredZone           = 0f;
 
    public float rewardDebrisFound                 = 0f;
    public float rewardFacingDebris                = 0f;

    [Header("Penalties (should be negative values)")]
    public float penaltyDebrisLeftFront            = -0f;
    public float penaltyDebrisLeftShovel           = -0f;
    public float penaltyDebrisLeftZone             = -0f;
    
    public float penaltyRobotHittingWall           = -0f;
    

    [Header("Debug")]
    public bool displayGizmos = false;
    
    // Training
    float timeElapsed;
    int timesWon = 0;
    int timesDone = 0;
    
    bool doneHasBeenCalled = false;
    bool isAddedToSuccessRateList = false;
    
    const int DebrisCount = 6;
    
    // Robot control values
    const float Robot_MovementPerStep = 0.1f; // (meters)
    const float Robot_RotationPerStep = 2f;   // (degrees)

    // Reset values
    Vector3 startPosition;
    Quaternion startRotation;

    // Positions relative to robot
    Vector3 debrisPosition;
    Vector3 dropZonePosition;
    
    // Observations
    List<string> observationNames = new List<string>();
    bool observationsLogged = false;

    // Current state of action vector
    float[] actionVector;
    
    // Variables used to check for rewards
    List<RobotVision.DebrisInfo> debrisInfos;
    
    List<bool> listIsDebrisLocated;
    
    List<bool> currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
    
    List<bool> currentDebrisInFront = new List<bool>() {false, false, false, false, false, false};
    List<bool> previousDebrisInFront = new List<bool>() {false, false, false, false, false, false};
    
    const float MinimumDistanceBeforeCheck = 0.5f;
    bool checkedPositionThisStep;
    Vector3 lastCheckedPosition;
    
    float previousDistanceFromZone;
    float currentDistanceFromZone;
    
    float highestDotProduct;
    
    // Gizmo colors
    readonly Color debrisHighlight = new Color(0,1,1);
    readonly Color debrisHighlightMissing = new Color(1,0,0);

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

            rb.constraints = RigidbodyConstraints.FreezeRotation;
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
        // Robot position in each environment
        AddVectorObs(this.transform.position - environment.transform.position);

        // Robot position
        Vector3 currentPosition = transform.position - environment.transform.position;
        AddVectorObs(currentPosition.x, "robot_position_x");
        AddVectorObs(currentPosition.z, "robot_position_z");

        // Robot rotation
        AddVectorObs(transform.rotation.eulerAngles.y, "robot_rotation");

        // Robot velocity
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        AddVectorObs(localVelocity.x, "robot_velocity_x");
        AddVectorObs(localVelocity.z, "robot_velocity_z");

        // Shovel position
        AddVectorObs(shovel.GetShovelPos(), "shovel_position");
        
        // Comment out these observations if using PPO
        AddVectorObs(timesWon, "times_won");
        AddVectorObs(timeElapsed, "time_elapsed");

        // DropZone position and radius
        dropZonePosition = transform.InverseTransformPoint(dropZone.transform.position);
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
        
        if (limitedObservations)
        {
            // Debris Position
            debrisPosition = Vector3.zero;
            if (debrisInfos != null && debrisInfos.Count > 0 && debrisInfos[0].transform != null)
                debrisPosition = transform.InverseTransformPoint(debrisInfos[0].transform.position);
        
            AddVectorObs(debrisPosition.x, "debris_position_x");
            AddVectorObs(debrisPosition.z, "debris_position_z");
        }
        else
        {
            for (int debris = 0; debris < debrisInfos.Count; debris++)
            {
                AddVectorObs(debrisInfos[debris].lastKnownPosition.x, "debris_" + (debris+1) + "_position_x");
                AddVectorObs(debrisInfos[debris].lastKnownPosition.z, "debris_" + (debris+1) + "_position_z");
            }
            
            for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
            {
                AddVectorObs(0f, "debris_" + (i+debrisInfos.Count+1) + "_position_x");
                AddVectorObs(0f, "debris_" + (i+debrisInfos.Count+1) + "_position_z");
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
        }

#if UNITY_EDITOR
        // AUTO GENERATE RobotObservations.py
        if (!observationsLogged && !limitedObservations)
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


            AddVectorObs(distanceToDebris, "distance_to_debris_" + (debris + 1));
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
        
        if (doneHasBeenCalled)
            return;

        int debIndex = 1;
        foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
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
        bool debrisIsInShovel = false;
        foreach (bool value in currentDebrisInShovel)
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
        
        
        switch (goal)
        {
            case RobotGoal.AllDebrisInDropZone:
            {
                // Check for debris related rewards
                RewardFacingDebris();
                RewardLocateDebris();
                RewardDebrisInFront();
                RewardDebrisInShovel();
                RewardDebrisInOutZone();
                
                float debrisDistanceToDropZone = Vector3.Distance(debrisInfos[0].transform.position,dropZone.transform.position);
        
                // Reached target
                if (debrisDistanceToDropZone < 5f)
                {
                    timesWon++;
                    AddReward(rewardGoalCompleted, "goal completed");
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
                    timesWon++;
                    AddReward(rewardGoalCompleted, "goal completed");
                    Done("Robot in zone");
                }
                
                break;
            }
        }
        
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
        if (!(rewardDebrisFound > 0))
            return;
        
        // Check for each debris if it is visible and has not been seen before
        for (int debrisNum = 0; debrisNum < debrisInfos.Count; debrisNum++)
        {
            if (debrisInfos[debrisNum].isVisible && !listIsDebrisLocated[debrisNum])
            {
                AddReward(rewardDebrisFound, "Debris was located");//, debrisInfos[debrisNum].transform.position);
                listIsDebrisLocated[debrisNum] = true;
            }
        }
    }

    void RewardFacingDebris()
    {
        if (!(rewardFacingDebris > 0))
            return;
        
        if (debrisInfos[0].transform != null)
        {
            Vector3 robotToDebris = debrisInfos[0].transform.position - transform.position;
            
            float fwdDotDebris = Vector3.Dot(transform.forward, robotToDebris.normalized);

            if (fwdDotDebris > highestDotProduct)
            {
                highestDotProduct = fwdDotDebris;
                AddReward(rewardFacingDebris, "facing debris");
            }
        }
    }
    
    // Check if debris has entered or left front
    void RewardDebrisInFront()
    {
        currentDebrisInFront = debrisInFront.GetDebrisInArea();
        
        for (int i = 0; i < currentDebrisInFront.Count; i++)
        {
            if (rewardDebrisEnteredFront > 0 &&
                currentDebrisInFront[i] && !previousDebrisInFront[i])
            {
                AddReward(rewardDebrisEnteredFront, "debris came in front");//, debrisInfos[i].transform.position);
                previousDebrisInFront[i] = true;
            }
            
            if (penaltyDebrisLeftFront > 0 &&
                previousDebrisInFront[i] && !currentDebrisInFront[i] && debrisInShovel && !dropZone.IsInZone(debrisInfos[i].transform.position))
            {
                AddReward(penaltyDebrisLeftFront, "debris left in front");//, debrisInfos[i].transform.position);
                previousDebrisInFront[i] = false;
            }
        }
    }

    // Check if debris has entered or left shovel
    void RewardDebrisInShovel()
    {
        currentDebrisInShovel = debrisInShovel.GetDebrisInArea();

        for (int i = 0; i < currentDebrisInShovel.Count; i++)
        {
            if (rewardDebrisEnteredShovel > 0 &&
                currentDebrisInShovel[i] && !previousDebrisInShovel[i])
            {
                AddReward(rewardDebrisEnteredShovel, "debris entered shovel");//, debrisInfos[i].transform.position);
                previousDebrisInShovel[i] = true;
            }
            
            if (penaltyDebrisLeftShovel < 0 && 
                previousDebrisInShovel[i] && !currentDebrisInShovel[i] &&
                !dropZone.IsInZone(debrisInfos[i].transform.position))
            {
                AddReward(penaltyDebrisLeftShovel, "debris left shovel outside DropZone");//, debrisInfos[i].transform.position);
                previousDebrisInShovel[i] = false;
            }
        }
    }
    
    // Check if debris has entered or left the DropZone
    void RewardDebrisInOutZone()
    {
        // Check if debris has left/entered the zone
        List<bool> previousDebrisInZone = environment.GetPreviousDebrisInZone();
        List<bool> currentDebrisInZone = environment.GetCurrentDebrisInZone();

        for (int i = 0; i < previousDebrisInZone.Count; i++)
        {
            if (previousDebrisInZone[i])
            {
                if (penaltyDebrisLeftZone < 0 && !currentDebrisInZone[i])
                    AddReward(penaltyDebrisLeftZone, "debris left zone");//, debrisInfos[i].transform.position);
            }
            else
            {
                if (rewardDebrisEnteredZone > 0 && currentDebrisInZone[i])
                    AddReward(rewardDebrisEnteredZone, "debris entered zone");//, debrisInfos[i].transform.position);
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject.layer == LayerMask.NameToLayer("environment"))
        {
            if (penaltyRobotHittingWall < 0)
                AddReward(penaltyRobotHittingWall, "hit wall");
            
            if (resetOnHitWall)
                Done("Hit wall");
        }
    }

    // Wrapper function for Done that prints a custom done message in console
    void Done(string reason)
    {
        if (doneHasBeenCalled)
            return;
        
        timesDone++;
        
        doneHasBeenCalled = true;
        
        Debug.Log("Done! reason: " + reason);
        Done();

        // Reset shovel content on restart
        currentDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        previousDebrisInShovel = new List<bool>() {false, false, false, false, false, false};
        
        // Force reset if not using our Python script
        //if (!academy.IsCommunicatorOn || academy.communicatorPort == RobotAcademy.CommunicatorPort.DefaultTraining)
        //    academy.ForceForcedFullReset();
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
            rb.MovePosition(rb.position + transform.forward * Robot_MovementPerStep);
        else if (drive == -1)
            rb.MovePosition(rb.position - transform.forward * Robot_MovementPerStep);
        
        if (rotate == 1)
            transform.Rotate(transform.up, Robot_RotationPerStep);
        else if (rotate == -1)
            transform.Rotate(transform.up, -Robot_RotationPerStep);
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

    public float[] GetActionVector()
    {
        return actionVector;
    }

    public float GetElapsedTime()
    {
        return timeElapsed;
    }
    
    public int GetTimesDone()
    {
        return timesDone;
    }
    
    public int GetTimesWon()
    {
        return timesWon;
    }

    // Translates action value into -1 (reverse), +1 (forward) or 0 (no throttle)
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
    
    // Translates action value into -1 (left), +1 (right) or 0 (no turning)
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
    
#if UNITY_EDITOR
    // Used to draw debug info on screen
    void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            return;
    
        if (!displayGizmos)
            return;
        
        if (debrisInfos == null)
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

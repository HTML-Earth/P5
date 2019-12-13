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
    RobotAcademy academy;
    Rigidbody rb;
    WheelDrive wheels;
    ShovelControl shovel;
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
        wheels = GetComponent<WheelDrive>();
        shovel = GetComponent<ShovelControl>();
        vision = GetComponent<RobotVision>();
        sensors = GetComponent<RobotSensors>();
        dropZone = FindObjectOfType<DropZone>();
        displayRewards = FindObjectOfType<DisplayRewards>();
        
        startPosition = transform.position;
        startRotation = transform.rotation;

        lastCheckedPosition = startPosition;
        AgentReset();
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
        // Robot position (0, 1)
        Vector3 currentPosition = transform.position;
        AddVectorObs(currentPosition.x);
        AddVectorObs(currentPosition.z);

        // Robot rotation (2)
        AddVectorObs(transform.rotation.eulerAngles.y);

        // Robot velocity (3, 4, 5)
        Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        AddVectorObs(localVelocity.x);
        //AddVectorObs(localVelocity.y);
        AddVectorObs(localVelocity.z);
        
        // Shovel position (6, 7) TODO: remove one of these
        //AddVectorObs(shovel.GetShovelPos());
        //AddVectorObs(shovel.GetShovelPos());

        // Drop-Zone Position and radius (8, 9, 10)
        Vector3 dropZonePosition = dropZone.transform.position;
        //AddVectorObs(dropZonePosition.x);
        //AddVectorObs(dropZonePosition.z);
        //AddVectorObs(dropZone.GetRadius());

        // Distance sensor measurements (11 - 40)
        float[] distances = sensors.GetMeasuredDistances();
        for (int dist = 0; dist < distances.Length; dist++)
        {
            AddVectorObs(distances[dist]);
        }

        // Debris positions (41 - 58)
        debrisInfos = vision.UpdateVision();

        //foreach (RobotVision.DebrisInfo debrisInfo in debrisInfos)
        //{
        AddVectorObs(debrisInfos[0].lastKnownPosition.x);
        //   AddVectorObs(debrisInfo.lastKnownPosition.y);
        AddVectorObs(debrisInfos[0].lastKnownPosition.z);
        //}
        //ObsPadOutInfinity(3);

        // Simulation time (59)
        //AddVectorObs(timeElapsed);

        // features:

        //Check if robot is within dropZone, Returns boolean (60)
        //bool isInDropZone = dropZone.IsInZone(transform.position);
        //AddVectorObs(isInDropZone);

        //ObsGettingCloserToDebris();   // Index 61 -> 66
        //ObsRobotPickedUpDebris();     // Index 67
        //bsAngleToDebris();           // Index 68 -> 73
        //ObsDebrisInFront();           // Index 74
        //ObsPointedAtDebris();         // Index 75

        // Check if robot is facing the zone (76)
        Vector3 robotToDropZone = dropZonePosition - rb.position;
        float angleToDropZone = Vector3.Angle(robotToDropZone, transform.forward);
        //AddVectorObs(angleToDropZone);

        //DebrisToDropZone();           // Index 77 -> 82
    }
    
    // *Old: Check if robot is getting closer to debris, Returns boolean (61 -> 66)
    // *New: Distance between robot and each debris with a total of 6, returns floats (61 -> 66)
    void ObsGettingCloserToDebris()
    {
        foreach (var debrisInfo in debrisInfos)
        {
            //bool gettingCloserToDebris = debrisInfo.distanceFromRobot < debrisInfo.lastDistanceFromRobot;
            //AddVectorObs(gettingCloserToDebris);
            
            Vector3 rbNewPosition = rb.position + rb.velocity; //robot current position + velocity
            
            float distanceToDebris = Vector3.Distance(debrisInfo.lastKnownPosition, rbNewPosition);
            
            
            AddVectorObs(distanceToDebris);
        }

        // If there are fewer than 6 debris, pad out the observations
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            AddVectorObs(false);
        }
    }
    
    //Check if robot has picked up debris, Returns boolean (67)
    void ObsRobotPickedUpDebris()
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

        AddVectorObs(debrisIsInShovel);
    }
    
    // Angle between (Robot forward) and (vector between robot and debris), Returns float (68 -> 73)
    void ObsAngleToDebris()
    {
        var robotPosition = transform.position;
        var forward = transform.forward;
        // Vec2 pointing straight from robot
        Vector2 vec2TransformForward = new Vector2(forward.x, forward.z);
        foreach (var debrisInfo in debrisInfos)
        {
            var debrisPosition = debrisInfo.transform.position;
            // Create vector2 from robot to debris
            Vector2 vec2RobotToDebris = new Vector2(debrisPosition.x - robotPosition.x,
                debrisPosition.z - robotPosition.z);
            // Find angle between robot direction and debris (Signed to indicate which side the debris is closest to)
            float angleToDebris = Vector2.SignedAngle(vec2RobotToDebris, vec2TransformForward);
            AddVectorObs(angleToDebris);
        }
        ObsPadOutInfinity(1);
    }
    
    //Check if debris infront of shovel, Returns boolean (74)
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
        AddVectorObs(debrisIsInFront);
    }

    // Check if robot is pointed towards a debris (75), Returns boolean //TODO Does not take walls into account
    void ObsPointedAtDebris()
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
        AddVectorObs(pointingTowardDebris);
    }
    
    // If there are fewer than 6 debris, pad out the observations
    void ObsPadOutInfinity(int observationAmount)
    {
        for (int i = 0; i < DebrisCount - debrisInfos.Count; i++)
        {
            for (int j = 0; j < observationAmount; j++)
            {
                AddVectorObs(Mathf.Infinity);
            }
        }
    }
    
    // distance from each debris to DropZone (total of 6) (77 -> 82)
    void DebrisToDropZone()
    {
        //List for every debris if it is in the shovel
        List<bool> debrisInShovelList = debrisInShovel.GetDebrisInArea();

        // make sure that the length of debrisInfo list and previous list is the same
        if (debrisInShovelList.Count.Equals(debrisInfos.Count))
        {
            // go through debris
            for (int i = 0; i < debrisInShovelList.Count; i++)
            {
                
                // if debris is in the shovel
                if (debrisInShovelList[i].Equals(true))
                {
                    bool debrisCloserToDropZone = false;
                    
                    // find the next position and predict the distance to dropzone
                    Vector3 dropZonePosition = dropZone.transform.position;
                    
                    // as the debris is in the shovel, to predict the next position, we use robot's position
                    Vector3 rbNewPosition = rb.position + rb.velocity; 
                    
                    // the length from DropZone to robot's new position is the new distance we will use 
                    float debrisToDropZone = Vector3.Distance(dropZonePosition, rbNewPosition);

                    float oldDebrisToDropZone = Vector3.Distance(dropZonePosition, rb.position);

                    // check if the new distance is shorter than the old distance
                    debrisCloserToDropZone = debrisToDropZone < oldDebrisToDropZone;

                    AddVectorObs(debrisCloserToDropZone);
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (doneHasBeenCalled)
            return;

        // Perform actions
        //print("vectoraction 0: " + vectorAction[0]);
        int movement = 0;
        int wheelAngle = 0;
        int rotateShovel = 0;
        //print("movement: " + movement);

        switch ((int)vectorAction[0])
        { 
            case 1:
                movement = -1;
                break;
            case 2:
                movement = 1;
                break;
        }

        switch ((int)vectorAction[1])
        {
            case 1:
                wheelAngle = -1;
                break;
            case 2:
                wheelAngle = 1;
                break;
        }

        switch ((int)vectorAction[2])
        {
            case 1:
                rotateShovel = -1;
                break;
            case 2:
                rotateShovel = 1;
                break;
        }

        wheels.SetTorque(movement);
        wheels.SetAngle(wheelAngle);
        shovel.RotateShovel(rotateShovel);
        
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

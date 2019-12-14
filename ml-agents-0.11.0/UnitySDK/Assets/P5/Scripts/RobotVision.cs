using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RobotVision : MonoBehaviour
{
    public struct DebrisInfo
    {
        public Transform transform;
        public Vector3 lastKnownPosition;
        public bool isVisible;
        public float distanceFromRobot;
        public float lastDistanceFromRobot;
    }
    
    public LayerMask layerMask;
    public Transform sensorPosition;

    public float visionAngle = 120f;

    RobotEnvironment environment;

    List<DebrisInfo> debrisInfos;
    
    Color fovColor = new Color(0,1,1, 0.5f);

    public void InitializeDebrisArray(RobotEnvironment env)
    {
        environment = env;
        
        // Get list of debris Transforms
        List<Debris> debrisList = environment.GetDebris();

        // Initialize list of DebrisInfos
        debrisInfos = new List<DebrisInfo>();
        
        // Add each debris transform and starting values to info list
        foreach (Debris d in debrisList)
        {
            DebrisInfo info = new DebrisInfo();
            info.transform = d.transform;
            info.lastKnownPosition = Vector3.positiveInfinity;
            info.isVisible = false;
            info.distanceFromRobot = Vector3.Distance(d.transform.position, transform.position);
            info.lastDistanceFromRobot = info.distanceFromRobot;

            debrisInfos.Add(info);
        }
        
        UpdateVision();
    }

    public List<DebrisInfo> UpdateVision()
    {
        if (debrisInfos == null)
            return new List<DebrisInfo>();
        
        if (debrisInfos.Count > 0 && debrisInfos[0].transform == null)
            return new List<DebrisInfo>();
        
        Vector3 currentSensorPosition = sensorPosition.position;
        
        // For every debris -> check if it is visible
        for (int i = 0; i < debrisInfos.Count; i++)
        {
            // Assign struct to variable to allow modification
            DebrisInfo currentDebrisInfo = debrisInfos[i];
            
            // Current debris-position
            Vector3 currentDebrisPosition = currentDebrisInfo.transform.position;

            // Vector from Sensor-position to current Debris-position
            Vector3 directionToDebris = currentDebrisPosition - currentSensorPosition;

            // Last Debris distance
            currentDebrisInfo.lastDistanceFromRobot = currentDebrisInfo.distanceFromRobot;
            
            // Debris distance from robot
            currentDebrisInfo.distanceFromRobot = Vector3.Distance(currentDebrisPosition, transform.position);

            // If debris is within vision cone
            if (DirectionIsWithinVision(directionToDebris))
            {
                Ray ray = new Ray(currentSensorPosition, directionToDebris);
                float distanceToDebris = Vector3.Distance(currentSensorPosition, currentDebrisPosition);

                // Cast a ray from the Sensor-position to Debris-position
                // If raycast from Robot to Debris gets blocked -> not visible, else -> visible
                if (Physics.Raycast(ray, out RaycastHit hitInfo, distanceToDebris, layerMask))
                    currentDebrisInfo.isVisible = false;
                else
                {
                    currentDebrisInfo.isVisible = true;
                    currentDebrisInfo.lastKnownPosition = currentDebrisPosition;
                }
            }
            else
            {
                currentDebrisInfo.isVisible = false;
                
                // Vector from Sensor-position to last known Debris-position
                Vector3 directionToLastKnownPos = currentDebrisInfo.lastKnownPosition - currentSensorPosition;

                // If last known position is within vision cone
                if (DirectionIsWithinVision(directionToLastKnownPos))
                {
                    Ray ray = new Ray(currentSensorPosition, directionToDebris);
                    float distanceToLastKnownPos = Vector3.Distance(currentSensorPosition, currentDebrisInfo.lastKnownPosition);
                    
                    // Cast a ray from the Sensor-position to last known position
                    // If raycast from sensor to point doesn't get blocked, we know the debris is no longer there
                    if (!Physics.Raycast(ray, out RaycastHit hitInfo, distanceToLastKnownPos, layerMask))
                        currentDebrisInfo.lastKnownPosition = Vector3.positiveInfinity;
                }
            }

            // Replace struct with modified struct
            debrisInfos[i] = currentDebrisInfo;
        }

        return debrisInfos;
    }

    bool DirectionIsWithinVision(Vector3 direction)
    {
        // Calculate angle between forward direction and given direction
        // If it is less than 60 degrees true, else false
        return Vector3.Angle(sensorPosition.forward, direction.normalized) < visionAngle * 0.5f;
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            return;

        Gizmos.color = fovColor;
        
        Gizmos.DrawRay(sensorPosition.position, Quaternion.AngleAxis(visionAngle * -0.5f, Vector3.up) * transform.forward * 50f);
        Gizmos.DrawRay(sensorPosition.position, Quaternion.AngleAxis(visionAngle * 0.5f, Vector3.up) * transform.forward * 50f);
    }
    #endif
}

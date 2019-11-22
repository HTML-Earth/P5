using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotVision : MonoBehaviour
{
    public LayerMask layerMask;
    public Transform sensorPosition;

    public float visionAngle = 120f;

    RobotAcademy academy;

    Transform[] debrisArray;

    bool[] visibility;
    Vector3[] knownPositions;

    void Awake()
    {
        academy = FindObjectOfType<RobotAcademy>();
    }

    public void InitializeDebrisArray()
    {
        // Get list of Debris
        List<Transform> debrisList = academy.GetDebris();

        // Initialize array of Transform for every debris
        debrisArray = new Transform[debrisList.Count];
        // Initialize array of Bools for visibility of every debris
        visibility = new bool[debrisList.Count];
        // Initialize array of Vectors for positions of every debris
        knownPositions = new Vector3[debrisList.Count];

        // Put every debris in the list in the array
        // Give every debris a position of "positive infinity"
        for (int debris = 0; debris < debrisArray.Length; debris++)
        {
            debrisArray[debris] = debrisList[debris];
            knownPositions[debris] = Vector3.positiveInfinity;
        }
        
        UpdateVision();
    }

    public void UpdateVision()
    {
        if (debrisArray == null)
            InitializeDebrisArray();
        
        Vector3 currentSensorPosition = sensorPosition.position;
        
        // For every debris -> check if it is visible
        for (int debris = 0; debris < debrisArray.Length; debris++)
        {
            // Current debris-position
            Vector3 currentDebrisPosition = debrisArray[debris].position;

            // Vector from Sensor-position to current Debris-position
            Vector3 directionToDebris = currentDebrisPosition - currentSensorPosition;

            // Calculate angle between normalized Sensor-position and normalized Debris-position
            // If it is less than 60 degrees -> continue
            if (Vector3.Angle(sensorPosition.forward, directionToDebris.normalized) < visionAngle * 0.5f)
            {
                Ray ray = new Ray(currentSensorPosition, directionToDebris);

                // Cast a ray from the Sensor-position to Debris-position
                // If raycast from Robot to Debris gets blocked
                if (Physics.Raycast(ray, out RaycastHit hitInfo,
                    Vector3.Distance(currentSensorPosition, currentDebrisPosition), layerMask))
                    visibility[debris] = false;
                else
                    visibility[debris] = true;
            }
            else
            {
                visibility[debris] = false;
            }
        }
    }

    public bool[] GetVisibiilty()
    {
        return visibility;
    }

    // Return positions of visible debris
    public Vector3[] GetKnownPositions()
    {
        for (int debris = 0; debris < debrisArray.Length; debris++)
        {
            if (visibility[debris])
                knownPositions[debris] = debrisArray[debris].position;
        }

        return knownPositions;
    }
}

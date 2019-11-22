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
        List<Transform> debrisList = academy.GetDebris();

        debrisArray = new Transform[debrisList.Count];
        visibility = new bool[debrisList.Count];

        knownPositions = new Vector3[debrisList.Count];

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
        
        for (int debris = 0; debris < debrisArray.Length; debris++)
        {
            Vector3 currentDebrisPosition = debrisArray[debris].position;

            Vector3 directionToDebris = currentDebrisPosition - currentSensorPosition;

            if (Vector3.Angle(sensorPosition.forward, directionToDebris.normalized) < visionAngle * 0.5f)
            {
                Ray ray = new Ray(currentSensorPosition, directionToDebris);

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

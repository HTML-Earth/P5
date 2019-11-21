using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotVision : MonoBehaviour
{
    public LayerMask layerMask;
    public Transform sensorPosition;

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
            
            Ray ray = new Ray(currentSensorPosition, currentDebrisPosition - currentSensorPosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo,
                Vector3.Distance(currentSensorPosition, currentDebrisPosition), layerMask))
                visibility[debris] = false;
            else
                visibility[debris] = true;
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

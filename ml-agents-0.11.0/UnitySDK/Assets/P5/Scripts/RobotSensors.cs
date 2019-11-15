using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class RobotSensors : MonoBehaviour
{
    public LayerMask layerMask;
    public Transform sensorPosition;
    public int sensorAmount = 30;

    public bool printDebug;

    float anglePerSensor;

    Vector3[] sensorDirections;
    float[] measuredDistances;

    void Awake()
    {
        sensorDirections = new Vector3[sensorAmount];
        measuredDistances = new float[sensorAmount];
        anglePerSensor = 360f / sensorAmount;
    }

    void CalculateSensorDirections()
    {
        for (int i = 0; i < sensorAmount; i++)
        {
            // sensor direction is calculated by rotating the forward vector by (anglePerSensor * i) around the up axis
            sensorDirections[i] = Quaternion.AngleAxis(anglePerSensor * i, transform.up) * transform.forward;
        }
    }

    void FixedUpdate()
    {
        UpdateDistances();

        if (printDebug)
        {
            StringBuilder debugString = new StringBuilder();

            debugString.Append("DISTANCES:\n");
            
            foreach (float distance in measuredDistances)
                debugString.Append(distance + " ");

            Debug.Log(debugString);
        }
    }

    void OnDrawGizmos()
    {
        if (!EditorApplication.isPlaying)
            return;

        for (int i = 0; i < sensorAmount; i++)
        {
            if (measuredDistances[i] < Mathf.Infinity)
                Gizmos.color = Color.cyan;
            else
                Gizmos.color = Color.red;
            
            Gizmos.DrawRay(sensorPosition.position, sensorDirections[i].normalized * measuredDistances[i]);
        }
    }

    void UpdateDistances()
    {
        if (sensorAmount > 0)
        {
            CalculateSensorDirections();
            
            for (int i = 0; i < sensorAmount; i++)
            {
                Ray ray = new Ray(sensorPosition.position, sensorDirections[i]);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 9000f, layerMask))
                    measuredDistances[i] = hitInfo.distance;
                else
                    measuredDistances[i] = Mathf.Infinity;
            }
        }
    }

    public float[] GetMeasuredDistances()
    {
        return measuredDistances;
    }
}

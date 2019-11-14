using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RobotSensors : MonoBehaviour
{
    public LayerMask layerMask;
    public Transform sensorPosition;
    public int sensorAmount = 30;

    public bool printDebug;

    void FixedUpdate()
    {
        if (!printDebug)
            return;
        
        List<float> distances = GetEnvironmentDistances();
        StringBuilder debugString = new StringBuilder();

        debugString.Append("DISTANCES:\n");
        
        foreach (float distance in distances)
            debugString.Append(distance + " ");

        Debug.Log(debugString);
    }

    public List<float> GetEnvironmentDistances()
    {
        List<float> distances = new List<float>();

        if (sensorAmount > 0)
        {
            float anglePerSensor = 360f / sensorAmount;
            
            for (int i = 0; i < sensorAmount; i++)
            {
                // sensor direction is calculated by rotating the forward vector by (anglePerSensor * i) around the up axis
                Vector3 sensorDirection = Quaternion.AngleAxis(anglePerSensor * i, transform.up) * transform.forward;
                
                Ray ray = new Ray(sensorPosition.position, sensorDirection);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 9000f, layerMask))
                    distances.Add(hitInfo.distance);
                else
                    distances.Add(Mathf.Infinity);
            }
        }

        return distances;
    }
}

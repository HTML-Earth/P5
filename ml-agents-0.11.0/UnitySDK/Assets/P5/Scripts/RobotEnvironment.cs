using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotEnvironment : MonoBehaviour
{
    EnvironmentGeneration environmentGeneration;

    RobotAgent robot;
    
    DropZone dropZone;
    List<Debris> debrisInEnvironment;
    
    List<bool> previousDebrisInZone;
    List<bool> currentDebrisInZone;

    int iterations;
    
    public bool useRandomEnvironment;

    public float boundsMinimumX = -25f;
    public float boundsMaximumX = 25f;
    
    public float boundsMinimumY = -20f;
    public float boundsMaximumY = 100f;

    public float boundsMinimumZ = -25f;
    public float boundsMaximumZ = 25f;

    public bool randomizeDropZonePosition;

    public void InitializeEnvironment()
    {
        // Initialize variables
        environmentGeneration = transform.Find("Environment_Generation").GetComponent<EnvironmentGeneration>();
        dropZone = transform.Find("DropZone").GetComponent<DropZone>();
        robot = transform.Find("RobotAgent").GetComponent<RobotAgent>();

        InitializeDebrisList();
        
        currentDebrisInZone = new List<bool>();
        previousDebrisInZone = new List<bool>();
    }

    void InitializeDebrisList()
    {
        debrisInEnvironment = new List<Debris>();
        
        // Add all debris in environment to the list
        foreach (Debris debris in FindObjectsOfType<Debris>())
        {
            // If debris is child of environment
            if (debris.transform.parent.Equals(transform))
            {
                debrisInEnvironment.Add(debris);
                debris.UpdateStartPosition();
            }
        }
    }

    public void ResetEnvironment()
    {
        if (useRandomEnvironment)
        {
            // Generate environment
            environmentGeneration.GenerateEnvironment();

            // If environmentGeneration resets debris, reinitialize the list
            if (environmentGeneration.createNewDebris)
                InitializeDebrisList();
        }
        else // If random generation is disabled
        {
            // Reset debris positions
            foreach (Debris debris in debrisInEnvironment)
            {
                debris.transform.position = debris.GetStartPosition();
                debris.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
            
            // Reset robot position
            robot.ResetPosition();
        }

        // Update DropZone's debris list
        dropZone.SetDebrisList(debrisInEnvironment);

        if (randomizeDropZonePosition)
        {
            float xPos = Random.Range(boundsMinimumX, boundsMaximumX);
            float zPos = Random.Range(boundsMinimumZ, boundsMaximumZ);
            dropZone.transform.position = new Vector3(xPos, -0.24f, zPos);
        }
        
        // Initialize debrisInZone values
        foreach (Debris debris in debrisInEnvironment)
        {
            currentDebrisInZone.Add(dropZone.IsInZone(debris.transform.position));
        }
        previousDebrisInZone = currentDebrisInZone;
        
        ResetDebrisInZone();
        
        iterations++;
    }
    
    public void StepEnvironment()
    {
        // Copy currentDebrisInZone values to previousDebrisInZone
        previousDebrisInZone = new List<bool>();
        foreach (bool b in currentDebrisInZone)
        {
            previousDebrisInZone.Add(b);
        }

        // Update currentDebrisInZone values
        for (int debris = 0; debris < debrisInEnvironment.Count; debris++)
        {
            currentDebrisInZone[debris] = dropZone.IsInZone(debrisInEnvironment[debris].transform.position);
        }
    }
    
    public int GetIterations()
    {
        return iterations;
    }

    public List<Debris> GetDebris()
    {
        return debrisInEnvironment;
    }

    public List<bool> GetCurrentDebrisInZone()
    {
        return currentDebrisInZone;
    }

    public List<bool> GetPreviousDebrisInZone()
    {
        return previousDebrisInZone;
    }

    public void ResetDebrisInZone()
    {
        for (int debris = 0; debris < debrisInEnvironment.Count; debris++)
        {
            currentDebrisInZone[debris] = false;
            previousDebrisInZone[debris] = false;
        }
    }

    public DropZone GetDropZone()
    {
        return dropZone;
    }

    public Vector3 GetBoundsMinimum()
    {
        Vector3 currentPos = transform.position;
        return new Vector3(currentPos.x + boundsMinimumX, currentPos.y + boundsMinimumY, currentPos.z + boundsMinimumZ);
    }

    public Vector3 GetBoundsMaximum()
    {
        Vector3 currentPos = transform.position;
        return new Vector3(currentPos.x + boundsMaximumX, currentPos.y + boundsMaximumY, currentPos.z + boundsMaximumZ);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGeneration : MonoBehaviour
{
    float robot_x;
    float robot_z;
    float dropzone_x;
    float dropzone_z;
    
    public GameObject debrisPrefab;
    public Transform dropzone;
    public Transform robot;
    public GameObject wall;

    public void GenerateEnvironment()
    {
        PlaceRobot();
        GenerateDropzone();
        GenerateWalls();
        GenerateDebris();
    }

    // Robot start position and rotation
    private void PlaceRobot()
    {
        robot_x = Random.Range(0, 5);
        robot_z = Random.Range(0, 5);
        
        float x = -20f + robot_x * 8;
        float z = -20f + robot_z * 8;
        float randomRotation = Random.Range(0, 3) * 90f;
        robot.position = new Vector3(x, 0.9f, z);
        robot.rotation = Quaternion.Euler(0, randomRotation, 0);
    }

    private void GenerateWalls()
    {
    }

    private void GenerateDebris()
    {
    }

    // Dropzone position
    public void GenerateDropzone()
    {
        do
        {
            dropzone_x = Random.Range(0, 5);
            dropzone_z = Random.Range(0, 5);
        } while (robot_x == dropzone_x && robot_z == dropzone_z);

        float x = -20f + Random.Range(0, 5) * 8;
        float z = -20f + Random.Range(0, 5) * 8;
        dropzone.position = new Vector3(x, 0.2f, z);
    }
}

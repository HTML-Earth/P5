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
        robot_x = Random.Range(0, 6);
        robot_z = Random.Range(0, 6);
        
        float x = -20f + robot_x * 8;
        float z = -20f + robot_z * 8;
        float randomRotation = Random.Range(0, 4) * 90f;
        robot.position = new Vector3(x, 0.9f, z);
        robot.rotation = Quaternion.Euler(0, randomRotation, 0);
    }

    private void GenerateWalls()
    {
        int amount = Random.Range(4, 9);
        int x;
        int z;
        List<int> xList = new List<int>();
        List<int> zList = new List<int>();
        for (int i = 0; i < amount; i++)
        {
            do
            {
                x = Random.Range(0, 4);
                z = Random.Range(0, 4);
            } while ();
            var rotation = Random.Range(0, 2) * 90f;
            Instantiate(wall, new Vector3(-16f + x * 8, 2.375f, -16f + z * 8), Quaternion.Euler(0f, rotation, 0f));
            xList.Add(x);
            zList.Add(z);
        }
    }

    private void GenerateDebris()
    {
    }

    // Dropzone position
    public void GenerateDropzone()
    {
        do
        {
            dropzone_x = Random.Range(0, 6);
            dropzone_z = Random.Range(0, 6);
        } while (robot_x == dropzone_x && robot_z == dropzone_z);

        float x = -20f + Random.Range(0, 6) * 8; // -20 to +20
        float z = -20f + Random.Range(0, 6) * 8;
        dropzone.position = new Vector3(x, 0.2f, z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGeneration : MonoBehaviour
{

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

    private void PlaceRobot()
    {
        //float x = Random.Range(-20f, 20f);
        //float z = Random.Range(-20f, 20f);
        float x = -20f + Random.Range(0, 5) * 8;
        float z = -20f + Random.Range(0, 5) * 8;
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

    public void GenerateDropzone()
    {
        
    }
}

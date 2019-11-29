using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnvironmentGeneration : MonoBehaviour
{
    int robot_x;
    int robot_z;
    int dropzone_x;
    int dropzone_z;
    
    List<GameObject> environmentObjects = new List<GameObject>();
    
    private List<Transform> debrisList;
    
    public GameObject debrisPrefab;
    public Transform dropzone;
    public Transform robot;
    public GameObject wall;

    RobotVision robotVision;

    void Awake()
    {
        robotVision = robot.gameObject.GetComponent<RobotVision>();
    }

    public void GenerateEnvironment()
    {
        if (environmentObjects.Count > 0)
        {
            foreach (GameObject environmentObject in environmentObjects)
            {
                Destroy(environmentObject);
            }
            
            environmentObjects.Clear();
        }

        PlaceRobot();
        GenerateDropzone();
        GenerateWalls();
        GenerateDebris();
        
        robotVision.InitializeDebrisArray();
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
        int amount = Random.Range(8, 15);
        int x;
        int z;
        List<int> xList = new List<int>();
        List<int> zList = new List<int>();
        for (int i = 0; i < amount; i++)
        {
            do
            {
                x = Random.Range(0, 5);
                z = Random.Range(0, 5);
            } while (SetInLists(x, z, xList, zList));
            var rotation = Random.Range(0, 2) * 90f;
            GameObject obstacle_wall = Instantiate(wall, new Vector3(-16f + x * 8, 2.375f, -16f + z * 8), Quaternion.Euler(0f, rotation, 0f));
            
            environmentObjects.Add(obstacle_wall);
            
            xList.Add(x);
            zList.Add(z);
        }
    }

    private void GenerateDebris()
    {
        int debrisAmount = 6;
        int debrisX;
        int debrisZ;
        var randomsX = new List<int>();
        var randomsY = new List<int>();
        debrisList = new List<Transform>();
        
        for (int i = 0; i < debrisAmount; i++)
        {
            //Random rotation numbers
            var rotateX = Random.Range(0, 2) * 90;
            var rotateY = Random.Range(0, 2) * 90;
            
            //Random position numbers
            do
            {
                debrisX = Random.Range(0, 6);
                debrisZ = Random.Range(0, 6);
            } while (SetInLists(debrisX, debrisZ, randomsX, randomsY) | ((debrisX == dropzone_x && debrisZ == dropzone_z) | (debrisX == robot_x && debrisZ == robot_z)) );

            float x = -20f + debrisX * 8;
            float z = -20f + debrisZ * 8;
            GameObject debrisOBJ = Instantiate(debrisPrefab, new Vector3(x, 0.5f, z), Quaternion.Euler(rotateX, rotateY, 0f));
            debrisList.Add(debrisOBJ.transform);
            
            environmentObjects.Add(debrisOBJ);
            
            randomsX.Add(debrisX);
            randomsY.Add(debrisZ);
        }
    }

    //Return true if the number set is within the given list
    private bool SetInLists(int num1, int num2, List<int> numList1, List<int> numList2)
    {
        for (int i = 0; i < numList1.Count; i++)
        {
            if (num1.Equals(numList1[i]) && num2.Equals(numList2[i]))
                return true;
        }
        return false;
    }

    // Dropzone position
    public void GenerateDropzone()
    {
        do
        {
            dropzone_x = Random.Range(0, 6);
            dropzone_z = Random.Range(0, 6);
        } while (robot_x == dropzone_x && robot_z == dropzone_z);

        float x = -20f + dropzone_x * 8; // -20 to +20
        float z = -20f + dropzone_z * 8;
        dropzone.position = new Vector3(x, -0.24f, z);
    }

    public List<Transform> GetDebris()
    {
        return debrisList;
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnvironmentGeneration : MonoBehaviour
{
    int robotX;
    int robotZ;
    int dropZoneX;
    int dropZoneZ;
    
    List<GameObject> spawnedDebris = new List<GameObject>();
    List<GameObject> spawnedWalls = new List<GameObject>();

    [Header("Environment Generation Settings")]
    public bool createNewDebris = true;
    [Range(1,6)]
    public int debrisAmount = 6;

    public bool createNewWalls = true;
    [Range(1, 20)] public int wallMinAmount = 8;
    [Range(1, 20)] public int wallMaxAmount = 15;

    public bool placeRobot = true;
    public bool placeDropzone = true;

    public Vector2 environmentSize; //TODO: use this for placement

    [Header("References")]
    public Transform dropZone;
    public Transform robot;

    [Header("Prefabs")]
    public GameObject debrisPrefab;
    public GameObject wallPrefab;

    public void GenerateEnvironment()
    {
        if (createNewDebris && spawnedDebris.Count > 0)
        {
            foreach (GameObject debris in spawnedDebris)
            {
                Destroy(debris);
            }
            spawnedDebris.Clear();
        }

        if (createNewWalls && spawnedWalls.Count > 0)
        {
            foreach (GameObject wall in spawnedWalls)
            {
                Destroy(wall);
            }
            spawnedWalls.Clear();
        }
        
        if (placeRobot)
            PlaceRobot();
        
        if (placeDropzone)
            PlaceDropzone();
        
        if (createNewWalls)
            GenerateWalls();
        
        if (createNewDebris)
            GenerateDebris();
    }

    // Robot start position and rotation
    void PlaceRobot()
    {
        robotX = Random.Range(0, 6);
        robotZ = Random.Range(0, 6);

        float x = -20f + robotX * 8;
        float z = -20f + robotZ * 8;
        float randomRotation = Random.Range(0, 4) * 90f;
        robot.position = new Vector3(x, 0.9f, z);
        robot.rotation = Quaternion.Euler(0, randomRotation, 0);
    }

    void GenerateWalls()
    {
        int amount = Random.Range(wallMinAmount, wallMaxAmount);
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
            GameObject obstacleWall = Instantiate(wallPrefab, new Vector3(-16f + x * 8, 1.25f, -16f + z * 8), Quaternion.Euler(0f, rotation, 0f));
            
            spawnedWalls.Add(obstacleWall);
            
            xList.Add(x);
            zList.Add(z);
        }
    }

    void GenerateDebris()
    {
        int debrisAmount = 6;
        int debrisX;
        int debrisZ;
        var randomsX = new List<int>();
        var randomsY = new List<int>();
        
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
            } while (SetInLists(debrisX, debrisZ, randomsX, randomsY) | ((debrisX == dropZoneX && debrisZ == dropZoneZ) | (debrisX == robotX && debrisZ == robotZ)) );

            float x = -20f + debrisX * 8;
            float z = -20f + debrisZ * 8;
            
            GameObject debrisOBJ = Instantiate(debrisPrefab, new Vector3(x, 0.5f, z), Quaternion.Euler(rotateX, rotateY, 0f));
            debrisOBJ.GetComponent<Debris>().SetDebrisIndex(i);
            
            spawnedDebris.Add(debrisOBJ);
            
            randomsX.Add(debrisX);
            randomsY.Add(debrisZ);
        }
    }

    //Return true if the number set is within the given list
    bool SetInLists(int num1, int num2, List<int> numList1, List<int> numList2)
    {
        for (int i = 0; i < numList1.Count; i++)
        {
            if (num1.Equals(numList1[i]) && num2.Equals(numList2[i]))
                return true;
        }
        return false;
    }

    // Dropzone position
    public void PlaceDropzone()
    {
        do
        {
            dropZoneX = Random.Range(0, 6);
            dropZoneZ = Random.Range(0, 6);
        } while (robotX == dropZoneX && robotZ == dropZoneZ);

        float x = -20f + dropZoneX * 8; // -20 to +20
        float z = -20f + dropZoneZ * 8;
        dropZone.position = new Vector3(x, -0.24f, z);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGeneration : MonoBehaviour
{
    float robot_x;
    float robot_z;
    float dropzone_x;
    float dropzone_z;
    private List<GameObject> debrisList;

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
        int debrisAmount = 6;
        int debrisX;
        int debrisY;
        var randomsX = new List<int>();
        var randomsY = new List<int>();
        debrisList = new List<GameObject>();
        
        for (int i = 0; i < debrisAmount; i++)
        {
            //Random rotation numbers
            var rotateX = Random.Range(0, 2) * 90;
            var rotateY = Random.Range(0, 2) * 90;
            
            //Random position numbers
            do
            {
                debrisX = Random.Range(0, 6);
                debrisY = Random.Range(0, 6);
            } while (SetInLists(debrisX, debrisY, randomsX, randomsY));

            float x = -20f + debrisX * 8;
            float z = -20f + debrisY * 8;
            GameObject debrisOBJ = Instantiate(debrisPrefab, new Vector3(x, 0.5f, z), Quaternion.Euler(rotateX, rotateY, 0f));
            debrisList.Add(debrisOBJ);
            
            randomsX.Add(debrisX);
            randomsY.Add(debrisY);
        }
    }

    //Return true if the number set is not within the given list
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
            dropzone_x = Random.Range(0, 5);
            dropzone_z = Random.Range(0, 5);
        } while (robot_x == dropzone_x && robot_z == dropzone_z);

        float x = -20f + Random.Range(0, 5) * 8;
        float z = -20f + Random.Range(0, 5) * 8;
        dropzone.position = new Vector3(x, 0.2f, z);
    }

    public List<GameObject> GetDebris()
    {
        return debrisList;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisDetector : MonoBehaviour
{
    List<bool> debrisInArea;

    public void InitializeDetector()
    {
        debrisInArea = new List<bool>();
        for (int i = 0; i < 6; i++)
        {
            debrisInArea.Add(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debris debris = other.transform.parent.GetComponent<Debris>();
        if (debris != null)
        {
            debrisInArea[debris.GetDebrisIndex()] = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debris debris = other.transform.parent.GetComponent<Debris>();
        if (debris != null)
            debrisInArea[debris.GetDebrisIndex()] = false;
    }

    public List<bool> GetDebrisInArea()
    {
        return debrisInArea;
    }
}

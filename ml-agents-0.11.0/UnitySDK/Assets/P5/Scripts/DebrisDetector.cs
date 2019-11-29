using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisDetector : MonoBehaviour
{
    List<bool> debrisInShovel;

    public void InitializeDetector()
    {
        debrisInShovel = new List<bool>();
        for (int i = 0; i < 6; i++)
        {
            debrisInShovel.Add(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debris debris = other.transform.parent.GetComponent<Debris>();
        if (debris != null)
        {
            debrisInShovel[debris.GetDebrisIndex()] = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debris debris = other.transform.parent.GetComponent<Debris>();
        if (debris != null)
            debrisInShovel[debris.GetDebrisIndex()] = false;
    }

    public List<bool> GetDebrisInShovel()
    {
        return debrisInShovel;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float radius = 4f;

    List<Debris> debris;

    bool allDebrisInZone;

    void Awake()
    {
        transform.Find("circle_area").localScale = new Vector3(radius, radius, radius);
    }
    
    public void SetDebrisList(List<Debris> debrisList)
    {
        this.debris = debrisList;
        allDebrisInZone = false;
    }

    void FixedUpdate()
    {
        CheckDebrisInZone();
    }
    
    // Updates allDebrisInZone value
    void CheckDebrisInZone()
    {
        if (debris == null)
            return;
        
        allDebrisInZone = true;
        foreach (Debris d in debris)
        {
            if (!IsInZone(d.transform.position))
                allDebrisInZone = false;
        }
    }    

    public bool IsAllDebrisInZone()
    {
        return allDebrisInZone;
    }
    
    // Returns true if the given point is in the zone
    public bool IsInZone(Vector3 point)
    {
        return Vector3.Distance(transform.position, point) <= radius;
    }

    public float GetRadius()
    {
        return radius;
    }
    
}

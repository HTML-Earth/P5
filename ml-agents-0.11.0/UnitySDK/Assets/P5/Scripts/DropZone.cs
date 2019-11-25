using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float radius = 4f;

    List<Transform> debris;

    bool allDebrisInZone;

    void Awake()
    {
        transform.Find("circle_area").localScale = new Vector3(radius, radius, radius);
    }

    void FixedUpdate()
    {
        CheckDebrisInZone();
    }

    public float GetRadius()
    {
        return radius;
    }

    public bool IsInZone(Vector3 point)
    {
        return Vector3.Distance(transform.position, point) <= radius;
    }

    public void SetDebrisList(List<Transform> debris)
    {
        this.debris = debris;
        allDebrisInZone = false;
    }

    void CheckDebrisInZone()
    {
        if (debris == null)
            return;
        
        allDebrisInZone = true;
        foreach (Transform d in debris)
        {
            if (!IsInZone(d.transform.position))
                allDebrisInZone = false;
        }
    }

    public bool IsAllDebrisInZone()
    {
        return allDebrisInZone;
    }

}

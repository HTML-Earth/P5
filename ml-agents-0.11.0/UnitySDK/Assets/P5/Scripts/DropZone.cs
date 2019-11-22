using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float radius = 5f;

    List<Transform> debris;

    bool allDebrisInZone;

    void OnDrawGizmos()
    {
        Handles.color = new Color(1f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float radius = 5f;

    void OnDrawGizmos()
    {
        Handles.color = new Color(1f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
    }
    
    public float GetRadius()
    {
        return radius;
    }

}

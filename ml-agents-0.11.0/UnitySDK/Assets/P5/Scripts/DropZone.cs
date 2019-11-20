using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float radius = 5f;

    void OnDrawGizmos()
    {
        Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
    }

}

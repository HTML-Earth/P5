using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour
{
    public float length;
    public float minLength = 4.0f;
    public float maxLength = 8.0f;
    
    void Start()
    {
        RandomWallSize();
    }
    
    private void RandomWallSize()
    {
        length = Random.Range(minLength, maxLength);

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, length);
    }

}

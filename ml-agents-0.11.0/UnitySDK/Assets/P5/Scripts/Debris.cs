using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Debris : MonoBehaviour
{
    public int maxWidth = 10;
    public int maxLength = 10;
    public int maxHeight = 10;
    public float maxScale = 0.25f;
    private void Start()
    {
        RandomDebris();
    }
    
    private void RandomDebris()
    {
        float width = transform.localScale.x;
        float height = transform.localScale.y;
        float length = transform.localScale.z;

        float minWidth = Random.Range(0, maxWidth);
        float minHeight = Random.Range(0, maxHeight);
        float minLength = Random.Range(0, maxLength);
        float minScale = Random.Range(0, maxScale);
        
        do
        {
            minWidth = Random.Range(0, maxWidth);
            minHeight = Random.Range(0, maxHeight);
            minLength = Random.Range(0, maxLength);
            minScale = Random.Range(0, maxScale);
        } while (minWidth == 0 || minHeight == 0 || minLength == 0 || minScale == 0);

        width = Random.Range(minWidth, maxWidth) * Random.Range(0, maxScale);
        height = Random.Range(minHeight, maxHeight) * Random.Range(0, maxScale);
        length = Random.Range(minLength, maxLength) * Random.Range(0, maxScale);

        transform.localScale = new Vector3(width, length, length);
    }
}
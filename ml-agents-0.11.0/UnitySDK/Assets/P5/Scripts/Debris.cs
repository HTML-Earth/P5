using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Debris : MonoBehaviour
{
    /*
     *  this value tells 
     *  this value can be scaled between 0 to 1
     */
    
    public float variant_range = 0.6f;  
    
    private void Start()
    {
        RandomDebris();
    }
    
    private void RandomDebris()
    { 
        float minWidth = transform.localScale.x; 
        float maxWidth = minWidth + variant_range;
        
        float minHeight = transform.localScale.y;
        float maxHeight = minHeight + variant_range;
        
        float minLength = transform.localScale.z; 
        float maxLength = minLength + variant_range;
        
        // the unit for length, width and height is meter
        float width = Random.Range(minWidth, maxWidth);
        float height = Random.Range(minHeight, maxHeight);
        float length = Random.Range(minLength, maxLength);

        transform.localScale = new Vector3(width, height, length);
    }
}
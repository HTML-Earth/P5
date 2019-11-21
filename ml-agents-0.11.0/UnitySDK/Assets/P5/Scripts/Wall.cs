using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour
{
    public float width, length, depth;
    public int maxWidth = 10;
    public int maxLength = 10;
    public int maxDepth = 1;
    public float maxScale = 0.75f;
    
    // Start is called before the first frame update
    void Start()
    {
        RandomWallSize();
    }

    private void RandomWallSize()
    {
        width = Random.Range(1, maxWidth) * Random.Range(0, maxScale);
        length = Random.Range(1, maxLength) * Random.Range(0, maxScale);
        depth = Random.Range(1, maxDepth) /** Random.Range(0, maxScale)*/;
        
        transform.localScale = new Vector3(width, length, depth);        
    }

}

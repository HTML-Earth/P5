using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelControl : MonoBehaviour
{
    public Transform shovel;

    public float rotationSpeed = 50f;

    float minShovelRotation = 270;
    float maxShovelRotation = 360;

    float shovelInput = 0;

    float shovelPos;

    Quaternion shovelStartRotation;

    void Awake()
    {
        // Make the x rotation =/= 0
        shovel.Rotate(Vector3.right, -1f, Space.Self);

        shovelStartRotation = shovel.localRotation;
        
        UpdateShovelPos();
    }

    public void ResetRotations()
    {
        shovel.localRotation = shovelStartRotation;
        
        UpdateShovelPos();
    }

    public void RotateShovel(float direction)
    {
        shovelInput = direction;
    }

    void UpdateShovelPos()
    {
        
        shovelPos = shovel.localRotation.eulerAngles.x - minShovelRotation;
    }

    public float GetShovelPos()
    {
        return shovelPos;
    }

    void FixedUpdate()
    {
        float armRotation = 0;
        float shovelRotation = 0;

        float rotAmt = rotationSpeed * Time.deltaTime;

        float currentShovelRotation = shovel.localEulerAngles.x;

        if (shovelInput > 0)
            shovelRotation += rotAmt;
        else if (shovelInput < 0)
            shovelRotation -= rotAmt;
        
        //clamp the rotation
        if (currentShovelRotation + shovelRotation > minShovelRotation && currentShovelRotation + shovelRotation < maxShovelRotation)
            shovel.Rotate(Vector3.right, shovelRotation, Space.Self);

        UpdateShovelPos();
    }
}

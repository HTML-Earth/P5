using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelControl : MonoBehaviour
{
    public Transform arm;
    public Transform shovel;

    public float rotationSpeed = 50f;

    float minArmRotation = 270;
    float maxArmRotation = 360;
    
    float minShovelRotation = 290;
    float maxShovelRotation = 360;

    float armInput = 0;
    float shovelInput = 0;

    float armPos;
    float shovelPos;

    void Awake()
    {
        // Make the x rotation =/= 0
        arm.Rotate(Vector3.right, -1f, Space.Self);
        shovel.Rotate(Vector3.right, -1f, Space.Self);
        
        UpdateArmAndShovelPos();
    }
    
    public void RotateArm(float direction)
    {
        armInput = direction;
    }
    
    public void RotateShovel(float direction)
    {
        shovelInput = direction;
    }

    void UpdateArmAndShovelPos()
    {
        
        armPos = arm.localRotation.eulerAngles.x - minArmRotation;
        shovelPos = shovel.localRotation.eulerAngles.x - minShovelRotation;
    }

    public float GetArmPos()
    {
        return armPos;
    }

    public float GetShovelPos()
    {
        return shovelPos;
    }

    void Update()
    {
        float armRotation = 0;
        float shovelRotation = 0;

        float rotAmt = rotationSpeed * Time.deltaTime;

        float currentArmRotation = arm.localEulerAngles.x;
        float currentShovelRotation = shovel.localEulerAngles.x;

        if (armInput > 0)
            armRotation += rotAmt;
        else if (armInput < 0)
            armRotation -= rotAmt;

        if (shovelInput > 0)
            shovelRotation += rotAmt;
        else if (shovelInput < 0)
            shovelRotation -= rotAmt;
        
        //clamp the rotation
        if (currentArmRotation + armRotation > minArmRotation && currentArmRotation + armRotation < maxArmRotation)
            arm.Rotate(Vector3.right, armRotation, Space.Self);

        //clamp the rotation
        if (currentShovelRotation + shovelRotation > minShovelRotation && currentShovelRotation + shovelRotation < maxShovelRotation)
            shovel.Rotate(Vector3.right, shovelRotation, Space.Self);

        UpdateArmAndShovelPos();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelControl : MonoBehaviour
{
    public Transform arm;
    public Transform shovel;

    float rotationSpeed = 50f;

    float minArmRotation = -120;
    float maxArmRotation = 0;
    
    void Update()
    {
        
        float armRotation = 0;
        float shovelRotation = 0;

        float rotAmt = rotationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            armRotation += rotAmt;
        else if (Input.GetKey(KeyCode.E))
            armRotation -= rotAmt;
        
        if (Input.GetKey(KeyCode.Z))
            shovelRotation -= rotAmt;
        else if (Input.GetKey(KeyCode.X))
            shovelRotation -= rotAmt;

        //clamp the rotation
        //armRotation = Mathf.Clamp(armRotation, arm.localRotation.x - minArmRotation,arm.localRotation.x - maxArmRotation);
        arm.Rotate(Vector3.right, armRotation, Space.Self);
        shovel.Rotate(Vector3.right, shovelRotation, Space.Self);
    }
}

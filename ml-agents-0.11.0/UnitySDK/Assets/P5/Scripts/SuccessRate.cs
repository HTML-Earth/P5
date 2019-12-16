using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SuccessRate : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAgent robotAgent;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAgent = FindObjectOfType<RobotAgent>();
        text.text = "Success rate: 0";
    }

    // Update is called once per frame
    void Update()
    //Shows the success rate of the last 100 attempts
    {
        if (robotAgent.GetTimesDone() == 0)
            return;
        
        float successRate = (((robotAgent.GetTimesWon()+0f) / (robotAgent.GetTimesDone()+0f)) * 100f);
        text.text = "Success rate: " + (successRate.ToString("0.00") + "%");
    }
}

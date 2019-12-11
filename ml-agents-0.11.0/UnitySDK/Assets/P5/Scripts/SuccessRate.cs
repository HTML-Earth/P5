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
        List<bool> lastHundredAttempts = robotAgent.GetLastHundredAttemptsList();
        int successFullAttempts = 0;

        foreach (var attempt in lastHundredAttempts)
        {
            if (attempt)
            {
                successFullAttempts++;
            }
        }

        if (lastHundredAttempts.Count > 0)
        {
            float successRate = (((successFullAttempts+0f) / (lastHundredAttempts.Count+0f)) * 100f);
            text.text = "Success rate: " + (successRate.ToString("0.00") + "%");
        }
    }
}

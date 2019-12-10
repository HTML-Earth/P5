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
    {
        List<bool> lastTenAttempts = robotAgent.GetLastTenAttemptsList();
        int successFullAttempts = 0;

        foreach (var attempt in lastTenAttempts)
        {
            if (attempt)
            {
                successFullAttempts++;
            }
        }

        if (lastTenAttempts.Count > 0)
        {
            text.text = "Success rate:" + successFullAttempts / (lastTenAttempts.Count + 0f);
        }

    }
}

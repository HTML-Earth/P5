using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayRewards : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAgent robotAgent;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAgent = FindObjectOfType<RobotAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        text.text = "Rewards:\n"
                    + robotAgent.GetReward().ToString("0.000")
                    + " (step)\n"
                    + robotAgent.GetCumulativeReward().ToString("0.00")
                    + " (episode)\n"
                    + robotAgent.GetTotalReward().ToString("0.00")
                    + " (total)";
    }
}

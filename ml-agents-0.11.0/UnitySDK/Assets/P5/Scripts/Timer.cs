using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAgent robotAgent;

    float theTime;
    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAgent = FindObjectOfType<RobotAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        theTime = robotAgent.GetElapsedTime();
        string minutes = Mathf.Floor((theTime % 3600) / 60).ToString("00");
        string seconds = (theTime % 60).ToString("00.##");
        text.text = minutes + ":" + seconds;
    }
}

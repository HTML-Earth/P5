using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DisplayActions : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAgent robotAgent;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAgent = FindObjectOfType<RobotAgent>();
    }

    void FixedUpdate()
    {
        float[] actions = robotAgent.GetActionVector();

        if (actions != null)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < actions.Length; i++)
            {
                sb.Append(actions[i].ToString("0.#"));
                sb.Append("\n");
            }

            text.text = sb.ToString();
        }
    }
}

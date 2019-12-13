using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayActions : MonoBehaviour
{
    TextMeshProUGUI text;
    RobotAgent robotAgent;

    Image up;
    Image down;
    Image left;
    Image right;
    
    Image armUp;
    Image armDown;
    
    Image shovelUp;
    Image shovelDown;

    Color opaque = Color.white;
    Color transparent = new Color(1,1,1,0.5f);

    public bool useTextInsteadOfGraphics;

    void Awake()
    {
        if (useTextInsteadOfGraphics)
            text = GetComponent<TextMeshProUGUI>();
        else
        {
            up = transform.Find("up").GetComponent<Image>();
            down = transform.Find("down").GetComponent<Image>();
            left = transform.Find("left").GetComponent<Image>();
            right = transform.Find("right").GetComponent<Image>();
            
            shovelUp = transform.Find("shovel_up").GetComponent<Image>();
            shovelDown = transform.Find("shovel_down").GetComponent<Image>();
        }
        robotAgent = FindObjectOfType<RobotAgent>();
    }

    void FixedUpdate()
    {
        if (useTextInsteadOfGraphics)
            DisplayActionsText();
        else
            DisplayActionsGraphical();
    }

    void DisplayActionsGraphical()
    {
        float[] actions = robotAgent.GetActionVector();

        if (actions != null)
        {
            up.color    = (actions[0] == 2)  ? opaque : transparent;
            down.color  = (actions[0] == 1) ? opaque : transparent;
            left.color  = (actions[1] == 1) ? opaque : transparent;
            right.color = (actions[1] == 2)  ? opaque : transparent;
            
            shovelUp.color   = (actions[2] == 1)  ? opaque : transparent;
            shovelDown.color = (actions[2] == 2) ? opaque : transparent;
        }
    }
    
    void DisplayActionsText()
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

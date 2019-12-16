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
    
    Image zero;
    
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
            
            zero = transform.Find("zero").GetComponent<Image>();
            
            shovelUp = transform.Find("shovel_up").GetComponent<Image>();
            shovelDown = transform.Find("shovel_down").GetComponent<Image>();
        }
        robotAgent = FindObjectOfType<RobotAgent>();
        
        shovelUp.color   = transparent;
        shovelDown.color = transparent;
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
        if (robotAgent.GetActionVector() == null)
            return;
        
        int action = (int)robotAgent.GetActionVector()[0];

        zero.color  = (action == 0) ? opaque : transparent;
        up.color  = (action == 1) ? opaque : transparent;
        left.color = (action == 2)  ? opaque : transparent;
        down.color    = (action == 3)  ? opaque : transparent;
        right.color  = (action == 4) ? opaque : transparent;

        //if (actions.Length > 2)
        //{
        //    int shov = robotAgent.ConvertAction((int) actions[2]);
        //    
        //    shovelUp.color   = (shov == 1)  ? opaque : transparent;
        //    shovelDown.color = (shov == 2) ? opaque : transparent;
        //}
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

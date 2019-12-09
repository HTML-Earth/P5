using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayRewards : MonoBehaviour
{
    public TextMeshProUGUI lastRewardText;

    public GameObject rewardPopup;
    
    TextMeshProUGUI text;
    RobotAgent robotAgent;
    
    RectTransform canvasRect;
    
    Color rewardColor = new Color(0, 1, 1, 0.5f);
    Color penaltyColor = new Color(1, 0, 0, 0.5f);

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        robotAgent = FindObjectOfType<RobotAgent>();
        
        canvasRect = transform.parent.GetComponent<RectTransform>();
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

    public void DisplayReward(float rewardValue, string rewardString, Vector3 position)
    {
        Color textColor = (rewardValue < 0) ? penaltyColor : rewardColor;
        
        GameObject popupObj = Instantiate(rewardPopup, transform.parent);
        
        TextMeshProUGUI popupText = popupObj.GetComponent<TextMeshProUGUI>();
        popupText.text = ((rewardValue < 0) ? "" : "+") + rewardValue + " (" + rewardString + ")";
        popupText.color = textColor;

        RectTransform popupTransform = popupObj.GetComponent<RectTransform>();
        
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(position);
        Vector2 screenPosition=new Vector2(
            ((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));
 
        popupTransform.anchoredPosition = screenPosition;

        StartCoroutine(UpdateText(popupObj, popupTransform, popupText, textColor));
        //Destroy(popupObj, 0.5f);
    }

    IEnumerator UpdateText(GameObject obj, RectTransform rectTransform, TextMeshProUGUI txt, Color baseColor)
    {
        txt.color = baseColor;
        float timeLeft = 1f;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            
            rectTransform.Translate(Time.deltaTime * 50f * Vector3.up);
            
            txt.color = new Color(baseColor.r, baseColor.g, baseColor.b, timeLeft);
            
            yield return null;
        }
        Destroy(obj);
    }
}

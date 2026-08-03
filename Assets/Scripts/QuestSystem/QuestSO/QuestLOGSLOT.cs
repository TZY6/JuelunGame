using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestLOGSLOT : MonoBehaviour
{
    public TMP_Text questNameText;
    public TMP_Text questLevelText;
    public QuestSO currentQuestSO;
    public void SetQuest(QuestSO questSO=null)
    {
        currentQuestSO = questSO;
        questNameText.text = questSO.QuestName;
        questLevelText.text = "LV:"+questSO.QuestLevel.ToString();
        gameObject.SetActive(true);
    }
    public void ClearSlot()
    {
        currentQuestSO = null;
        gameObject.SetActive(false);
    }
    public void OnValidate()
    {
        if (currentQuestSO != null)
        {
            SetQuest(currentQuestSO);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void OnSlotClicked()
    {        
        QuestLogUI.instance.HandleQuestClicked(currentQuestSO);
    }

}

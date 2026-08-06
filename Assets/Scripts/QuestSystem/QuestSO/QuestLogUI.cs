using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    public static QuestLogUI instance {  get; private set; }
    public QuestSO currentCheckQuest;
    public List<QuestSO> availableQuests = new List<QuestSO>();
    public QuestLOGSLOT[] questLOGSLOT;
    public QuestObjSlot[] ObjSlots;
    public QuestRewardSlot[] RewardSlots;
    //public List<QuestSO> activeQuest;
    [Header("任务UI")]
    public TMP_Text QuestNameText;
    public TMP_Text QuestDescriptionText;
    [SerializeField] private CanvasGroup questCanvas;
    [SerializeField] private CanvasGroup acceptCanvas;
    //[SerializeField] private CanvasGroup declineCanvas;
    [SerializeField] private CanvasGroup completeCanvas;
    public QuestSO noAvailableQuest;
    //public TMP_Text QuestObjectiveText;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void OnEnable()
    {
        QuestEvent.OnQuestOfferRequested += AddQuestOffer;      
    }
    private void OnDisable()
    {
        QuestEvent.OnQuestOfferRequested -= AddQuestOffer;     
    }
    public void AddQuestOffer(QuestSO newQuestSO)       //  新增可被接取的任务
    {        
        for (int i = 0; i < questLOGSLOT.Length; i++)
        {
            if (questLOGSLOT[i].currentQuestSO != null)
                continue;
            else
            {
                questLOGSLOT[i].currentQuestSO = newQuestSO;
                questLOGSLOT[i].SetQuest(newQuestSO);
                questLOGSLOT[i].gameObject.SetActive(true);
                break;
            }
        }
        availableQuests.Add(newQuestSO);
    }
    public void ShowQuestOffer(QuestSO questSO)  //展示左侧任务界面
    {
        RefreshQuestList();
            if (QuestManager.instance.IsQuestAccepted(questSO) || QuestManager.instance.isQuestCompleted(questSO))      //如果该任务已被接受 或之前已经完成
            {
                currentCheckQuest = noAvailableQuest;
            }
            else
            {
                currentCheckQuest = questSO;
            }
            HandleQuestClicked(currentCheckQuest);
            SetCanvasGroupActive(questCanvas, true);
             
                
    }
    public void OnAcceptQuestClicked()      //点击接收任务按钮
    {
        QuestManager.instance.AcceptQuest(currentCheckQuest);
        SetCanvasGroupActive(completeCanvas, false);
        SetCanvasGroupActive(acceptCanvas, false);
        availableQuests.Remove(currentCheckQuest);
    }
    public void OnDeclineQuestClicked()     //点击关闭任务面板
    {
        SetCanvasGroupActive(questCanvas, false);

    }
    public void OnCompleteQuestClicked()      //点击任务完成
    {
        Debug.Log($"已提交{currentCheckQuest.QuestName}");
        QuestManager.instance.CompleteQuest(currentCheckQuest);
        RefreshQuestList();
        HandleQuestClicked(noAvailableQuest);
        SetCanvasGroupActive(completeCanvas, false);
        currentCheckQuest = noAvailableQuest;
    }
    public void RefreshQuestList()          //刷新任务log页面
    {
        List<QuestSO> activeList = QuestManager.instance.GetActiveQuests();
        List<QuestSO> allQuests = new List<QuestSO>(activeList);
        allQuests.AddRange(availableQuests);
        allQuests.RemoveAll(q => q == null || q == noAvailableQuest); // 过滤无效

        int slotCount = questLOGSLOT.Length;
        for (int i = 0; i < slotCount; i++)
        {
            if (i < allQuests.Count)
            {
                questLOGSLOT[i].SetQuest(allQuests[i]);
                questLOGSLOT[i].gameObject.SetActive(true);
            }
            else
            {
                questLOGSLOT[i].ClearSlot();
                questLOGSLOT[i].gameObject.SetActive(false);
            }
        }
    }
    public void SetCanvasGroupActive(CanvasGroup canvasGroup, bool state)
    {
        canvasGroup.alpha = state ? 1 : 0;
        canvasGroup.blocksRaycasts = state;
        canvasGroup.interactable = state;
    }
    public void HideQuestOffer()
    {
        SetCanvasGroupActive(questCanvas, false);
        
    }
    #region 任务选中后逻辑
    public void HandleQuestClicked(QuestSO questSO=null)         //选中具体任务， 展示右侧任务界面
    {
        if(questSO == null)
        {
            currentCheckQuest = null;
            QuestNameText.text = "选择任务后查看";
            QuestDescriptionText.text = "选择任务后查看";
            return;
        }
        SetCanvasGroupActive(completeCanvas, false);
        if (!QuestManager.instance.IsQuestAccepted(questSO))    //  如果任务还没有接受，开启“接受”按钮
        {
            SetCanvasGroupActive(acceptCanvas, true);
        }
        else                                                    //  如果任务已经接受，隐藏“接受”按钮
            SetCanvasGroupActive(acceptCanvas, false);
        if (questSO.QuestName == "无待接任务")
            SetCanvasGroupActive(acceptCanvas, false); ;        //  如果是假任务，隐藏“接受”按钮
        if (QuestManager.instance.HandleQuestComplete(questSO))
        {
            SetCanvasGroupActive(completeCanvas, true);
        }
        currentCheckQuest = questSO;
        QuestNameText.text = questSO.QuestName;
        QuestDescriptionText.text = questSO.QuestDescription;
        DisplayObjective(questSO);
        DisplayRewards(questSO);

        foreach (var objective in questSO.objectives)
        {
            
            Debug.Log($"{objective.description}===="+QuestManager.instance.GetProgressText(questSO, objective));
            List<QuestSO> log = QuestManager.instance.GetActiveQuests();
            foreach(var l in log)
            {
                Debug.Log(l.QuestName);
            }
        }
    }
    public void DisplayObjective(QuestSO questSO = null)         //显示任务目标清单
    {
        if (questSO == null)
        {
            for (int i = 0; i < ObjSlots.Length; i++)
            {
                ObjSlots[i].GetComponentInChildren<TMP_Text>().text = "";
            }
            return;
        }
        for(int i = 0; i < ObjSlots.Length; i++)
        {
            if(i<questSO.objectives.Count)
            {
                ObjSlots[i].gameObject.SetActive(true);
                QuestManager.instance.UpdateObjProgress(questSO, questSO.objectives[i]);
                ObjSlots[i].GetComponent<QuestObjSlot>().RefreshObj(questSO.objectives[i].description, $"{QuestManager.instance.GetCurrentAmount(questSO, questSO.objectives[i])}/{questSO.objectives[i].RequiredAmount}" , QuestManager.instance.GetCurrentAmount(questSO, questSO.objectives[i]) == questSO.objectives[i].RequiredAmount);
            }
            else
            {
                ObjSlots[i].gameObject.SetActive(false);
            }
        }
    }
    public void DisplayRewards(QuestSO questSO)                  //显示战利品
    {
        for(int i = 0;i < RewardSlots.Length;i++)
        {
            if(i<questSO.rewards.Count)
            {
                RewardSlots[i].gameObject.SetActive(true);
                RewardSlots[i].DisplayReward(questSO.rewards[i].ItemSO.icon, questSO.rewards[i].amount);
            }
            else
            {
                RewardSlots[i].gameObject.SetActive(false);
            }
        }
    }
    #endregion
}

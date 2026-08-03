using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }
    public CanvasGroup QuestMainCG;
     private Dictionary<QuestSO, Dictionary<QuestObjective, int>> questProgress = new();        //存储任务完成情况的字典
    private List<QuestSO> completedQuest = new List<QuestSO> ();
    public bool isQuestLogOpen;
    private void OnEnable()
    {
        QuestEvent.isQuestComplete += HandleQuestComplete;
    }
    private void OnDisable()
    {
        QuestEvent.isQuestComplete -= HandleQuestComplete;
    }
    public void MakeQuestComplete(QuestSO questSO)   //强制完成某个任务(默认只有一个任务目标)
    {
        questProgress[questSO][questSO.objectives[0]] = questSO.objectives[0].RequiredAmount;
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    #region 接取任务
    private void Start()
    {
        AcceptQuest(QuestLogUI.instance.noAvailableQuest);
    }
    public bool IsQuestAccepted(QuestSO questSO)
    {
        if (questSO == null)
            return false; // 或者根据业务逻辑决定是否抛出异常

        return questProgress.ContainsKey(questSO);
    }
    public void AcceptQuest(QuestSO questSO)
    {
        questProgress[questSO] = new Dictionary<QuestObjective, int>();
        foreach(var objective in questSO.objectives)
        {
            UpdateObjProgress(questSO, objective);
        }
        Debug.Log($"成功接取任务{questSO.QuestName}");
    }
    public List<QuestSO> GetActiveQuests()
    {
        return new List<QuestSO>(questProgress.Keys);       //返回当前处于“接受”状态下的任务，而接受的任务会储存在管理器的字典中
    }
    #endregion
    #region 完成任务
    public bool HandleQuestComplete(QuestSO questSO)
    {
        if (!questProgress.TryGetValue(questSO, out Dictionary<QuestObjective, int> progressDict))       //还没有接取该任务
        {
            return false;
        }
        foreach (var obj in questSO.objectives)
        {
            UpdateObjProgress(questSO, obj);     //更新已接取任务的进度
        }
        foreach (var obj in questSO.objectives)
        {
            if (progressDict[obj] < obj.RequiredAmount)         //如果存在任务目标未完成
                return false;
        }
        return true;
    }
    public void CompleteQuest(QuestSO questSO)          //任务完成奖励结算
    {
        questProgress.Remove(questSO);
        completedQuest.Add(questSO);
        foreach(var reward in questSO.rewards)
        {
            InventoryManager.Instance.AddItem(reward.ItemSO, reward.amount);
        }
    }   
    public bool isQuestCompleted(QuestSO questSO)
    {
        return completedQuest.Contains(questSO);
    }
    #endregion
    private void OnValidate()
    {                
        if(!isQuestLogOpen)
        {
            QuestMainCG.alpha = 0;
            QuestMainCG.blocksRaycasts = false;
            QuestMainCG.interactable = false;
        }
        else
        {
            QuestMainCG.alpha = 1;
            QuestMainCG.blocksRaycasts = true;
            QuestMainCG.interactable = true;
        }
    }
    public void UpdateObjProgress(QuestSO questSO, QuestObjective obj)  //  更新任务进度
    {
        if(!questProgress.ContainsKey(questSO))
        {
            //questProgress[questSO] = new Dictionary<QuestObjective, int>();
            Debug.Log("当前任务未接收");
            return;
        }
        var progressDictionary = questProgress[questSO];//暂存在内存中
        int newAmount = 0;
        if(obj.targetItem!=null)//如果目标是获取物品
        {
            newAmount = InventoryManager.Instance.GetItemCount(obj.targetItem);
        }
        else if(obj.targetLocation!=null && LocationHistoryTracker.instance.HasArrived(obj.targetLocation))//如果目标是某个地点
        {
            newAmount = obj.RequiredAmount;
        }
        else if(obj.targetActor!=null && DialogueHistoryTracker.instance.HasSpokenWith(obj.targetActor))//如果目标是跟某个角色对话
        {
            newAmount = obj.RequiredAmount;
        }
        progressDictionary[obj] = newAmount;
    }
    public string GetProgressText(QuestSO questSO, QuestObjective obj)
    {
        if(IsQuestAccepted(questSO))
        {
            int currentAmount = GetCurrentAmount(questSO, obj);
            if (currentAmount >= obj.RequiredAmount)
            {
                return "任务完成";
            }
            else if (obj.targetItem != null)
            {
                return $"现有数量{GetCurrentAmount(questSO, obj)}——目标数量{obj.RequiredAmount}";
            }
            else
            {
                return "任务仍在进行中";
            }
        }
        else
        {
            return "任务未接取";
        }
    }
    public int GetCurrentAmount(QuestSO questSO, QuestObjective obj)
    {
        if(questProgress.TryGetValue(questSO, out var objetiveDictionay ))
            if(objetiveDictionay.TryGetValue(obj,out int amount))
                return amount;
        return 0;       //任务不存在或完成量为0
    }
    private void Update()
    {
        
    }
    public bool isQuestOpen()
    {
        return QuestMainCG.alpha == 1;
    }
}
 
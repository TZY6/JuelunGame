using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "任务SO", menuName ="任务/新增任务")]
public class QuestSO : ScriptableObject
{
    public string QuestName;
    [TextArea]public string QuestDescription;
    public int QuestLevel;
    public List<QuestObjective> objectives;
    public List<QuestReward> rewards;
}
[Serializable]
public class QuestObjective
{
    public string description;
    public ItemSO targetItem;
    public ActorSO targetActor;
    public LocationSO targetLocation;

    public int RequiredAmount;
    public int CurrentAmount;
}
[Serializable]
public class QuestReward
{
    public ItemSO ItemSO;
    public int amount;

}
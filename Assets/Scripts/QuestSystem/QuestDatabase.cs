using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
    public static QuestDatabase Instance;

    public List<QuestSO> quests;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public QuestSO GetQuestByID(string id)
    {
        foreach (QuestSO quest in quests)
        {
            if (quest.QuestID == id)
                return quest;
        }

        return null;
    }
}

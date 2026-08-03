using UnityEngine;

public class GetNewQuest : MonoBehaviour
{
    public static GetNewQuest instance {  get; private set; } 
    public QuestSO[] NewQuestsAbleToAccept;
   [Range(1,6)] public int QuestIndexToCreat;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        //AddCurrentQuest();
    }
    [ContextMenu("新建任务")]
    public void EditorCreatQuest()
    {
        QuestSO questSO = NewQuestsAbleToAccept[QuestIndexToCreat - 1];
        AddCurrentQuest(questSO);
    }
    public void AddCurrentQuest(QuestSO questSO)    //加入到任务列表显示中
    {
        QuestEvent.OnQuestOfferRequested?.Invoke(questSO);
    }
}

using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }
    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        savePath = Application.persistentDataPath + "/save.json";
    }

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();

        data.playerData = DataManager.instance.GetSaveData();

        data.inventoryData = InventoryManager.Instance.GetSaveData();

        data.expData = ExpManager.instance.GetSaveData();

        data.questData = QuestManager.instance.GetSaveData();

        data.dialogueHistoryData = DialogueHistoryTracker.instance.GetSaveData();

        data.locationHistoryData = LocationHistoryTracker.instance.GetSaveData();

        data.availablePoints = SkillTreeManager.instance.availablePoints;

        data.saveTime = DateTime.Now.ToString();


        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(savePath, json);

        Debug.Log("游戏保存成功:" + savePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("没有找到存档文件");
            return;
        }

        string json = File.ReadAllText(savePath);

        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null)
        {
            Debug.LogWarning("存档数据为空");
            return;
        }


        DataManager.instance.LoadData(data.playerData);

        InventoryManager.Instance.LoadData(data.inventoryData);

        ExpManager.instance.LoadData(data.expData);

        QuestManager.instance.LoadData(data.questData);

        DialogueHistoryTracker.instance.LoadData(data.dialogueHistoryData);

        LocationHistoryTracker.instance.LoadData(data.locationHistoryData);


        SkillTreeManager.instance.availablePoints = data.availablePoints;


        Debug.Log("游戏读取成功");
    }

    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    public void DeleteSaveData()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("存档删除成功");
        }
    }
}

[Serializable]
public class GameSaveData
{
    public int availablePoints;

    public ExpData expData;
    public InventoryData inventoryData;
    public PlayerData playerData;
    public QuestData questData;
    public DialogueHistoryData dialogueHistoryData;
    public LocationHistoryData locationHistoryData;

    public string saveTime;
}
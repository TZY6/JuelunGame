using System;
using System.IO;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }
    private string saveDirectory;          // 存档目录（不含文件名）
    public CanvasGroup SaveSlotCanvasGroup;
    public SaveSlot[] SaveSlots;
    public CanvasGroup SaveConfirm;
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

        saveDirectory = Application.persistentDataPath;
    }
    public void showSaveSlot()
    {
        QuestLogUI.instance.SetCanvasGroupActive(SaveSlotCanvasGroup, true);
        if (SaveSlots.Length > 0)
        {
            RefreshAllSaveSlots(SaveSlots);
        }
        else
        {
            Debug.LogWarning("没有找到 SaveSlot 组件，请确保 UI 中有对应脚本");
        }
    }
    public void RefreshAllLoadSlots(LoadSlot[] slots)
    {
        foreach (LoadSlot slot in slots)
        {
            int index = slot.LoadSlotIndex; // 假设 LoadSlot 也有 SaveSlotIndex
            string fileName = "save" + index;
            string path = GetSavePath(fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                if (data != null && !string.IsNullOrEmpty(data.saveTime))
                {
                    slot.UpdateSlotDisplay(hasSave: true, dateTimeString: data.saveTime);
                }
                else
                {
                    slot.UpdateSlotDisplay(hasSave: false);
                }
            }
            else
            {
                slot.UpdateSlotDisplay(hasSave: false);
            }
        }
    }
    public void RefreshAllSaveSlots(SaveSlot[] slots)
    {
        foreach (SaveSlot slot in slots)
        {
            int index = slot.SaveSlotIndex;
            string fileName = "save" + index;          // 对应存档文件 save1, save2, ...
            string path = GetSavePath(fileName);

            if (File.Exists(path))
            {
                // 读取 JSON 并提取 saveTime
                string json = File.ReadAllText(path);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                if (data != null && !string.IsNullOrEmpty(data.saveTime))
                {
                    // 解析存档中的保存时间（字符串）
                    // 注意：这里不需要转换回 DateTime，直接传字符串让 UI 格式化即可
                    slot.UpdateSlotDisplay(hasSave: true, dateTimeString: data.saveTime);
                }
                else
                {
                    slot.UpdateSlotDisplay(hasSave: false);
                }
            }
            else
            {
                slot.UpdateSlotDisplay(hasSave: false);
            }
        }
    }
    // ---------- 核心方法：支持自定义文件名 ----------
    private string GetSavePath(string fileName)
    {
        // 如果 fileName 不包含扩展名，自动添加 .json
        if (!Path.HasExtension(fileName))
            fileName += ".json";
        return Path.Combine(saveDirectory, fileName);
    }
    public void ConfirmSaveGame(string fileName = null)
    {
        if(fileName==null)
        {
            QuestLogUI.instance.SetCanvasGroupActive(SaveConfirm, false); //关闭确认页面
            return;
        }     
        
        SaveGame(fileName, true);
        QuestLogUI.instance.SetCanvasGroupActive(SaveConfirm, false); //关闭确认页面
    }

    public void SaveGame(string fileName, bool overwrite = false)
    {
        string path = GetSavePath(fileName);

        if (File.Exists(path) && !overwrite)
        {
            Debug.Log($"存档文件已存在，保存取消: {path}");
            QuestLogUI.instance.SetCanvasGroupActive(SaveConfirm, true);

            Button saveButton = SaveConfirm.transform.GetChild(1).GetComponent<Button>();
            saveButton.onClick.RemoveAllListeners(); // 清除所有旧监听
            saveButton.onClick.AddListener(() => ConfirmSaveGame(fileName));

            Button cancelButton = SaveConfirm.transform.GetChild(2).GetComponent<Button>();
            cancelButton.onClick.RemoveAllListeners(); // 清除所有旧监听
            cancelButton.onClick.AddListener(() => ConfirmSaveGame());
            return;
        }

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
        File.WriteAllText(path, json);
        Debug.Log($"游戏保存成功: {path}");
        RefreshAllSaveSlots(SaveSlots);
    }

    public void LoadGame(string fileName)
    {
        string path = GetSavePath(fileName);
        if (!File.Exists(path))
        {
            Debug.Log($"没有找到存档文件: {path}");
            return;
        }

        string json = File.ReadAllText(path);
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

        Debug.Log($"游戏读取成功: {path}");

    }

    public bool HasSaveData(string fileName)
    {
        string path = GetSavePath(fileName);
        return File.Exists(path);
    }

    public void DeleteSaveData(string fileName)
    {
        string path = GetSavePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"存档删除成功: {path}");
        }
    }

    // ---------- 兼容旧版本的无参方法（默认使用 "save" 作为文件名） ----------
    //public void SaveGame()
    //{
    //    SaveGame("save");
    //}

    public void LoadGame()
    {
        LoadGame("save");
    }

    public bool HasSaveData()
    {
        return HasSaveData("save");
    }

    //public void DeleteSaveData()
    //{
    //    DeleteSaveData("save");
    //}
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
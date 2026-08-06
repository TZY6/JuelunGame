using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[DisallowMultipleComponent]
public class ExpManager : MonoBehaviour
{
    public static ExpManager instance {  get; private set; }
    public int Level;
    public int currentExp;
    public int ExpToNextLevel = 10;
    public float expGrowth = 1.2f;
    public Slider expGrowthSlider = null;
    public TMP_Text currentLevel;
    public QuestSO QuestOfKilling;
    public static event Action<int> PlayerLevelUp;
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        UpdateExpUI();
    }
    private void OnEnable()
    {
        Enemy_Health.OnMonsterDefeated += GetExperience;//监听敌人是否被击败
    }
    private void OnDisable()
    {
        Enemy_Health.OnMonsterDefeated -= GetExperience;
    }
    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    GetExperience(2);
        //    Debug.Log("经验+2");

        //}
    }
    public void GetExperience(int amount)
    {
        currentExp += amount;
        if(currentExp > ExpToNextLevel )
        {
            LevelUp();
            
        }
        UpdateExpUI();
        if(QuestManager.instance.IsQuestAccepted(QuestOfKilling))
        {
            Debug.Log("检测到敌人死亡，完成战斗任务");
            QuestManager.instance.MakeQuestComplete(QuestOfKilling);
        }
    }
    public void LevelUp()
    {
        Level++;
        currentExp -= ExpToNextLevel;
        ExpToNextLevel = Mathf.RoundToInt(ExpToNextLevel*expGrowth);
        PlayerLevelUp?.Invoke(DataManager.instance.GainSkillPointPerLevel);
        //SkillTreeManager.instance.UpdateSkillPoints(DataManager.instance.GainSkillPointPerLevel);
    }
    public void UpdateExpUI()
    {
        if(expGrowthSlider != null)
        {
            expGrowthSlider.maxValue = ExpToNextLevel;
            expGrowthSlider.value = currentExp;
            currentLevel.text = "Level:" + Level;
        }
    }
    public ExpData GetSaveData()
    {
        ExpData data = new ExpData();
        data.Level = Level;
        data.currentExp = currentExp;
        data.ExpToNextLevel = ExpToNextLevel;
        data.expGrowth = expGrowth;
        return data;
    }
    public void LoadData(ExpData data)
    {
        if (data == null)
        {
            Debug.LogWarning("加载的玩家数据为空！");
            return;
        }

        Level = data.Level;
        currentExp = data.currentExp;
        ExpToNextLevel = data.ExpToNextLevel;
        expGrowth = data.expGrowth;
    }
}
[Serializable]
public class ExpData
{
    public int Level;
    public int currentExp;
    public int ExpToNextLevel;
    public float expGrowth;
}
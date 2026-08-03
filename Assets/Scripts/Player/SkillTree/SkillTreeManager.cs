using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager instance { get; private set; }
    public SkillSlot[] skillSlots;
    public TMP_Text pointsText;
    public int availablePoints;//可用能力点
    public NoticeManager noticeManager;
    //public CanvasGroup SkillCanvasGroup;
    //public bool isSkillUIOpen=false;
    public GameObject ManagersUnderESCControl;
    private void OnEnable()
    {
        SkillSlot.OnSkillPointUsed += HandleSkillPointsUsed;
        SkillSlot.OnSkillMaxed += HandleSkillMaxed;
        ExpManager.PlayerLevelUp += UpdateSkillPoints;
    }
    private void OnDisable()
    {
        SkillSlot.OnSkillPointUsed -= HandleSkillPointsUsed;
        SkillSlot.OnSkillMaxed -= HandleSkillMaxed;
        ExpManager.PlayerLevelUp -= UpdateSkillPoints;
    }
    public bool isSkillUIOpen()
    {
        return ManagersUnderESCControl.activeSelf;
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (SkillSlot slot in skillSlots)
        {
            slot.skillButton.onClick.AddListener(() => CheckAvailablePoints(slot));//为所有技能槽添加按钮检测函数
        }
        UpdateSkillPoints(0);
    }
    public void CheckAvailablePoints(SkillSlot slot)
    {
        if (availablePoints > 0)
        {
            slot.UpgradeSkill();
        }
        else
        {
            noticeManager.ShowNotice("无技能点!");
            //Debug.Log("无技能点！");
        }
    }
    public void UpdateSkillPoints(int amount)
    {
        availablePoints += amount;
        pointsText.text = "技能点数:"+availablePoints.ToString();
    }
    private void HandleSkillPointsUsed(SkillSlot skillSlot)
    {
        if(availablePoints>0)
        {
            UpdateSkillPoints(-1);
        }
        else
        {

        }
    }
    private void HandleSkillMaxed(SkillSlot skillSlot)
    {
        foreach(SkillSlot slot in skillSlots)
        {
            if(!slot.isUnLocked && slot.CanUnlockSkill())
            {
                slot.Unlock();
            }
           
        }
    }
    private void Awake()
    {
        if(instance ==  null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    //private void HandlePlayerLevelUp(int PointGained)
    //{
    //    UpdateSkillPoints(PointGained);
    //    Debug.Log($"获取技能点{PointGained}");
    //}
    // Update is called once per frame
    void Update()
    {
        
    }
}

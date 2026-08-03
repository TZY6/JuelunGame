using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SkillSlot;

public class SkillSlot : MonoBehaviour
{
    public List<SkillSlot> PreRequestSkillSlots;
    public SkillSO skillSO;
    public Image skillIcon;

    public int currentSkillLevel;
    public bool isUnLocked;
    public TMP_Text skillPointText;

    public Button skillButton;

    public static event Action<SkillSlot> OnSkillPointUsed;
    public static event Action<SkillSlot> OnSkillMaxed;
    public void OnValidate()
    {
        if(skillSO != null  )
        {
            
            UpdateUI();
        }
    }
    public void UpdateUI()
    {
        skillIcon.sprite = skillSO.skillIcon;
        if(isUnLocked )
        {
            skillButton.interactable = true;
            skillPointText.text =currentSkillLevel.ToString()+ "/" + skillSO.maxLevel.ToString();
            skillIcon.color = Color.white;
        }
        else
        {
            skillButton.interactable = false;
            skillPointText.text = "锁定";
            skillIcon.color = Color.grey;
        }
    }
    public void UpgradeSkill()
    {
        if (isUnLocked && currentSkillLevel<skillSO.maxLevel)
        {
            currentSkillLevel++;
            Debug.Log("技能" + skillSO.SkillName + "升了1级");
            OnSkillPointUsed?.Invoke(this);
            if(currentSkillLevel>=skillSO.maxLevel)
            {
                OnSkillMaxed?.Invoke(this);
            }
            UpdateUI();//更新UI
        }
        
    }

    public bool CanUnlockSkill()
    {
        foreach(SkillSlot slot in PreRequestSkillSlots)
        {
            if(!slot.isUnLocked || slot.currentSkillLevel<slot.skillSO.maxLevel)
            {
                return false;//并非所有前置技能槽都已解锁或前置技能槽未满级
            }
        }
        return true;//说明前置技能槽均已解锁且满级
    }

    public void Unlock()
    {
        isUnLocked = true;
        UpdateUI();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

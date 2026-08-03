using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance { get; private set; }
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
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        SkillSlot.OnSkillPointUsed += HandleSkillPointUsed;
    }
    private void OnDisable()
    {
        SkillSlot.OnSkillPointUsed -= HandleSkillPointUsed;
    }
    public void HandleSkillPointUsed(SkillSlot slot)
    {
        string skillName = slot.skillSO.SkillName;
        switch (skillName)
        {
            case "最大生命值":
                DataManager.instance.UpdateMaxHealth(1);
                break;
            case "速度":
                DataManager.instance.UpdateSpeed(1);
                break;
            case "攻击力":
                DataManager.instance.UpdateAttackDamage(-1);
                break;
            case "冰系魔法":
                if(!DataManager.instance.isAbleToPlayICE)
                {
                    DataManager.instance.isAbleToPlayICE = true;
                }
                DataManager.instance.UpdateICEDamage(-1);

                break;
            case "火焰魔法":
                if (!DataManager.instance.isAbleToPlayICE)
                {
                    DataManager.instance.isAbleToPlayFIRE = true;
                }
                DataManager.instance.UpdateFIREDamage(-1);
                break;
            case "雷电魔法":
                if (!DataManager.instance.isAbleToPlayICE)
                {
                    DataManager.instance.isAbleToPlayTHUNDER = true;
                }
                DataManager.instance.UpdateTHUNDERDamage(-1);
                break;
            case "冰系魔法范围":
                DataManager.instance.UpdateICEMagicRange(0.5f);
                break;
            case "火焰魔法范围":
                DataManager.instance.UpdateFIREMagicRange(0.5f);
                break;
            case "雷电魔法范围":
                DataManager.instance.UpdateTHUNDERMagicRange(0.5f);
                break;
            case "冰系魔法频率":
                DataManager.instance.UpdateICEDamageInterval(-0.1f);
                break;
            case "火焰魔法频率":
                DataManager.instance.UpdateFIREDamageInterval(-0.1f);
                break;
            case "雷电魔法频率":
                DataManager.instance.UpdateTHUNDERDamageInterval(-0.1f);
                break;


            case "冰系魔法附加效果":
                
                DataManager.instance.UpdateICEeasyTohurt(0.3f);
                break;
            case "火焰魔法附加效果":
                

                DataManager.instance.UpdateFIREburnDamage(-1);
                break;
            case "雷电魔法附加效果":
                

                DataManager.instance.UpdateTHUNDERstunTime(1);
                break;

            default:
                Debug.LogError($"未知技能名称+{skillName}");
                break;
        }
    }
}

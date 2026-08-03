using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance { get; private set; }
    public TextMeshProUGUI HealthText;
    //public static DataManager instance;

    [Header("攻击状态相关")]
    public int damage;
    public float coolDown;
    public float magicCooldown;
    public float AttackRange;
    public float KnockBackForce;
    public float KnockBackTime;
    public float KnockBackStunTime;
    public string CurrentWeapon;

    [Header("弓箭相关")]
    public float ArrowLife;
    public float ArrowSpeed;
    public int ArrowHurt;
    public float ArrowCoolDown = 1f;
    public float ArrowBackForce;
    public float ArrowBackTime;
    public float ArrowBackStunTime;

    [Header("魔法相关")]
    //魔法能力启用
    public bool isAbleToPlayICE;
    public bool isAbleToPlayFIRE;
    public bool isAbleToPlayTHUNDER;
    //伤害间隔
    public float ICEDamageInterval;
    public float FIREDamageInterval;
    public float THUNDERDamageInterval;
    //施法范围
    public float ICEMagicRange;
    public float FIREMagicRange;
    public float THUNDERMagicRange;
    //魔法伤害
    public float ICEAttackDamage;
    public float FIREAttackDamage;
    public float THUNDERAttackDamage;
    //附加效果
    public float ICEeasyTohurt;
    public float FIREburnDamage;
    public float THUNDERstunTime;
    [Header("移动相关")]
    public int speed;

    [Header("生命相关")]
    public int maxHealth;
    public int currentHealth;

    [Header("数据相关")]
    public int GainSkillPointPerLevel;
    public int GoldAmount;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        if(InventoryManager.Instance!=null)
            InventoryManager.Instance.Gold_Text.text = GoldAmount.ToString();
        CurrentWeapon = "初始武器";
        Player_DataUI.instance.UpdateWeapon();
    }
    
    private void OnValidate()
    {
        UpdateMaxHealth();
        UpdateCurrentHealth();
    }
    public void UpdateMaxHealth(int amount=0)
    {
        maxHealth += amount;
        HealthText.text = $"HP: {currentHealth}/{maxHealth}";
    }
    public void UpdateCurrentHealth(int amount=0)
    {
        currentHealth += amount;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        HealthText.text = $"HP: {currentHealth}/{maxHealth}";
    }
    public void UpdateSpeed(int amount)
    {
        speed += amount;

    }
    public void UpdateAttackDamage(int amount)
    {
        damage += amount;

    }
    public void UpdateICEDamage(int amount)
    {
        ICEAttackDamage += amount;
    }
    public void UpdateFIREDamage(int amount)
    {
        FIREAttackDamage += amount;
    }
    public void UpdateTHUNDERDamage(int amount)
    {
        THUNDERAttackDamage += amount;
    }
    public void UpdateICEMagicRange(float amount)
    {
        ICEMagicRange += ICEMagicRange;
    }

    public void UpdateFIREMagicRange(float amount)
    {
        FIREMagicRange += amount;
    }

    public void UpdateTHUNDERMagicRange(float amount)
    {
        THUNDERMagicRange += amount;
    }
    public void UpdateICEDamageInterval(float amount)
    {
        ICEDamageInterval += amount;
    }
    public void UpdateFIREDamageInterval(float amount)
    {
        FIREDamageInterval += amount;
    }
    public void UpdateTHUNDERDamageInterval(float amount)
    {
        THUNDERDamageInterval += amount;
    }

    public void UpdateICEeasyTohurt(float amount)
    {
        ICEeasyTohurt += amount;
    }
    public void UpdateFIREburnDamage(int amount)
    {
        FIREburnDamage += amount;
    }
    public void UpdateTHUNDERstunTime(int amount)
    {
        THUNDERstunTime += amount;
    }
    public void UpdateCurrentWeapon(string weaponName)
    {
        CurrentWeapon = weaponName;
    }
}

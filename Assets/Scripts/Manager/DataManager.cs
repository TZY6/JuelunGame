using System;
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
    public PlayerData GetSaveData()
    {
        PlayerData data = new PlayerData();

        //攻击状态相关
        data.damage = damage;
        data.coolDown = coolDown;
        data.magicCooldown = magicCooldown;
        data.AttackRange = AttackRange;
        data.KnockBackForce = KnockBackForce;
        data.KnockBackTime = KnockBackTime;
        data.KnockBackStunTime = KnockBackStunTime;
        data.CurrentWeapon = CurrentWeapon;


        //弓箭相关
        data.ArrowLife = ArrowLife;
        data.ArrowSpeed = ArrowSpeed;
        data.ArrowHurt = ArrowHurt;
        data.ArrowCoolDown = ArrowCoolDown;
        data.ArrowBackForce = ArrowBackForce;
        data.ArrowBackTime = ArrowBackTime;
        data.ArrowBackStunTime = ArrowBackStunTime;


        //魔法相关
        data.isAbleToPlayICE = isAbleToPlayICE;
        data.isAbleToPlayFIRE = isAbleToPlayFIRE;
        data.isAbleToPlayTHUNDER = isAbleToPlayTHUNDER;

        data.ICEDamageInterval = ICEDamageInterval;
        data.FIREDamageInterval = FIREDamageInterval;
        data.THUNDERDamageInterval = THUNDERDamageInterval;

        data.ICEMagicRange = ICEMagicRange;
        data.FIREMagicRange = FIREMagicRange;
        data.THUNDERMagicRange = THUNDERMagicRange;

        data.ICEAttackDamage = ICEAttackDamage;
        data.FIREAttackDamage = FIREAttackDamage;
        data.THUNDERAttackDamage = THUNDERAttackDamage;

        data.ICEeasyTohurt = ICEeasyTohurt;
        data.FIREburnDamage = FIREburnDamage;
        data.THUNDERstunTime = THUNDERstunTime;


        //移动相关
        data.speed = speed;


        //生命相关
        data.maxHealth = maxHealth;
        data.currentHealth = currentHealth;


        //数据相关
        data.GainSkillPointPerLevel = GainSkillPointPerLevel;
        data.GoldAmount = GoldAmount;
        data.PlayerPositon = PlayerHealth.Instance.gameObject.transform.position;

        return data;
    }
    public void LoadData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("加载的GameSaveData为空！");
            return;
        }


        //攻击状态相关
        damage = data.damage;
        coolDown = data.coolDown;
        magicCooldown = data.magicCooldown;
        AttackRange = data.AttackRange;
        KnockBackForce = data.KnockBackForce;
        KnockBackTime = data.KnockBackTime;
        KnockBackStunTime = data.KnockBackStunTime;
        CurrentWeapon = data.CurrentWeapon;


        //弓箭相关
        ArrowLife = data.ArrowLife;
        ArrowSpeed = data.ArrowSpeed;
        ArrowHurt = data.ArrowHurt;
        ArrowCoolDown = data.ArrowCoolDown;
        ArrowBackForce = data.ArrowBackForce;
        ArrowBackTime = data.ArrowBackTime;
        ArrowBackStunTime = data.ArrowBackStunTime;


        //魔法相关
        isAbleToPlayICE = data.isAbleToPlayICE;
        isAbleToPlayFIRE = data.isAbleToPlayFIRE;
        isAbleToPlayTHUNDER = data.isAbleToPlayTHUNDER;

        ICEDamageInterval = data.ICEDamageInterval;
        FIREDamageInterval = data.FIREDamageInterval;
        THUNDERDamageInterval = data.THUNDERDamageInterval;

        ICEMagicRange = data.ICEMagicRange;
        FIREMagicRange = data.FIREMagicRange;
        THUNDERMagicRange = data.THUNDERMagicRange;

        ICEAttackDamage = data.ICEAttackDamage;
        FIREAttackDamage = data.FIREAttackDamage;
        THUNDERAttackDamage = data.THUNDERAttackDamage;

        ICEeasyTohurt = data.ICEeasyTohurt;
        FIREburnDamage = data.FIREburnDamage;
        THUNDERstunTime = data.THUNDERstunTime;


        //移动相关
        speed = data.speed;


        //生命相关
        maxHealth = data.maxHealth;
        currentHealth = data.currentHealth;


        //数据相关
        GainSkillPointPerLevel = data.GainSkillPointPerLevel;
        GoldAmount = data.GoldAmount;

        //调用初始化脚本
        PlayerHealth.Instance.UpdateHealth();
        PlayerHealth.Instance.gameObject.transform.position = data.PlayerPositon;
    }
}
[Serializable]
public class PlayerData
{
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
    public Vector3 PlayerPositon;
}
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using static Enemy_Status;
//using static UnityEditor.PlayerSettings;

public class Enemy_Health : MonoBehaviour
{
    public int ExpAward = 4;
    public float max_health;
    public float current_Health;
    public float EasyToHurt=1;
    //public ExpManager expManager;
    //public GameObject HurtUICanvas;
    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;

    [SerializeField] private Canvas HurtCanvas;
    [SerializeField] private GameObject HurtPannel;
    [SerializeField] private GameObject HurtTMPro;
    private void OnEnable()
    {
        current_Health = max_health;
        
    }

    public void HealthChange(float health, string damageType = "NORMAL")
    {
        //触发冻伤的逻辑
        if (damageType == "NORMAL" )
        {
            if(GetComponent<Enemy_Status>().enemyState == EnemyStatus.ICE)
            {
                int damage = (int)Mathf.Round(health * EasyToHurt);
                current_Health += damage;
                EnemyHurtUI(damage, damageType);
                Debug.Log($"敌人血量变化:{damage}点，伤害类型:{damageType}，消耗易伤");
                GetComponent<Enemy_Status>().enemyState = EnemyStatus.NORMAL;
                EasyToHurt = 1; // 消耗易伤
            }
            else
            {
                current_Health += health;
                EnemyHurtUI(health, damageType);
                Debug.Log($"敌人血量变化:{health}点，伤害类型:{damageType}");
            }               
        }
        //冰冻伤害，消耗烧伤状态造成两倍伤害
        else if (damageType == "ICE" )
        {
            if(GetComponent<Enemy_Status>().enemyState == EnemyStatus.FIRE)
            {
                current_Health += health * 2; // 其他类型伤害无易伤加成，也不消耗易伤
                EnemyHurtUI(health * 2, damageType);
                Debug.Log($"敌人血量变化:{health * 2}点，伤害类型:{damageType}");
                GetComponent<Enemy_Status>().enemyState = EnemyStatus.NORMAL;
                GetComponent<Enemy_Status>().StopBurn();
            }
            else
            {
                current_Health += health;
                EnemyHurtUI(health, damageType);
                Debug.Log($"敌人血量变化:{health}点，伤害类型:{damageType}");
                GetComponent<Enemy_Status>().enemyState = EnemyStatus.ICE;
            }                            
        }
        //火焰伤害，消耗冻伤状态造成两倍伤害
        else if (damageType == "FIRE")
        {
            if(GetComponent<Enemy_Status>().enemyState == EnemyStatus.ICE)
            {
                current_Health += health * 2; // 其他类型伤害无易伤加成，也不消耗易伤
                EnemyHurtUI(health * 2, damageType);
                Debug.Log($"敌人血量变化:{health * 2}点，伤害类型:{damageType}");
                GetComponent<Enemy_Status>().enemyState = EnemyStatus.NORMAL;
            }
            else
            {
                current_Health += health;
                EnemyHurtUI(health, damageType);
                Debug.Log($"敌人血量变化:{health}点，伤害类型:{damageType}");
                GetComponent<Enemy_Status>().enemyState = EnemyStatus.FIRE;
            }
        }
        //雷电伤害在这里
        else
        {
            current_Health += health; // 其他类型伤害无易伤加成，也不消耗易伤
            EnemyHurtUI(health, damageType);
            //受到雷电伤害后触发麻痹
            GetComponent<Enemy_Movement>().EnemyKonckedBack(transform, 0f, 0f, DataManager.instance.THUNDERstunTime);
            Debug.Log($"敌人血量变化:{health}点，伤害类型:{damageType}");
        }

        //HealthTextAni.Play("TextUI");
        if (current_Health > max_health)
        {
            current_Health = max_health;
        }
        if (current_Health <= 0)
        {
            current_Health = 0;
            EnemyIsDead();

        }
        //EasyToHurt = 1;
    }
    public void EnemyHurtUI(float amount,string damagetype)
    {
        GameObject newHurtUI = Instantiate(HurtTMPro);
        newHurtUI.transform.SetParent(HurtPannel.transform, false);


        TextMeshProUGUI tmp = newHurtUI.GetComponent<TextMeshProUGUI>();
        tmp.text = amount.ToString();
        switch(damagetype)
        {
            case "ICE":
                ColorUtility.TryParseHtmlString("#003DFF", out Color ICEcolor);
                newHurtUI.GetComponent<TextMeshProUGUI>().color = ICEcolor;
                break;
            case "FIRE":
                ColorUtility.TryParseHtmlString("#FF5100", out Color FIREcolor);
                newHurtUI.GetComponent<TextMeshProUGUI>().color = FIREcolor;
                break;
            case "THUNDER":
                ColorUtility.TryParseHtmlString("#D300FF", out Color THUNDERcolor);
                newHurtUI.GetComponent<TextMeshProUGUI>().color = THUNDERcolor;
                break;
            case "NORMAL":
                ColorUtility.TryParseHtmlString("#D4D4D4", out Color NORMALcolor);
                newHurtUI.GetComponent<TextMeshProUGUI>().color = NORMALcolor;
                break;

        }

        Animator anim = newHurtUI.GetComponent<Animator>();
        anim.enabled = false;
        Vector3 enemyWorldPos = transform.position; // 假设此脚本挂在敌人身上
        Debug.Log(enemyWorldPos);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(enemyWorldPos);
        //Vector2 uiPosition = GetScreenPositionForUI(enemyWorldPos);
        newHurtUI.GetComponent<TMP_Text>().rectTransform.position = screenPos + new Vector2(0,160f);

        anim.enabled = true;
        anim.Play("EnemyHurt");  

        //Animator animator = newHurtUI.GetComponent<Animator>();
        //animator.Play("EnemyHurt");
    }
    
    private Vector2 GetScreenPositionForUI(Vector3 worldPosition)
    {
        // 1. 将3D物体的世界坐标转换为屏幕坐标。
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // 2. 根据Canvas模式，将屏幕坐标转换为UI本地坐标。
        if (HurtCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 模式A：屏幕空间 - 覆盖模式
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                HurtCanvas.GetComponent<RectTransform>(),
                screenPoint,
                null,  // 覆盖模式下摄像机为null
                out localPoint
            );
            return localPoint;
        }
        else
        {
            // 模式B & C：屏幕空间 - 摄像机模式 和 世界空间模式
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                HurtCanvas.GetComponent<RectTransform>(),
                screenPoint,
                HurtCanvas.worldCamera, // 使用Canvas绑定的摄像机
                out localPoint
            );
            return localPoint;
        }
    }
    public void EnemyIsDead()//敌人死亡
    {
       
        //expManager.GetExperience(ExpAward);
        OnMonsterDefeated(ExpAward);
        gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        if(HurtCanvas == null)
        {
            HurtCanvas = SetActiveControl.instance.HurtCanvas;
        }
        if(HurtPannel == null)
        {
            HurtPannel = SetActiveControl.instance.HurtPannel;
        }
        //HurtCanvas = Hurt_Canvas.instance.gameObject.GetComponent<Canvas>();
        //HurtPannel = HurtCanvas.transform.GetChild(0).gameObject;
        //HurtTMPro = HurtPannel.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        //HurtUICanvas.transform.localScale = Vector3.one;
    }
}

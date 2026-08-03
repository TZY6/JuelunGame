using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enemy_Status;

public class Player_Combat : MonoBehaviour
{

    public Animator animator;
    //public float cooldown = 2;
    [SerializeField] private float attackTimer;
    [SerializeField] private float magicCooldownTimer;
    // 各魔法光环独立伤害间隔计时器
    [SerializeField] private float iceDamageTimer = 0f;
    [SerializeField] private float fireDamageTimer = 0f;
    [SerializeField] private float thunderDamageTimer = 0f;
    public Transform PlayerAttackPoint;
    //public float PlayerAttackRange;
    public LayerMask EnemyLayer;
    [SerializeField] private bool isICEActive = false;
    [SerializeField] private bool isFIREActive = false;
    [SerializeField] private bool isTHUNDERActive = false;
    [SerializeField] private GameObject attackRangeCircle;
    [SerializeField] private GameObject ICEattackRangeCircle;
    [SerializeField] private GameObject FIREattackRangeCircle;
    [SerializeField] private GameObject THUNDERattackRangeCircle;
    //private WaitForSeconds waitInterval;
    //private Coroutine damageCoroutine;
    
    public void PlayerAttack()
    {
        if(attackTimer <= 0)
        {
            animator.SetBool("isAttacking", true);
            attackTimer = DataManager.instance.coolDown;
        }
        
    }
    public void PlayerMagic(string MagicName)
    {
        if (magicCooldownTimer <= 0)
        {
            animator.SetBool("isAttacking", true);
            animator.SetBool($"is{MagicName}", true);
            
            switch(MagicName)
            {
                case "ICE":
                    isICEActive = true;
                    ICEattackRangeCircle.SetActive(true);
                    ICEattackRangeCircle.transform.localScale = new Vector3(DataManager.instance.ICEMagicRange * 2, DataManager.instance.ICEMagicRange * 2, DataManager.instance.ICEMagicRange * 2);
                    iceDamageTimer = DataManager.instance.ICEDamageInterval;//避免第一次伤害判定延迟
                    break;
                case "FIRE":
                    isFIREActive = true;
                    FIREattackRangeCircle.SetActive(true);
                    FIREattackRangeCircle.transform.localScale = new Vector3(DataManager.instance.FIREMagicRange*2, DataManager.instance.FIREMagicRange * 2, DataManager.instance.FIREMagicRange * 2);
                    fireDamageTimer = DataManager.instance.FIREDamageInterval;
                    break;
                case "THUNDER":
                    isTHUNDERActive = true;
                    THUNDERattackRangeCircle.SetActive(true);
                    THUNDERattackRangeCircle.transform.localScale = new Vector3(DataManager.instance.THUNDERMagicRange * 2, DataManager.instance.THUNDERMagicRange * 2, DataManager.instance.THUNDERMagicRange * 2);
                    thunderDamageTimer = DataManager.instance.THUNDERDamageInterval;
                    break;
            }
            magicCooldownTimer = DataManager.instance.magicCooldown;
        }

    }

    public void PlayerDamageDeal()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(PlayerAttackPoint.position, DataManager.instance.AttackRange, EnemyLayer);
        if (enemies.Length > 0)
        {
            //对敌人造成击退
            enemies[0].GetComponent<Enemy_Movement>().EnemyKonckedBack(transform, DataManager.instance.KnockBackForce, DataManager.instance.KnockBackTime, DataManager.instance.KnockBackStunTime);
            //对敌人造成伤害
            enemies[0].GetComponent<Enemy_Health>().HealthChange(DataManager.instance.damage);
            Debug.Log("对敌人造成伤害");
        }
    }
    public void EndPlayerAttack()//结束攻击
    {
        animator.SetBool("isAttacking", false);
    }
    public void EndPlayerMagic(string MagicName)
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool($"is{MagicName}", false);
        switch (MagicName)
        {
            case "ICE":
                isICEActive = false;
                ICEattackRangeCircle.SetActive(false);
                break;
            case "FIRE":
                isFIREActive = false;
                FIREattackRangeCircle.SetActive(false);
                break;
            case "THUNDER":
                isTHUNDERActive = false;
                THUNDERattackRangeCircle.SetActive(false);
                break;
        }
    }
        
    // Start is called before the first frame update
    void Start()
    {
        // 初始化攻击范围指示器大小
        if (attackRangeCircle != null)
        {
            attackRangeCircle.transform.localScale = new Vector3(DataManager.instance.AttackRange * 2, DataManager.instance.AttackRange * 2, DataManager.instance.AttackRange * 2);
        }
    }
    //检索魔法范围内的敌人层级物体，并获取对方组件进行伤害以及状态赋值
    public void ApplyFrostDamageInICE()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(gameObject.transform.position, DataManager.instance.ICEMagicRange, EnemyLayer);
        Debug.Log($"检测到敌人数量+{enemies.Length}");
        foreach(var enemy in enemies)
        {
            //施加冻伤效果
            //enemy.GetComponent<Enemy_Status>().enemyState = EnemyStatus.ICE;
            //易损
            enemy.GetComponent<Enemy_Health>().EasyToHurt = DataManager.instance.ICEeasyTohurt;
            //施加冰系伤害
            enemy.GetComponent<Enemy_Health>().HealthChange(DataManager.instance.ICEAttackDamage, "ICE");
        }
        
    }
    public void ApplyFrostDamageInFIRE()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(gameObject.transform.position, DataManager.instance.FIREMagicRange, EnemyLayer);
        //enemies[0].transform.parent.GetComponent<>
        Debug.Log($"检测到敌人数量+{enemies.Length}");
        foreach (var enemy in enemies)
        {
            //施加烧伤效果
            //enemy.GetComponent<Enemy_Status>().enemyState = EnemyStatus.FIRE;
            enemy.GetComponent<Enemy_Status>().StartBurn();
            //施加火系伤害
            enemy.GetComponent<Enemy_Health>().HealthChange(DataManager.instance.FIREAttackDamage, "FIRE");
        }
    }
    public void ApplyFrostDamageInTHUNDER()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(gameObject.transform.position, DataManager.instance.THUNDERMagicRange, EnemyLayer);
        //enemies[0].transform.parent.GetComponent<>
        foreach(var enemy in enemies)
        {
            //施加麻痹效果
            //enemy.GetComponent<Enemy_Movement>().EnemyKonckedBack(transform, 0f,0f,3f);
            //施加雷系伤害
            enemy.GetComponent<Enemy_Health>().HealthChange(DataManager.instance.FIREAttackDamage, "THUNDER");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(attackTimer > 0)
        {
            attackTimer -= Time.deltaTime; 
        }
        if (magicCooldownTimer > 0)
        {
            magicCooldownTimer -= Time.deltaTime;
        }
        
        if (animator.GetBool("isAttacking"))
        {
            PlayerMovement.Instance.enabled = false;
            PlayerMovement.Instance.rb.velocity = Vector2.zero;//攻击过程中霸体
        }
        else
        {
            PlayerMovement.Instance.enabled = true;
        }
        if (isICEActive)
        {
            // 累加时间
            iceDamageTimer += Time.deltaTime;
            if (iceDamageTimer >= DataManager.instance.ICEDamageInterval)
            {
                // 时间到，执行伤害
                ApplyFrostDamageInICE();
                // 重置计时器（保留多余的时间余量，可根据需要决定是否回减）
                iceDamageTimer = 0f;
            }
        }
        else
        {
            // 未激活时重置计时器，确保下次激活时从0开始计时
            iceDamageTimer = 0f;
        }
        if (isFIREActive)
        {
            // 累加时间
            fireDamageTimer += Time.deltaTime;
            if (fireDamageTimer >= DataManager.instance.FIREDamageInterval)
            {
                // 时间到，执行伤害
                ApplyFrostDamageInFIRE();
                // 重置计时器（保留多余的时间余量，可根据需要决定是否回减）
                fireDamageTimer = 0f;
            }
        }
        else
        {
            // 未激活时重置计时器，确保下次激活时从0开始计时
            fireDamageTimer = 0f;
        }
        if (isTHUNDERActive)
        {
            // 累加时间
            thunderDamageTimer += Time.deltaTime;
            if (thunderDamageTimer >= DataManager.instance.ICEDamageInterval)
            {
                // 时间到，执行伤害
                ApplyFrostDamageInTHUNDER();
                // 重置计时器（保留多余的时间余量，可根据需要决定是否回减）
                thunderDamageTimer = 0f;
            }
        }
        else
        {
            // 未激活时重置计时器，确保下次激活时从0开始计时
            thunderDamageTimer = 0f;
        }
    }
    private void Awake()
    {
        
    }

}

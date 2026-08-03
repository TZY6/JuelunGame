using System.Collections;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Rigidbody2D rbCollision;
    [SerializeField] private Animator enemyAnimator;

    [Header("Movement Settings")]
     public float currentSpeed = 3f;
    public float initialSpeed;
    //[SerializeField] private float deadZone = 0.5f;      // 原代码未使用，保留以备后续

    [Header("Attack Settings")]
    public float attackRange = 2f;     // 攻击范围
    [SerializeField] private GameObject attackRangeCircle; // 可视化范围指示器

    [Header("State")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle; // 当前状态
    private Enemy_Combat Enemy_combat;
    [SerializeField]
    private GameObject AttackPoint;
    [SerializeField]
    private GameObject AboutAttack;
    // 私有变量
    [SerializeField] private GameObject player;          // 玩家对象引用
    private int currentFace = 1;        // 当前面向方向：1 右，-1 左
    private Vector2 moveDirection;      // 移动方向（归一化）
    private bool isKnocked = false;
    //[SerializeField]
    //private Rigidbody2D Enemyrb;
    // 属性：只读状态，供外部查询
    public EnemyState State => currentState;

    public void EnemyKonckedBack(Transform player, float force, float knockbackTime ,float stunTime)
    {
        isKnocked = true;
         Vector2 direction = (transform.position - player.position).normalized;
         rb.velocity = direction * force;
        Debug.Log("敌人被击退");
        StartCoroutine(EnemyStun(knockbackTime,stunTime));
    }
    IEnumerator EnemyStun(float knockbackTime, float stuntime)
    {
        yield return new WaitForSeconds(knockbackTime);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stuntime);
        isKnocked = false;
    }
    // 初始化
    private void Start()
    {
        // 获取组件（防止未赋值）
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if(rbCollision == null) rbCollision = GetComponentInChildren<Rigidbody2D>();
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
        Enemy_combat = AboutAttack.GetComponent<Enemy_Combat>();
        // 初始化攻击范围指示器大小
        if (attackRangeCircle != null)
        {
            attackRangeCircle.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, attackRange * 2);
        }
    }
    public void EnemyConbat()
    {
        if(Vector2.Distance(AttackPoint.transform.position,player.transform.position) < attackRange)
        {
            Enemy_combat.Attack();
        }
        
    }
    private void Update()
    {
        // 如果没有玩家目标，不进行任何逻辑
        if (player == null) return;
        //如果敌人处于被击退状态，不进行任何逻辑
        //if (isKnocked) return;
        if (!isKnocked)
        {
            // 1. 计算移动方向（归一化）
            moveDirection = (player.transform.position - transform.position).normalized;

            // 2. 根据与玩家的距离决定是否进入攻击状态（攻击状态优先级最高）
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            //Debug.Log(distanceToPlayer);
            if (distanceToPlayer <= attackRange)
            {
                // 攻击范围内且尚未处于攻击状态，则切换至攻击状态
                if (currentState != EnemyState.Attack)
                {
                    ChangeStateTo(EnemyState.Attack);
                }
            }
            else
            {
                // 超出攻击范围，如果当前是攻击状态则退出攻击
                if (currentState == EnemyState.Attack)
                {
                    // 注意：原逻辑中攻击状态结束后立即根据 isChasing 决定下一步
                    // 此处我们保持原行为：攻击状态结束后，如果仍在追逐范围内（由触发器控制），则应转为追逐
                    // 原代码在 Update 中攻击状态退出后直接判断 isChasing，这里我们模拟该逻辑
                    if (IsPlayerInChaseTrigger()) // 需要判断是否仍处于追逐触发区域
                    {
                        ChangeStateTo(EnemyState.Chasing);
                    }
                    else
                    {
                        ChangeStateTo(EnemyState.Idle);
                    }
                }
                // 如果当前不是攻击状态，则状态由触发器控制，Update 中不再改变状态
            }

            // 3. 根据当前状态执行对应行为
            switch (currentState)
            {
                case EnemyState.Chasing:
                    Chase();
                    break;
                case EnemyState.Attack:
                    // 攻击状态下不移动，动画由状态切换时设置
                    break;
                case EnemyState.Idle:
                    // 空闲状态下不移动
                    rb.velocity = Vector2.zero;
                    break;
            }

        }


    }

    // 追逐移动逻辑
    private void Chase()
    {
        
            // 原 Chase 方法直接设置速度，现保持相同行为
            rb.velocity = moveDirection * currentSpeed;
            // 4. 处理面向方向（始终根据 moveDirection 更新）
            UpdateFacingDirection();
        
        
    }

    // 更新角色面向方向
    private void UpdateFacingDirection()
    {
        if (moveDirection.x == 0) return; // 无水平移动时不改变方向

        if (moveDirection.x < 0 && currentFace == 1)
        {
            // 需要转向左
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            currentFace = -1;
        }
        else if (moveDirection.x > 0 && currentFace == -1)
        {
            // 需要转向右
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            currentFace = 1;
        }
    }

    // 统一的状态切换方法
    private void ChangeStateTo(EnemyState newState)
    {
        if (currentState == newState) return; // 状态未变，无需重复处理

        // 退出当前状态（如需）
        // 目前没有需要额外处理的退出逻辑，但可以预留扩展点

        // 更新状态变量
        currentState = newState;

        // 根据新状态配置动画、移动等
        switch (newState)
        {
            case EnemyState.Chasing:
                enemyAnimator.SetBool("isChasing", true);
                enemyAnimator.SetBool("isAttacking", false);
                // 注意：追逐时速度由 Chase 方法每帧设置，不需要在此重置
                break;

            case EnemyState.Idle:
                enemyAnimator.SetBool("isChasing", false);
                enemyAnimator.SetBool("isAttacking", false);
                rb.velocity = Vector2.zero;
                break;

            case EnemyState.Attack:
                enemyAnimator.SetBool("isAttacking", true);
                enemyAnimator.SetBool("isChasing", false);
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // 辅助方法：判断玩家是否仍在追逐触发范围内（原逻辑由触发器管理）
    // 原脚本通过 OnTriggerEnter/Exit 控制 isChasing，现在保留类似逻辑
    // 我们需要维护一个玩家是否在触发区域内的标志，以替代原来的 isChasing
    private bool playerInChaseTrigger = false;

    public void OnTriggerEnterDIY(Collider2D collision, GameObject PLAYER, bool playerInChase)
    {
        if (collision.CompareTag("Player"))
        {
            player = PLAYER;
            playerInChaseTrigger = playerInChase;

            // 如果当前是空闲状态，则开始追逐
            if (currentState == EnemyState.Idle)
            {
                ChangeStateTo(EnemyState.Chasing);
                Debug.Log("开始碰撞");
            }
        }
    }

    public void OnTriggerExitDIY(Collider2D collision, bool playerInChase)
    {
        if (collision.CompareTag("Player"))
        {
            playerInChaseTrigger = playerInChase;

            // 如果当前是追逐状态且玩家离开触发区域，则转为空闲
            if (currentState == EnemyState.Chasing)
            {
                ChangeStateTo(EnemyState.Idle);
                Debug.Log("退出碰撞");
            }
        }
    }

    // 公共方法，供外部调用退出追逐（原 ExitChasing 方法）
    // 注意：原 ExitChasing 不仅停止追逐，还将状态设为 Idle，并停止速度
    public void ExitChasing()
    {
        if (currentState == EnemyState.Chasing)
        {
            ChangeStateTo(EnemyState.Idle);
        }
    }

    // 辅助方法：判断玩家是否仍在追逐触发区域内（用于攻击状态退出后的判断）
    private bool IsPlayerInChaseTrigger()
    {
        return playerInChaseTrigger;
    }
}

// 状态枚举保持不变
public enum EnemyState
{
    Chasing,
    Idle,
    Attack
}
# BetterDesign 架构与验收记录

## 范围
改动仅在 Assets/BetterDesign。原 RPG1、原 Manager、原存档文件和资源均保留。该场景是可单独体验的迁移样板，不是原游戏全部内容的自动迁移；原游戏的装备、完整技能树、对话树和地图进度需要逐项接入，不能把新场景通过测试等同于原游戏所有功能通过回归。

## 原有架构审查
以下为代码审查发现，不是 Profiler 测量结果。

| 现有架构 / 问题 | 风险 | 设计原则及处理 | 是否值得重构 |
| --- | --- | --- | --- |
| QuestManager.UpdateObjProgress 对尚未访问/交谈的目标走完成分支 | P0 | 条件显式判断；未知条件默认失败。BetterDesign 使用 ConditionEvaluator 与稳定 ID 的 WorldFacts | 是；原文件在边界外，仍需后续迁移 |
| QuestManager.CompleteQuest 缺少领取资格与幂等检查 | P0 | 运行时模型校验接取、完成、容量、领取状态，一次提交奖励 | 是 |
| Loot 初始化不可拾取，TriggerExit 条件又要求已经可拾取；业务依赖拾取通知监听器 | P0 | LootController 拥有拾取事务，成功才移除；事件只通知已发生事实 | 是 |
| PlayerHealth.OnEnable 检测 Instance 时未排除自身；重复启用会销毁自己 | P0 | SceneContext 直接引用；角色无需全局唯一 | 是 |
| SaveManager 未覆盖技能等级/解锁、Buff 剩余时间、场景身份；写入/载入缺少完整事务 | P0 | 静态配置/运行时/DTO 分离，快照校验、校验和、原子替换、备份 | 是；新存档独立，原存档不覆盖 |
| NPC_Walkaround 在运行时脚本无条件 using UnityEditor | P0 | Editor/Runtime 编译边界 | 是；原文件未改，整个原项目 Player 构建仍有风险 |
| TimeScaleManager 计数可因重复 Close 递减至负数 | P0 | 新场景 PanelStack 的模态状态单一拥有者，不依赖全局 timeScale | 是 |
| Player_Combat 与 Player_Bow 根据 Animator 同时开关 PlayerMovement | P1 | 逻辑状态拥有操作权；Locomotion 与 Action 分离，Animator 仅表现 | 是 |
| DataManager.Awake 直接依赖 Player_DataUI.instance，未建立初始化顺序 | P1 | DemoScene 显式构造/绑定，销毁时逆序释放 | 是 |
| ShopManager.SetChangeButton 重复注册按钮委托 | P1 | 场景序列化按钮命令；订阅句柄由生命周期拥有者释放 | 是 |
| InventorySlot/SkillSlot 持有背包数量和技能等级 | P1 | InventoryModel/ProgressionModel 拥有数据，UI 读取；存档不读取 TMP 或动画值 | 是 |
| ExpManager 使用 > 且一次只升一级，普通经验获取混入击杀任务推进 | P1 | >= 循环处理等级，击杀/经验/任务为不同事实 | 是 |
| 原箭矢、伤害文字、掉落和通知频繁创建销毁 | P1 | 有界通用池，预配置对象重用，溢出策略显式 | 是；测量后再扩张规模 |
| Enemy_Status/NPC_Wander 的协程与复用状态未统一取消/重置 | P1 | 新场景采用拥有者 Tick 时间线，退出/受伤/回收取消未结算操作 | 是 |
| 原战斗有负数伤害约定和雷/冰/火字段混用 | P1 | 新流程正数 DamageContext，计算和表现分离；不能对旧流程直接统一改符号 | 是 |
| 静态 ItemDatabase/QuestDatabase 等配置查找入口 | P2 | 只读配置目录可以共享；不应持有玩家会话状态 | 可保留 |
| 小规模枚举、12 格背包与 16 名敌人的线性扫描 | P2 | 当前规模简单、可追踪；不引入反射注册、虚拟列表和空间树 | 保留，按测量扩展 |
| 项目未使用 DOTween/UniTask | P2 | 不为架构标签额外引入包 | 保留 |

原项目的全局入口包括 Hurt_Canvas、BagManager、DataManager、DialogueManager、ExpManager、GameBootstrap、GameManager、InventoryManager、ItemDatabase、MenuManager、NoticeManager、QuestManager、SaveManager、ShopManager、TimeScaleManager、DialogueHistoryTracker、NPCDatabase、LocationHistoryTracker、LOCATIONDatabase、Player_DataUI、PlayerHealth、PlayerMovement、SkillManager、SkillTreeManager、GetNewQuest、QuestDatabase、QuestLogUI、SceneControl、SetActiveControl。应按配置目录、应用级服务、会话数据、场景组件四种生命周期判断，而不是统一删除 Instance。

## 实际目标结构
```text
DemoScene（场景组合根，创建/绑定/销毁）
 ├─ PlayerInputReader → ActorController → ActorStateMachine
 │                          ├─ ActorModel（位置、朝向、生命）
 │                          ├─ AttackExecutor → IActorWorld → CombatSystem → IDamageable
 │                          └─ ActorView → Animator Speed BlendTree / CrossFade
 ├─ DemoWorld（世界用例与场景协调；IActorWorld、IInventoryActions）
 │   ├─ GameModel → InventoryModel / ProgressionModel / WorldFacts
 │   │               └─ ConditionEvaluator（无 Unity 依赖）
 │   ├─ CombatSystem → FixedPool（已布置的投射物/特效槽）
 │   ├─ LootController → FixedPool（已布置的掉落槽）
 │   └─ CheckpointCoordinator → ISaveStore → JsonSaveStore
 └─ InventoryController → IInventoryView → InventoryView
     DemoHud ← 场景拥有的类型安全 GameEvents
```

- 配置拥有者：Sanctuary.asset。AttackDefinition 保存时序、移动倍率、攻击类别，SO 不保存冷却、等级、背包或血量。
- 数据修改者：角色控制器修改 ActorModel；游戏用例通过 GameModel 修改背包、金币、成长和任务；UI 仅发命令。
- 事件发送者：GameModel 提交数据后发送 EnemyKilled、ItemCollected、GoldChanged、InventoryChanged、PlayerLevelUp、QuestProgressChanged。CombatEvent 是命中事实。Notice 是显示文字的专用通道，不作为业务数据载体。
- 事件监听者：HUD、InventoryController。模型的关键事务直接调用并返回结果，不靠监听器执行奖励或扣款。没有静态 EventBus。
- 生命周期：DemoScene 拥有世界及 Controller；UI Disable 退订、Enable 刷新并重新订阅；Dispose 幂等。订阅新增于本次 Publish 后才生效；退订立即失效。监听器异常不跳过后续监听器；运行时报告给 Unity Console。
- 存档拥有者：CheckpointCoordinator 捕获模型和世界快照；载入前完整校验，角色/敌人/掉落均恢复后才发布 Restored。暂停、冷却、攻击时钟不持久化，载入回到稳定可操作状态。
- 不建立 ServiceLocator：目前只有一个场景作用域，构造注入已经能暴露依赖。未来应用级 Save/SceneFlow 可以由 AppRoot 拥有，但不能把当前 DemoWorld 升级成静态全局对象。

## 玩家与战斗决策
Locomotion 记录 Idle/Move；Action 记录 Attack/Skill/Dash/Hurt/Dead/Interact。状态有进入、退出、Tick、转换规则及 ActionVersion。移动不会覆盖正在执行的动作；近战移动倍率为 0，弓与法术为 0.65。受伤取消未命中的攻击/法术，死亡是终态；冲刺无敌期间不接受普通受伤。时序使用逻辑时钟，Animator 或 Animation Event 不拥有业务状态。投射物命中、近战命中都进入 ReceiveDamage → DamageCalculator。

选择紧凑的显式转换规则，而不是每个枚举值创建一个只转发调用的类。动作差异由 AttackDefinition + AttackExecutor 表达；当出现多阶段蓄力、连招或可嵌套控制权时，再把对应动作拆成状态对象。像素角色当前是单 SpriteRenderer，Avatar Mask 无法凭空得到上半身独立骨骼；使用 Speed BlendTree 和动作 CrossFade，后续分层动画需要相应美术资源。

R 切换剑/弓；空格/左键普通攻击；Q/右键法术；Shift 冲刺。每名敌人提供 10 经验，精确达到门槛即可升级，超额经验允许连续升级。K 消耗技能点升级法术，最高 3 阶，每阶伤害 +5。装备选择、等级、经验、技能点、法术阶数均保存。

## 条件与数据扩展
ConditionEvaluator 对 IConditionFacts 查询；同一 ConditionConfig 可供任务、功能/技能解锁、成就或对话准入使用。WorldFacts 用稳定字符串 ID 存储已到达地点、已完成对话；背包条件查询当前数量，不能与历史拾取数量混用。契约采用接取时的击杀基线，领取奖励幂等；未知地点/对话/条件不会默认通过。现有 4 种物品保持小枚举，超过当前规模再迁移配置目录，避免预先制造反射 Registry。

## 池与资源重置
FixedPool 提供 TryRent/Return/ReturnAll、初始可用容量、倍增到硬上限、Active/Peak/Misses。所有 GameObject 已在场景内，扩容只启用预布置储备，不会运行时创建对象。重复注册、外来归还、重复归还均检查；租出回调失败会回滚计数。
- ActorView 重置 Animator、闪白、颜色、翻转、排序、缩放；ActorController.Reset 重置 HP、位置、状态、无敌和冷却。
- 特效重置速度、寿命、投射物标记、伤害、颜色、缩放；每次使用明确写入位置。
- 掉落重置物品数据、拾取延迟和显示状态。
- 当前池对象没有 Rigidbody/Collider/ParticleSystem/TrailRenderer/DOTween/Buff/Target 订阅。以后添加此类组件时，必须由对应适配器明确缓存并重置，不能声称 SetActive(false) 自动清空这些状态。
- 特效池满：丢弃表现；投射物池满：显式失败；掉落池满：波次/丢弃命令提前预留容量，不丢奖励。

## UI 与异步
背包 12 个原生序列化格子复用，无需虚拟列表。Controller 关闭时退订，打开时完整读取模型；View 缓存每格内容，只改变化格子。金币保存在模型并立即提交；血条显示缓动不改变真实 HP。HUD 诊断仅 4 Hz 格式化文字。Init/Bind → Open/Enable → Refresh → Close/Disable → Dispose 均有明确拥有者。

目前不使用后台任务、Coroutine 或 DOTween 执行核心玩法；攻击、冷却、闪白由场景 Tick 驱动，停止 Tick 就停止推进。存档体积有限且同步原子写入，可能存在磁盘抖动，需在目标设备测量；若改后台 I/O，先在主线程捕获 DTO，只在工作线程编码/写文件，通过会话取消令牌防止已关闭场景收到结果，不能在工作线程调用 Unity API。

## 性能验证与代价
| 问题来源 | Profiler 验证 | 修改方案 | 修改后检查 |
| --- | --- | --- | --- |
| 重复 Instantiate/Destroy | CPU Timeline 搜索 Instantiate/Destroy、GC.Alloc，连续 10 波 | 场景对象池与硬容量 | 无新增场景对象；Peak/Misses 可见；池满奖励不丢 |
| UI 全格刷新/无关血量事件刷新背包 | UI 模块、Canvas.BuildBatch、GC.Alloc | 专用 InventoryChanged、关闭退订、内容缓存 | 连续受伤不调用背包 Render；单格变化仅更新相关内容 |
| Update 中查找/临时容器 | Deep Profile 定位 Find/GetComponent/LINQ/闭包 | 组合根缓存、数组循环 | 游戏热路径无场景查找与集合分配；诊断文字采样单独统计 |
| 物理查询 | Physics2D Queries/CPU | 当前小地图模型坐标+16 目标扫描，无每帧 Physics 查询 | 不宣称有 NonAlloc 优化收益；接入墙体/碰撞后测量 LayerMask 与频率 |
| 存档顿帧 | Timeline 中磁盘写入，低速盘反复 F5 | 小 DTO、原子替换、备份；不要逐帧保存 | 记录最慢保存耗时；必要时再引入取消式后台写入 |
| 长会话订阅与复用残留 | Memory Profiler、反复开关背包/载入/重开 | 句柄退订与池回调重置 | ListenerCount 不增长；池 Active 回归；无旧颜色/技能/目标 |

不提供虚构的毫秒或 GC 提升百分比。需要在真实 Player、关闭 Deep Profile 后建立改前改后同一输入脚本、同波次数的基线。代价包括池预留的常驻内存、显式事件类型维护、快照版本迁移成本；当前枚举/固定地图适用于样板规模，不能直接推断海量实体性能。

## 渐进式迁移顺序
1. 以新场景回归为基线接入 Player FSM，保留旧场景入口。
2. 将击杀、拾取、金币、升级等多消费者通知接入会话事件；奖励仍由用例提交。
3. 每次替换一个原 Singleton 的调用方，应用级服务保留唯一实例，角色/UI改显式引用。
4. 迁移近战、弓、魔法到统一伤害/时序接口；核对旧负伤害约定。
5. 分对象类型迁移池，并验证特效、物理和取消清理。
6. 将原任务条件映射到稳定 ID 和 WorldFacts，旧任务逐项验证。
7. 把原装备/技能树/对话 UI 的真实数据搬入运行时模型，再迁移 Presenter。
8. 对完整旧存档建立版本化迁移和失败回滚后，才替换原 SaveManager。

## 验收
打开 Scenes/BetterDesignDemo.unity，运行 Tools/BetterDesign/Validate Assets 与 Test Runner 中 ArchitectureTests。验收范围包括静态引用/字体、模型事务、状态中断、订阅生命周期、池容量、条件、成长、存档版本及场景世界启动/移动/射击/保存/恢复。自动化编译与测试结果见 VALIDATION.md；人工画面、手感和完整旧 RPG1 回归应单独记录。

### 首次导入兼容
在 Unity 2022.3.29f1 的独立验证项目中发现首次导入可能缓存 MonoScript.GetClass() 为空，尽管程序集已编译。DemoImportRepair 在编译完成后核对 9 个序列化脚本的类型绑定，仅对不匹配的脚本强制重导入，不重建 GameObject、不改 GUID、不覆盖场景。Validate Assets 同样执行此检查；必要时可手动运行 Tools/BetterDesign/Repair Script Imports。该检查解决导入缓存，不用于掩盖编译错误或丢失的资源。

## 对战修复后的补充
玩家鼠标瞄准通过 IAimedPlayerInput 独立于移动输入；连续攻击/法术配合 180 ms 缓冲，闪避可取消未结算动作。投射物采用线段扫掠，敌人使用有限邻居分离。ActorView 的血条、攻击/瞄准指示与弓手 Animator 均来自场景序列化引用。输入和 HUD 引用本场景 EventSystem，模态关闭清理选择，避免原 Submit/空格冲突。SceneFontFallback 通过可释放的 TMP 字体请求回调和文本预处理补齐主字体缺字，不修改共享字体资产的配置；所有字体资源仍为静态配置。详见 BATTLE_ACCEPTANCE.md。

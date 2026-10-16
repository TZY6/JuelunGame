# 验证记录
日期：2026-10-16。基线：21e467ed7cb919f7c069c67c679d8c19a9451767。目标分支：codex/better-design-sanctuary。

## 已执行
- 使用本机 Unity 2022.3.29f1 的 Roslyn、Mono/.NET 参考程序集编译 BetterDesign.Core、Runtime、Editor、Tests：4 个程序集均通过。
- 在系统临时目录创建独立 Unity 项目，只复制 BetterDesign 及引用资源；包使用本机现有缓存。没有编辑、切换或推送用户当前工作副本。
- Unity BatchMode / NoGraphics / EditMode，过滤 BetterDesign.Tests.ArchitectureTests：**42 total / 42 passed / 0 failed**。
- 其中 UnityTest 实际进入和退出 Play Mode，检查 DemoScene 初始化、背包模态开关、UI Disable/Enable 后重新绑定与场景退出清理。
- 场景集成测试使用已布置的组件启动 DemoWorld，验证移动、移动中射箭、保存、恢复位置/武器及存活敌人数。
- 资源校验覆盖 17 个角色、48 个特效槽、64 个掉落槽、12 个格子、22 个序列化按钮、49 个 TMP 标签及指定字体。Runtime 源码检查禁止运行时创建 GameObject/组件/纹理。
- 模型测试覆盖原子拾取/满包、奖励容量与幂等、任务接取基线、精确/多级升级、技能点消费、世界事实、状态中断、订阅重入/异常/释放、池上限/重复登记/外来归还。
- 实际 Unity JsonUtility + 文件 I/O 测试覆盖存档往返、损坏主档恢复备份、非法数据不修改模型。测试使用独立临时存档目录，不覆盖用户检查点。

## 测试中发现并处理
1. 首次导入脚本编译成功，但部分 MonoScript 类型绑定为空：增加针对性导入校验与重新导入入口；没有修改 GUID 或动态补建组件。
2. 独立验证项目缺少 TMP Settings 和 shader include：补齐临时项目中的原有 TMP Essential Resources 后重跑。仓库外资源原本存在，未修改这些文件。
3. 背包数据事件原先包含金币和任务等无关变更：通过 InventoryModel.Revision 只在背包实际变化或整体恢复时发布 InventoryChanged；View 继续按格缓存。

最终结果来自修复后的完整测试轮次，没有把跳过测试或仅编译视为通过。

## 尚未完成的验收
- NoGraphics 自动化不替代画面、美术、手感与目标设备性能验收。没有宣称 FPS、GC 字节数或耗时提升百分比。
- 没有构建整个原项目的 Player；原 NPC_Walkaround 的无条件 UnityEditor 引用等目录外 P0 风险仍在，详见 ARCHITECTURE.md。
- 没有把原 RPG1 的全部装备、技能树 UI、任务/对话内容、Buff 和跨地图进度搬入新场景。原场景与代码保持原状，不宣称其全部功能已经完成重构回归。
- 当前为有界平面战斗样板；碰撞地图、完整角色动画分层、联网、大规模实体等须按实际需求继续接入，不以空接口宣称实现。

## 本地复验
1. 在当前分支执行 git pull --ff-only，等待 Unity 导入。
2. Tools → BetterDesign → Validate Assets；若导入异常，执行 Repair Script Imports。
3. Test Runner → EditMode → BetterDesign.Tests.ArchitectureTests → Run All。
4. 打开 Assets/BetterDesign/Scenes/BetterDesignDemo.unity，Play；WASD 移动，R 切剑弓，Space 攻击，Q 法术，Shift 冲刺，K 升级法术，E 契约，Tab 背包，F1 架构面板，F5/F9 存读档。
5. 按 ARCHITECTURE.md 的 Profiler 表建立目标设备基线，再判断是否需要增加池容量、异步存档或空间查询结构。

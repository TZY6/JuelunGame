# 当前验证结果
朝向与拖拽修复基线：9b87625313e95ff6e4ed948e2caec9b0f6374bd9。Unity 2022.3.29f1 / TMP 3.0.6。

- BetterDesign.Core、Runtime、Editor、Tests 四个程序集 Roslyn 编译通过。
- 独立临时 Unity 项目中执行 BetterDesign.Tests：**64 / 64 通过，0 失败**。包含进入 Play Mode、Awake、UI Disable/Enable、存档 JSON/文件恢复及 12 项对战验收测试。
- 新增朝向、拖拽与存档格子回归；Play Mode 检查动画更新后朝向。UGUI 事件测试覆盖移动、关闭、数据变化、拖出取消；模型测试覆盖合并上限、交换及失败原子性。
- 近战、弓箭、法术分别通过整波清场模拟，敌人 AI 正常运行；远程测试允许合法消耗初始治疗补给。
- 额外进入 Play Mode，以真实 EventSystem.RaycastAll 命中第 6 格并通过 ExecuteHierarchy 投放第 1 格补给，验证 0 → 5 的模型移动及拖拽图标关闭。该探针在临时项目运行，不是操作系统鼠标回放。
- 场景有 629 个预置 GameObject（新增 1 个无射线拖拽图标），12 个格子各绑定 InventorySlotDrag。
- 场景验证 17 个角色、48 FX、64 掉落、12 格背包、49 TMP 标签、22 按钮，以及血条/攻击方向/瞄准/弓手动画/字体回退/相机/EventSystem 引用。
- 字体验证覆盖实际 TMP 字符输出，而不仅是资源引用。原 7000 主字体及目录外的字体资源、TMP 配置没有修改。
- 有图形设备的 Unity 相机渲染检查了真实 HUD、字形、血条和战区；架构面板默认隐藏。
- 交付场景使用 Git blob 校验，与独立验证项目场景一致。代码通过 GitHub API 提交；用户工作副本未编辑。

测试输入通过 IPlayerInput/IAimedPlayerInput 驱动真实场景对象与玩法模块，不宣称完成操作系统层键鼠回放或人工试玩。没有提供未测量的 FPS/GC 提升数字；原 RPG1 的目录外缺陷与整体 Player 构建未在本轮改动。

具体复现、修复和本地玩家验收路径见 [BATTLE_ACCEPTANCE.md](BATTLE_ACCEPTANCE.md)。架构审查和迁移边界见 [ARCHITECTURE.md](ARCHITECTURE.md)。
